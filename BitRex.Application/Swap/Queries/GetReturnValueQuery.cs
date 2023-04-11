using BitRex.Core.Enums;
using BitRex.Core.Model;
using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BitRex.Application.Swap.Queries
{
    public class GetReturnValueQuery : IRequest<Response<object>>
    {
        public decimal Amount { get; set; }
        public ExchangeType FromExchange { get; set; }
        public ExchangeType ToExchange { get; set; }
    }

    public class GetReturnValueQueryHandler : IRequestHandler<GetReturnValueQuery, Response<object>>
    {
        private readonly IConfiguration _config;
        public GetReturnValueQueryHandler(IConfiguration config)
        {
            _config = config;
        }

        public async Task<Response<object>> Handle(GetReturnValueQuery request, CancellationToken cancellationToken)
        {
            decimal serviceCharge = default;
            decimal receivingAmount = default;
            decimal serviceChargeValue = default;
            decimal minerFee = default;
            var response = new Response<object> { Succeeded = false };
            decimal.TryParse(_config["DustValue"], out decimal dustValue);
            decimal.TryParse(_config["MinimumAmountBtc"], out decimal minAmount);
            decimal.TryParse(_config["MaximumAmountBtc"], out decimal maxAmount);
            decimal.TryParse(_config["ServiceChargeBtc"], out serviceCharge);
            try
            {
                if (request.Amount <= 0)
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "Amount cannot be less than or equal zero";
                    return response;
                }
                if (request.Amount <= dustValue)
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "Amount must not be less than dust limit";
                    return response;
                }
                switch (request.FromExchange)
                {
                    case ExchangeType.Bitcoin:
                        switch (request.ToExchange)
                        {
                            case ExchangeType.Bitcoin:
                                decimal.TryParse(_config["ServiceCharge:BtcToBtc"], out serviceCharge);
                                serviceChargeValue = request.Amount * serviceCharge;
                                receivingAmount = request.Amount - serviceChargeValue;
                                break;
                            case ExchangeType.LnBtc:
                                decimal.TryParse(_config["ServiceCharge:BtcToBtc"], out serviceCharge);
                                decimal.TryParse(_config["MinerFee:BtcToLnBtc"], out minerFee);
                                serviceChargeValue = request.Amount * serviceCharge;
                                receivingAmount = request.Amount - (serviceChargeValue + minerFee);
                                break;
                            default:
                                response.StatusCode = (int)HttpStatusCode.BadRequest;
                                response.Message = "Invalid destination exchange type selected";
                                return response;
                        }
                        break;
                    case ExchangeType.LnBtc:
                        switch (request.ToExchange)
                        {
                            case ExchangeType.Bitcoin:
                                decimal.TryParse(_config["ServiceCharge:LnBtcToLnBtc"], out serviceCharge);
                                decimal.TryParse(_config["MinerFee:LnBtcToBtc"], out minerFee);
                                serviceChargeValue = request.Amount * serviceCharge;
                                receivingAmount = request.Amount - (serviceChargeValue + minerFee);
                                break;
                            case ExchangeType.LnBtc:
                                decimal.TryParse(_config["ServiceCharge:LnBtcToLnBtc"], out serviceCharge);
                                serviceChargeValue = request.Amount * serviceCharge;
                                receivingAmount = request.Amount - serviceChargeValue;
                                break;
                            default:
                                response.StatusCode = (int)HttpStatusCode.BadRequest;
                                response.Message = "Invalid destination exchange type selected";
                                return response;
                        }
                        break;
                    default:
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        response.Message = "Invalid source exchange type selected";
                        return response;
                }

                var result = new
                {
                    MinAmount = minAmount,
                    MaxAmount = maxAmount,
                    BitRexFee = $"{serviceCharge}%",
                    MinerFee = $"{minerFee} BTC",
                    YouSend = request.Amount,
                    YouReceive = receivingAmount
                };
                response.Succeeded = true;
                response.Data = result;
                response.Message = $"Excahnge summary retrieved successfully";
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
