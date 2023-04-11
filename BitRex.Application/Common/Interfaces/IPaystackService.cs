
using BitRex.Application.Common.Model.Request;

namespace BitRex.Application.Common.Interfaces
{
    public interface IPaystackService
    {
        Task<string> MakePayment(PaystackPaymentRequest request);
        Task<bool> VerifyPayment(string reference);
    }
}
