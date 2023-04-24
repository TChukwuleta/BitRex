using BitRex.Application.Common.Interfaces;
using BitRex.Core.Enums;
using BitRex.Core.Model;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace BitRex.Application.Remittance.Command
{
    public class RemittanceSummaryCommand : IRequest<Response<object>>
    {
        public decimal Amount { get; set; }
        public ExchangeType ExchangeType { get; set; }
    }

    public class RemittanceSummaryCommandHandler : IRequestHandler<RemittanceSummaryCommand, Response<object>>
    {
        private readonly IConfiguration _config;
        private readonly IGraphqlService _graphqlService;
        public RemittanceSummaryCommandHandler(IConfiguration config, IGraphqlService graphqlService)
        {
            _config = config;
            _graphqlService = graphqlService;
        }

        public async Task<Response<object>> Handle(RemittanceSummaryCommand request, CancellationToken cancellationToken)
        {
            var response = new Response<object> { Succeeded = false };
            object summary = default;
            decimal total = default;
            decimal serviceCharge = default;
            decimal.TryParse(_config["DustValue"], out decimal dustValue);
            decimal.TryParse(_config["DollarToNairaRate"], out decimal dollarNairaRate);
            decimal.TryParse(_config["ServiceCharge:FiatToLnBtc"], out decimal lightningFeeCharges);
            decimal.TryParse(_config["ServiceCharge:FiatToBtc"], out decimal bitcoinFeeCharges);
            try
            {
                var dollarEquiv = request.Amount / dollarNairaRate;
                var price = await _graphqlService.GetPrices(PriceGraphRangeType.ONE_DAY);
                var monetaryValue = (dollarEquiv / price);
                switch (request.ExchangeType)
                {
                    case ExchangeType.Bitcoin:
                        serviceCharge = bitcoinFeeCharges * monetaryValue;
                        total = monetaryValue - serviceCharge;
                        break;
                    case ExchangeType.LnBtc:
                        serviceCharge = lightningFeeCharges * monetaryValue;
                        total = monetaryValue - serviceCharge;
                        break;
                    default:
                        break;
                }
                var val = (total * 100000000);
                if (val <= dustValue)
                {
                    summary = new
                    {
                        //ServiceChargeInDollars = serviceCharge,
                        ProposedAmountInNaira = request.Amount,
                        ProposedAmountInDollar = (request.Amount / dollarNairaRate),
                        ReturnValue = total,
                        CurrentBitcoinPriceInDollar = $"$ {price}",
                        CurrentBitcoinPriceInNaira = $"N {price * dollarNairaRate}",
                        Message = "Please note that the amount inputed is less than or equal to the dust value. Hence it would not be processed"
                    };
                }
                else
                {
                    summary = new
                    {
                        //ServiceChargeInDollars = serviceCharge,
                        ProposedAmountInNaira = request.Amount,
                        ProposedAmountInDollar = (request.Amount / dollarNairaRate),
                        ReturnValueInSats = (total * 100000000),
                        CurrentBitcoinPriceInDollar = $"$ {price}",
                        CurrentBitcoinPriceInNaira = $"N {price * dollarNairaRate}",
                    };
                }
                
                response.Succeeded = true;
                response.Message = $"Remittance summary was retrieved successfully";
                response.Data = summary;
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
