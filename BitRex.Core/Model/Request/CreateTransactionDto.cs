using BitRex.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitRex.Core.Model.Request
{
    public class CreateTransactionDto
    {
        public decimal DestinationAmount { get; set; }
        public string DestinationAddress { get; set; }
        public PaymentModeType DestinationPaymentModeType { get; set; }
        public decimal SourceAmount { get; set; }
        public string Hash { get; set; }
        public string SourceAddress { get; set; }
        public PaymentModeType SourcePaymentModeType { get; set; }
        public string Narration { get; set; }
        public TransactionStatus TransactionStatus { get; set; }
        public string TransactionReference { get; set; }
    }
}
