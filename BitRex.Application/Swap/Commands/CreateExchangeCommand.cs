using BitRex.Application.Common.Interfaces;
using BitRex.Application.Transaction;
using BitRex.Core.Enums;
using BitRex.Core.Model;
using BitRex.Core.Model.Request;
using MediatR;
using Microsoft.Extensions.Configuration;
using NBitcoin;
using System.Net;

namespace BitRex.Application.Swap.Commands
{
    public class CreateExchangeCommand : IRequest<Response<object>>
    {
        public decimal Amount { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }
        public string Narration { get; set; }
        public ExchangeType FromExchange { get; set; }
        public ExchangeType ToExchange { get; set; }
    }

    public class CreateExchangeCommandHandler : IRequestHandler<CreateExchangeCommand, Response<object>>
    {
        private readonly IConfiguration _config;
        private readonly IBitcoinCoreClient _bitcoinCoreClient;
        private readonly ILightningService _lightningService;
        private readonly IAppDbContext _context;
        private readonly IGraphqlService _graphqlService;
        public CreateExchangeCommandHandler(IConfiguration config, IBitcoinCoreClient bitcoinCoreClient, 
            ILightningService lightningService, IAppDbContext context, IGraphqlService graphqlService)
        {
            _config = config;
            _lightningService = lightningService;
            _bitcoinCoreClient = bitcoinCoreClient;
            _context = context;
            _graphqlService = graphqlService;
        }

        public async Task<Response<object>> Handle(CreateExchangeCommand request, CancellationToken cancellationToken)
        {
            var response = new Response<object> { Succeeded = false };
            var reference = $"BitRex_{DateTime.Now.Ticks}";
            decimal minerFee = default;
            decimal serviceCharge = default;
            decimal serviceChargeValue = default;
            decimal total = default;
            decimal.TryParse(_config["DustValue"], out decimal dustValue);
            decimal.TryParse(_config["DollarToNairaRate"], out decimal dollarNairaRate);
            decimal.TryParse(_config["MinimumAmountBtc"], out decimal minAmount);
            decimal.TryParse(_config["MaximumAmountBtc"], out decimal maxAmount);
            try
            {
                var transactionRecord = new CreateTransactionDto
                {
                    Narration = request.Narration,
                    SourceAmount = request.Amount,
                    TransactionReference = reference,
                    Hash = new Key().PubKey.ToHex()
                };

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


                //monetaryValue = (monetaryValue * 100000000);

                switch (request.ToExchange)
                {
                    case ExchangeType.Bitcoin:
                        switch (request.FromExchange)
                        {
                            case ExchangeType.Bitcoin:
                                response.StatusCode = (int)HttpStatusCode.BadRequest;
                                response.Message = "Cannot convert to same currency type";
                                return response;
                                break;
                            case ExchangeType.LnBtc:
                                var address = await _bitcoinCoreClient.ValidateBitcoinAddress(request.Destination);
                                if (!address)
                                {
                                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                                    response.Message = "Invalid bitcoin address";
                                    return response;
                                }
                                decimal.TryParse(_config["ServiceCharge:LnBtcToLnBtc"], out serviceCharge);
                                decimal.TryParse(_config["MinerFee:LnBtcToBtc"], out minerFee);
                                serviceChargeValue = monetaryValue * (serviceCharge / 100);
                                total = monetaryValue - (serviceChargeValue + minerFee);
                                if ((total * 100000000) <= dustValue)
                                {
                                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                                    response.Message = "Monetary value cannot be less than the dust value";
                                    return response;
                                }
                                var generateInvloice = await _lightningService.CreateSwapInvoice(transactionRecord.Hash, (long)(total * 100000000), reference);
                                var invoiceResult = new
                                {
                                    Invloce = generateInvloice
                                };
                                transactionRecord.DestinationAddress = request.Destination;
                                transactionRecord.SourceAddress = generateInvloice;
                                transactionRecord.DestinationPaymentModeType = PaymentModeType.Bitcoin;
                                transactionRecord.SourcePaymentModeType = PaymentModeType.Lightning;
                                transactionRecord.DestinationAmount = (total * 100000000);
                                transactionRecord.TransactionStatus = TransactionStatus.Initiated;
                                var transaction = await new TransactionHelper(_context).CreateTransaction(transactionRecord);
                                response.Succeeded = true;
                                response.Data = invoiceResult;
                                response.Message = $"A lightning invoice has been generated successfully";
                                response.StatusCode = (int)HttpStatusCode.OK;
                                return response;
                            default:
                                break;
                        }
                        break;
                    case ExchangeType.LnBtc:
                        switch (request.FromExchange)
                        {
                            case ExchangeType.Bitcoin:
                                var validateInvoice = await _lightningService.ValidateLightningAddress(request.Destination);
                                if (!validateInvoice.success)
                                {
                                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                                    response.Message = "Invalid lightning invoice";
                                    return response;
                                }
                                decimal.TryParse(_config["ServiceCharge:BtcToBtc"], out serviceCharge);
                                decimal.TryParse(_config["MinerFee:BtcToLnBtc"], out minerFee);
                                serviceChargeValue = monetaryValue * (serviceCharge / 100);
                                total = monetaryValue - (serviceChargeValue + minerFee);
                                if ((total * 100000000) <= dustValue)
                                {
                                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                                    response.Message = "Monetary value cannot be less than the dust value";
                                    return response;
                                }

                                if ((total * 100000000) < validateInvoice.amount)
                                {
                                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                                    response.Message = $"Invoice value is greater than return value by {validateInvoice.amount - (total * 100000000)}";
                                    return response;
                                }
                                if ((total * 100000000) > validateInvoice.amount)
                                {
                                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                                    response.Message = $"Invoice value is less than return value by {(total * 100000000) - validateInvoice.amount}";
                                    return response;
                                }
                                var generateAddress = await _bitcoinCoreClient.SwapBitcoinAddress(request.Source, validateInvoice.amount, request.Destination);
                                var addressResult = new 
                                {
                                    Address = generateAddress
                                };
                                transactionRecord.SourceAddress = generateAddress.response;
                                transactionRecord.DestinationAddress = request.Destination;
                                transactionRecord.DestinationPaymentModeType = PaymentModeType.Lightning;
                                transactionRecord.SourcePaymentModeType = PaymentModeType.Bitcoin;
                                transactionRecord.DestinationAmount = total;
                                transactionRecord.TransactionStatus = TransactionStatus.Initiated;
                                var transaction = await new TransactionHelper(_context).CreateTransaction(transactionRecord);
                                response.Succeeded = true;
                                response.Data = addressResult;
                                response.Message = $"A lightning invoice has been generated successfully";
                                response.StatusCode = (int)HttpStatusCode.OK;
                                return response;
                                break;
                            case ExchangeType.LnBtc:
                                response.StatusCode = (int)HttpStatusCode.BadRequest;
                                response.Message = "Cannot convert to same currency type";
                                return response;
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }
                response.Succeeded = true;
                response.Message = "Exchange initiation was successful";
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
