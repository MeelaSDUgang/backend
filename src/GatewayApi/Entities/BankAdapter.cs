namespace GatewayApi.Entities;

public class BankAdapter
{
    public Guid Id { get; set; }
    public string RoutingKey { get; set; }
    public string Name { get; set; }
    public bool IsActive { get; set; }
    public string SupportedGatewayTypes { get; set; } = "";

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}