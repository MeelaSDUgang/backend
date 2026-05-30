using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GatewayApi.Services.AntiFraud;

public class GptAntiFraudService : IAntiFraudService
{
    private const string SystemPrompt = """
                                        You are an AI anti-fraud engine for a payment gateway (PISP).
                                        Your job is to evaluate each payment transaction and return a fraud risk score.

                                        Evaluate risk based on these heuristics:
                                        1. **Amount Heuristic**: Transactions above 100,000 in any currency are high risk (+30 points).
                                        2. **Time-of-Day Risk**: Transactions between 02:00–04:00 UTC are suspicious (+20 points).
                                        3. **Merchant Account Age**: If the merchant account is less than 30 days old, it's risky (+20 points).
                                        4. **Velocity / Pattern**: Unusual patterns (round numbers, repeated amounts) add risk (+15 points).
                                        5. **Cross-border Risk**: If sender and receiver appear to be in different countries/regions (+15 points).

                                        You MUST respond with ONLY a valid JSON object in this exact format, no markdown, no explanation:
                                        {
                                          "riskScore": <integer 0-100>,
                                          "reason": "<short explanation of the risk factors found>"
                                        }

                                        The riskScore should be the sum of applicable risk factors, capped at 100.
                                        If no risk factors apply, return a low score (0-15) with reason "No significant risk factors detected".
                                        """;

    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GptAntiFraudService> _logger;

    public GptAntiFraudService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<GptAntiFraudService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<FraudCheckResult> EvaluateAsync(FraudEvaluationContext context, CancellationToken ct = default)
    {
        var apiKey = _configuration["OpenAI:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("OpenAI API key is not configured — fraud check skipped, returning safe score");
            return new FraudCheckResult(0, "Fraud check skipped: API key not configured", false);
        }

        var userPrompt = BuildUserPrompt(context);

        try
        {
            var gptResponse = await CallGptAsync(apiKey, userPrompt, ct);
            var result = ParseResponse(gptResponse);

            _logger.LogInformation(
                "Anti-Fraud AI evaluated {GatewayType} payment of {Amount} {Currency} for {Merchant} — Score: {Score}, Blocked: {Blocked}, Reason: {Reason}",
                context.GatewayType, context.Amount, context.Currency,
                context.MerchantName, result.RiskScore, result.IsBlocked, result.Reason);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Anti-Fraud AI call failed — defaulting to safe score for resilience");
            return new FraudCheckResult(0, $"Fraud check error: {ex.Message}", false);
        }
    }

    private static string BuildUserPrompt(FraudEvaluationContext context)
    {
        var currentUtcHour = DateTime.UtcNow.Hour;
        var merchantAgeDays = (DateTime.UtcNow - context.MerchantCreatedAt).TotalDays;

        return $"""
                Evaluate this payment transaction for fraud risk:

                - Payment Type: {context.GatewayType}
                - Amount: {context.Amount} {context.Currency}
                - Merchant: {context.MerchantName}
                - Merchant Account Age: {merchantAgeDays:F0} days
                - Account Identifier: {context.AccountIdentifier}
                - Current UTC Hour: {currentUtcHour}:00
                - Transaction Payload: {context.RawPayload}

                Return your risk assessment as JSON.
                """;
    }

    private async Task<string> CallGptAsync(string apiKey, string userPrompt, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient("OpenAI");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var model = _configuration["OpenAI:Model"] ?? "gpt-4o-mini";

        var requestBody = new GptRequest
        {
            Model = model,
            Temperature = 0.1,
            MaxTokens = 200,
            Messages =
            [
                new GptMessage { Role = "system", Content = SystemPrompt },
                new GptMessage { Role = "user", Content = userPrompt }
            ]
        };

        var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", httpContent, ct);

        var responseBody = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("OpenAI API returned {StatusCode}: {Body}", response.StatusCode, responseBody);
            throw new HttpRequestException($"OpenAI API error: {response.StatusCode}");
        }

        using var doc = JsonDocument.Parse(responseBody);
        var messageContent = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return messageContent ?? throw new InvalidOperationException("Empty response from GPT");
    }

    private FraudCheckResult ParseResponse(string gptResponse)
    {
        var cleaned = gptResponse
            .Replace("```json", "")
            .Replace("```", "")
            .Trim();

        try
        {
            using var doc = JsonDocument.Parse(cleaned);
            var root = doc.RootElement;

            var score = root.GetProperty("riskScore").GetInt32();
            var reason = root.GetProperty("reason").GetString() ?? "No reason provided";

            score = Math.Clamp(score, 0, 100);

            return new FraudCheckResult(score, reason, score > 75);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse GPT fraud response: {Response}", gptResponse);
            return new FraudCheckResult(0, $"Parse error, raw response: {gptResponse}", false);
        }
    }

    private class GptRequest
    {
        public string Model { get; set; } = "gpt-4o-mini";
        public double Temperature { get; set; } = 0.1;
        public int MaxTokens { get; set; } = 200;
        public List<GptMessage> Messages { get; set; } = [];
    }

    private class GptMessage
    {
        public string Role { get; set; } = "";
        public string Content { get; set; } = "";
    }
}