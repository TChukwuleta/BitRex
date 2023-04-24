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
        public decimal AmountInBtc { get; set; }
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
        public CreateExchangeCommandHandler(IConfiguration config, IBitcoinCoreClient bitcoinCoreClient, 
            ILightningService lightningService, IAppDbContext context)
        {
            _config = config;
            _lightningService = lightningService;
            _bitcoinCoreClient = bitcoinCoreClient;
            _context = context;
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
            decimal.TryParse(_config["MinimumAmountBtc"], out decimal minAmount);
            decimal.TryParse(_config["MaximumAmountBtc"], out decimal maxAmount);
            try
            {
                var transactionRecord = new CreateTransactionDto
                {
                    Narration = request.Narration,
                    SourceAmount = request.AmountInBtc,
                    TransactionReference = reference,
                    Hash = new Key().PubKey.ToHex()
                };
                
                if (request.AmountInBtc < minAmount)
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "Value is less than minimum amount that the system can process";
                    return response;
                }
                if (request.AmountInBtc > maxAmount)
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
                            case ExchangeType.LnBtc:
                                var address = await _bitcoinCoreClient.ValidateBitcoinAddress(request.Destination);
                                if (!address)
                                {
                                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                                    response.Message = "Invalid bitcoin address";
                                    return response;
                                }
                                decimal.TryParse(_config["ServiceCharge:LnBtcToBtc"], out serviceCharge);
                                decimal.TryParse(_config["MinerFee:LnBtcToBtc"], out minerFee);
                                serviceChargeValue = request.AmountInBtc * (serviceCharge / 100);
                                total = request.AmountInBtc - (serviceChargeValue + minerFee);
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
                                decimal.TryParse(_config["ServiceCharge:BtcToLnBtc"], out serviceCharge);
                                decimal.TryParse(_config["MinerFee:BtcToLnBtc"], out minerFee);
                                serviceChargeValue = request.AmountInBtc * (serviceCharge / 100);
                                total = request.AmountInBtc - (serviceChargeValue + minerFee);
                                if ((total * 100000000) <= dustValue)
                                {
                                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                                    response.Message = "Monetary value cannot be less than the dust value";
                                    return response;
                                }
                                var val = total * 100000000;
                                var confirmValue = await _lightningService.ConfirmLightningValue(request.Destination, val);
                                if (!confirmValue.success)
                                {
                                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                                    response.Message = confirmValue.message;
                                    return response;
                                }
                                var generateAddress = await _bitcoinCoreClient.SwapBitcoinAddress(request.Source, validateInvoice.amount, request.Destination);
                                var addressResult = new 
                                {
                                    Address = generateAddress.response
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
                                response.Message = $"A bitcoin address has been generated successfully";
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
