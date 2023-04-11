using BitRex.Core.Enums;

namespace BitRex.Application.Common.Interfaces.Validators
{
    public interface ITransactionRequestValidator
    {
        public string Sender { get; set; }
        public string Recipient { get; set; }
        public decimal Amount { get; set; }
        public TransactionType TransactionType { get; set; }
    }
}
