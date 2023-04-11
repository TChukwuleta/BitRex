using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitRex.Application.Common.Model.Request
{
    public class PaystackPaymentRequest
    {
        public string Name { get; set; }
        public string Reference { get; set; }
        public string Email { get; set; }
        public int Amount { get; set; }
    }
}
