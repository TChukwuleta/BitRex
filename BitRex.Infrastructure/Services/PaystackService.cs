using BitRex.Application.Common.Interfaces;
using BitRex.Application.Common.Model.Request;
using BitRex.Core.Model.Response;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PayStack.Net;

namespace BitRex.Infrastructure.Services
{
    public class PaystackService : IPaystackService
    {
        private readonly IConfiguration _config;
        private readonly PayStackApi payStack;
        private readonly string token;
        public PaystackService(IConfiguration config)
        {
            _config = config;
            token = _config["Paystack:SecretKey"];
            payStack = new PayStackApi(token);
        }

        public async Task<PaystackInitializationResponse> MakePayment(PaystackPaymentRequest request)
        {
            var result = new PaystackInitializationResponse();
            try
            {
                TransactionInitializeRequest paymentRequest = new()
                {
                    AmountInKobo = (int)(request.Amount * 100),
                    Email = request.Email,
                    Reference = request.Reference,
                    Currency = "NGN",
                    CallbackUrl = "http://localhost:7293/payment/verify"
                };
                TransactionInitializeResponse response = payStack.Transactions.Initialize(paymentRequest);
                if (response.Status)
                {
                    result.AuthorizationCode = response.Data.AccessCode;
                    result.Url = response.Data.AuthorizationUrl;
                    result.Reference = response.Data.Reference;
                    return result;
                }
                result.ErrorMessage = response.Message;
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> VerifyPayment(string reference)
        {
            try
            {
                TransactionVerifyResponse response = payStack.Transactions.Verify(reference);
                if (response.Data.Status == "success")
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
