using BitRex.Core.Model;
using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace BitRex.Application.Swap
{
    public class CreateSubmarineSwapCommand : IRequest<Response<string>>
    {
        public string PublicKey { get; set; }
        public string PaymentRequest { get; set; }
        public decimal Value { get; set; }
    }

    public class CreateSubmarineSwapCommandHandler : IRequestHandler<CreateSubmarineSwapCommand, Response<string>>
    {
        private readonly IConfiguration _config;
        public CreateSubmarineSwapCommandHandler(IConfiguration config)
        {
            _config = config;
        }

        public async Task<Response<string>> Handle(CreateSubmarineSwapCommand request, CancellationToken cancellationToken)
        {
            var response = new Response<string> { Succeeded = false };
            decimal.TryParse(_config["DustValue"], out decimal dustValue);
            decimal.TryParse(_config["MinimumAmountBtc"], out decimal minAmount);
            decimal.TryParse(_config["MaximumAmountBtc"], out decimal maxAmount);
            decimal.TryParse(_config["ServiceChargeBtc"], out decimal serviceCharge);
            try
            {
                if (request.PublicKey.Length < 64)
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "Invalid public key";
                    return response;
                }
                if (request.Value <= 0)
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "Amount cannot be less than or equal zero";
                    return response;
                }
                if (request.Value <= dustValue)
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "Amount must not be less than dust limit";
                    return response;
                }
                if (request.Value < minAmount)
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = $"Amount is less than the minimum amount set on the system. Please enter a value equal or greater than {minAmount}";
                    return response;
                }
                if (request.Value > maxAmount)
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = $"Amount is greater than the maximum amount set on the system. Please enter a value equal or less than {maxAmount}";
                    return response;
                }

                if (request.PaymentRequest.Length != 64)
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "Invalid payment hash";
                    return response;
                }
                response.Succeeded = true;
                response.Message = $"User creation was successful. An OTP has been sent to your";
                response.StatusCode = (int)HttpStatusCode.OK;
                return response;
            }
            catch (Exception ex)
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = $"An error occured. {ex?.Message ?? ex?.InnerException.Message}";
                return response;
            }
        }
    }
}
