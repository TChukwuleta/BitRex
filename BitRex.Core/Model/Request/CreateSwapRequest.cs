namespace BitRex.Core.Model.Request
{
    public class CreateSwapRequest
    {
        public string PublicKey { get; set; }
        public string PaymentRequest { get; set; }
        public decimal Value { get; set; }
    }
}
