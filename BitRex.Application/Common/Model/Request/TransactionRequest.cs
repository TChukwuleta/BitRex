using BitRex.Application.Common.Interfaces.Validators;
using BitRex.Core.Enums;

namespace BitRex.Application.Common.Model.Request
{
    public class TransactionRequest : ITransactionRequestValidator
    {
        public string Description { get; set; }
        public string DebitAccount { get; set; }
        public string CreditAccount { get; set; }
        public decimal Amount { get; set; }
        public TransactionType TransactionType { get; set; }
        public string Sender { get; set; }
        public string Recipient { get; set; }
    }
}
