namespace GatewayApi.Models;

public record PaymentResponse(
    string TransactionId,
    string BankId,
    string Type,
    string Status,
    decimal Amount,
    string NameDest,
    string NameOrig,
    decimal NewbalanceDest,
    decimal NewbalanceOrig,
    decimal OldbalanceDest,
    decimal OldbalanceOrg,
    int Step,
    string Label,
    string Currency,
    DateTime UpdatedAt);

public record TransactionStatusResponse(
    string TransactionId,
    string BankId,
    string Type,
    string Status,
    decimal Amount,
    string NameDest,
    string NameOrig,
    decimal NewbalanceDest,
    decimal NewbalanceOrig,
    decimal OldbalanceDest,
    decimal OldbalanceOrg,
    int Step,
    string Label,
    string Currency,
    string? FailureReason,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record GatewayResponse(
    Guid Id,
    string Name,
    bool IsActive,
    string[] SupportedTypes);

public record ErrorResponse(string Error, string? Detail = null);

public record P2PRequest(
    string BankId,
    decimal Amount,
    string Currency,
    string Message,
    ContactInfo Sender,
    ContactInfo Receiver,
    string? NameDest = null,
    string? NameOrig = null,
    decimal NewbalanceDest = 0,
    decimal NewbalanceOrig = 0,
    decimal OldbalanceDest = 0,
    decimal OldbalanceOrg = 0,
    int Step = 0,
    string? Type = null,
    string? Label = null);

public record A2ARequest(
    string BankId,
    decimal Amount,
    string Currency,
    string ConsentId,
    string EndToEndId,
    AccountInfo DebtorAccount,
    AccountInfo CreditorAccount);

public record B2BRequest(
    string BankId,
    decimal Amount,
    string Currency,
    string Priority,
    string PurposeCode,
    CompanyInfo PayerInfo,
    CompanyInfo ReceiverInfo);

public record PayoutRequest(
    string BankId,
    decimal Amount,
    string Currency,
    string PurposeCode,
    string MerchantReference,
    AccountInfo FundingAccount,
    RecipientInfo RecipientAccount);

public record RecipientInfo(string Iban, string Iin, string FullName);

public record AccountInfo(string Iban);

public record CompanyInfo(string CompanyName, string TaxId, string AccountNumber);

public record ContactInfo(string PhoneNumber);