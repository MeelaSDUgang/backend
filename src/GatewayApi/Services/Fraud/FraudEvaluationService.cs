using System.Text.Json;
using Microsoft.Extensions.Options;

namespace GatewayApi.Services.Fraud;

public class FraudEvaluationService : IFraudEvaluationService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly ILogger<FraudEvaluationService> _logger;
    private readonly FraudOptions _options;

    public FraudEvaluationService(
        HttpClient httpClient,
        IOptions<FraudOptions> options,
        ILogger<FraudEvaluationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;

        _httpClient.Timeout = TimeSpan.FromSeconds(Math.Max(1, _options.TimeoutSeconds));
    }

    public async Task<FraudEvaluationResult> EvaluateAsync(
        FraudScoringInput input,
        CancellationToken cancellationToken)
    {
        try
        {
            var score = await PostScoreAsync(input, cancellationToken);
            var riskLevel = FraudRiskTierParser.Parse(score.RiskTier);
            if (riskLevel is FraudRiskLevel.None or FraudRiskLevel.Low)
                return new FraudEvaluationResult(true, null, score, null);

            var interpretation = await PostInterpretationAsync(score, cancellationToken);
            return new FraudEvaluationResult(true, null, score, interpretation);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or JsonException)
        {
            _logger.LogWarning(exception, "Fraud evaluation failed.");
            return new FraudEvaluationResult(false, exception.Message, null, null);
        }
    }

    private async Task<FraudScoreResponse> PostScoreAsync(
        FraudScoringInput input,
        CancellationToken cancellationToken)
    {
        var request = new FraudScoreRequest
        {
            Step = input.Step,
            Type = input.Type,
            Amount = input.Amount,
            OldbalanceOrg = input.OldbalanceOrg,
            NewbalanceOrig = input.NewbalanceOrig,
            OldbalanceDest = input.OldbalanceDest,
            NewbalanceDest = input.NewbalanceDest,
            NameOrig = input.NameOrig,
            NameDest = input.NameDest
        };

        using var response =
            await _httpClient.PostAsJsonAsync(_options.ScoreUrl, request, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<FraudScoreResponse>(JsonOptions, cancellationToken)
               ?? throw new JsonException("Fraud scoring response is empty.");
    }

    private async Task<FraudInterpretationResponse> PostInterpretationAsync(
        FraudScoreResponse score,
        CancellationToken cancellationToken)
    {
        using var response =
            await _httpClient.PostAsJsonAsync(_options.InterpretUrl, score, JsonOptions, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<FraudInterpretationResponse>(JsonOptions, cancellationToken)
               ?? throw new JsonException("Fraud interpretation response is empty.");
    }
}