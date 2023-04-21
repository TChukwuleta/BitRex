
using BitRex.Core.Enums;

namespace BitRex.Core.Entities
{
    public class Transaction : GeneralEntity
    {
        public string DestinationAddress { get; set; }
        public PaymentModeType DestinationPaymentModeType { get; set; }
        public string DestinationPaymentModeTypeDesc { get { return DestinationPaymentModeType.ToString(); } }
        public string SourceAddress { get; set; }
        public PaymentModeType SourcePaymentModeType { get; set; }
        public string SourcePaymentModeTypeDesc { get { return SourcePaymentModeType.ToString(); } }
        public string Narration { get; set; }
        public string Hash { get; set; }
        public decimal DestinationAmount { get; set; }
        public decimal SourceAmount { get; set; }
        public TransactionStatus TransactionStatus { get; set; }
        public string TransactionStatusDesc { get { return TransactionStatus.ToString(); } }
        public string TransactionReference { get; set; }
    }
}
