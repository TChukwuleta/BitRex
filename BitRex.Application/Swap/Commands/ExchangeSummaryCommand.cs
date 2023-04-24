using BitRex.Application.Common.Interfaces;
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

namespace BitRex.Application.Swap.Commands
{
    public class ExchangeSummaryCommand : IRequest<Response<object>>
    {
        public decimal Amount { get; set; }
        public ExchangeType ExchangeType { get; set; }
    }

    public class ExchangeSummaryCommandHandler : IRequestHandler<ExchangeSummaryCommand, Response<object>>
    {
        private readonly IConfiguration _config;
        private readonly IBitcoinCoreClient _bitcoinCoreClient;
        private readonly ILightningService _lightningService;
        private readonly IAppDbContext _context;
        private readonly IGraphqlService _graphqlService;
        public ExchangeSummaryCommandHandler(IConfiguration config, IBitcoinCoreClient bitcoinCoreClient, ILightningService lightningService, IAppDbContext context, IGraphqlService graphqlService)
        {
            _config = config;
            _bitcoinCoreClient = bitcoinCoreClient;
            _lightningService = lightningService;
            _context = context;
            _graphqlService = graphqlService;
        }

        public async Task<Response<object>> Handle(ExchangeSummaryCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var response = new Response<object> { Succeeded = false };
                var reference = $"BitRex_{DateTime.Now.Ticks}";
                decimal minerFee = default;
                decimal serviceCharge = default;
                object summary = default;
                decimal serviceChargeValue = default;
                decimal total = default;
                decimal.TryParse(_config["DustValue"], out decimal dustValue);
                decimal.TryParse(_config["DollarToNairaRate"], out decimal dollarNairaRate);
                decimal.TryParse(_config["MinimumAmountBtc"], out decimal minAmount);
                decimal.TryParse(_config["MaximumAmountBtc"], out decimal maxAmount);

                var dollarEquiv = request.Amount / dollarNairaRate;
                var price = await _graphqlService.GetPrices(PriceGraphRangeType.ONE_DAY);
                var monetaryValue = (dollarEquiv / price);
                if (monetaryValue < minAmount)
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "Value is less than minimum amount that the system can process";
                    return response;
                }
                if (monetaryValue > maxAmount)
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "Value is more than maximum amount that the system can process";
                    return response;
                }
                switch (request.ExchangeType)
                {
                    case ExchangeType.Bitcoin:

                        decimal.TryParse(_config["ServiceCharge:LnBtcToLnBtc"], out serviceCharge);
                        decimal.TryParse(_config["MinerFee:LnBtcToBtc"], out minerFee);
                        serviceChargeValue = monetaryValue * (serviceCharge / 100);
                        total = monetaryValue - (serviceChargeValue + minerFee);
                        break;
                    case ExchangeType.LnBtc:
                        decimal.TryParse(_config["ServiceCharge:BtcToBtc"], out serviceCharge);
                        decimal.TryParse(_config["MinerFee:BtcToLnBtc"], out minerFee);
                        serviceChargeValue = monetaryValue * (serviceCharge / 100);
                        total = monetaryValue - (serviceChargeValue + minerFee);
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

                throw ex;
            }
        }
    }
}
