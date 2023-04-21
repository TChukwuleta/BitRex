
using BitRex.Application.Common.Model.Request;
using BitRex.Core.Model.Response;

namespace BitRex.Application.Common.Interfaces
{
    public interface IPaystackService
    {
        Task<PaystackInitializationResponse> MakePayment(PaystackPaymentRequest request);
        Task<bool> VerifyPayment(string reference);
    }
}
