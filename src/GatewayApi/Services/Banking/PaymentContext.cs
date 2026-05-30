using GatewayApi.Enums;

namespace GatewayApi.Services.Banking;

public record PaymentContext(
    Guid TransactionId,
    GatewayType Type,
    Guid BankId,
    decimal Amount,
    string Currency,
    string RawPayload);