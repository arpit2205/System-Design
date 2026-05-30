namespace Models;

public class PaymentResult
{
    public bool Success { get; set; }
    public Guid TransactionId { get; set; }
    public string Message { get; set; } = string.Empty;

}