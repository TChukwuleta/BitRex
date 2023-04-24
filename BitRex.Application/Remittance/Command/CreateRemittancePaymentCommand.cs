using BitRex.Application.Common.Interfaces;
using BitRex.Application.Common.Model.Request;
using BitRex.Application.Transaction;
using BitRex.Core.Enums;
using BitRex.Core.Model;
using BitRex.Core.Model.Request;
using BitRex.Core.Model.Response;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace BitRex.Application.Remittance.Command
{
    public class CreateRemittancePaymentCommand : IRequest<Response<PaystackInitializationResponse>>
    {
        public int Amount { get; set; }
        public string Destination { get; set; }
        public string Narration { get; set; }
        public ExchangeType ExchangeType { get; set; }
    }

    public class CreateRemittancePaymentCommandHandler : IRequestHandler<CreateRemittancePaymentCommand, Response<PaystackInitializationResponse>>
    {
        private readonly IConfiguration _config;
        private readonly IPaystackService _paystackService;
        private readonly ILightningService _lightningService;
        private readonly IAppDbContext _context;
        private readonly IGraphqlService _graphqlService;
        private readonly IBitcoinCoreClient _bitcoinCoreClient;

        public CreateRemittancePaymentCommandHandler(IConfiguration config,
            ILightningService lightningService, IAppDbContext context, 
            IGraphqlService graphqlService, IPaystackService paystackService, IBitcoinCoreClient bitcoinCoreClient)
        {
            _config = config;
            _lightningService = lightningService;
            _context = context;
            _graphqlService = graphqlService;
            _bitcoinCoreClient = bitcoinCoreClient;
            _paystackService = paystackService;
        }

        public async Task<Response<PaystackInitializationResponse>> Handle(CreateRemittancePaymentCommand request, CancellationToken cancellationToken)
        {
            var response = new Response<PaystackInitializationResponse> { Succeeded = false };
            var reference = $"BitRex_{DateTime.Now.Ticks}";
            decimal total = default;
            decimal serviceCharge = default;
            decimal.TryParse(_config["DustValue"], out decimal dustValue);
            decimal.TryParse(_config["DollarToNairaRate"], out decimal dollarNairaRate);
            decimal.TryParse(_config["ServiceCharge:FiatToLnBtc"], out decimal lightningFeeCharges);
            decimal.TryParse(_config["ServiceCharge:FiatToBtc"], out decimal bitcoinFeeCharges);
            try
            {
                var transactionRecord = new CreateTransactionDto
                {
                    Narration = request.Narration,
                    SourceAmount = request.Amount,
                    TransactionReference = reference,
                    SourcePaymentModeType = PaymentModeType.Fiat,
                    SourceAddress = "Admin-Fiat",
                    Hash = "Admin-Fiat",
                    TransactionStatus = TransactionStatus.Initiated
                };
                var dollarEquiv = request.Amount / dollarNairaRate;
                var price = await _graphqlService.GetPrices(PriceGraphRangeType.ONE_DAY);
                var monetaryValue = (dollarEquiv / price);
                if (monetaryValue <= dustValue)
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "Monetary value cannot be less than the dust value";
                    return response;
                }
                
                switch (request.ExchangeType)
                {
                    case ExchangeType.Bitcoin:
                        serviceCharge = bitcoinFeeCharges * monetaryValue;
                        total = monetaryValue - serviceCharge;
                        var validateBitcoinAddress = await _bitcoinCoreClient.ValidateBitcoinAddress(request.Destination);
                        if (!validateBitcoinAddress)
                        {
                            response.StatusCode = (int)HttpStatusCode.BadRequest;
                            response.Message = "Invalid bitcoin address";
                            return response;
                        }
                        var bitcoinBalance = await _bitcoinCoreClient.GetWalletBalance();
                        if (bitcoinBalance <= total)
                        {
                            response.StatusCode = (int)HttpStatusCode.BadRequest;
                            response.Message = "Cannot process transaction. Insufficient bitcoin balance";
                            return response;
                        }
                        transactionRecord.DestinationAmount = (total * 100000000);
                        transactionRecord.DestinationAddress = request.Destination;
                        transactionRecord.DestinationPaymentModeType = PaymentModeType.Bitcoin;
                        transactionRecord.SourcePaymentModeType = PaymentModeType.Fiat;
                        break;
                    case ExchangeType.LnBtc:
                        serviceCharge = lightningFeeCharges * monetaryValue;
                        total = monetaryValue - serviceCharge;
                        var validateLightningRequest = await _lightningService.ValidateLightningAddress(request.Destination);
                        if (!validateLightningRequest.success)
                        {
                            response.StatusCode = (int)HttpStatusCode.BadRequest;
                            response.Message = "Invalid bitcoin address";
                            return response;
                        }
                        var lightningBalance = await _lightningService.GetWalletBalance();
                        if (lightningBalance <= (total * 100000000))
                        {
                            response.StatusCode = (int)HttpStatusCode.BadRequest;
                            response.Message = "Cannot process transaction. Insufficient lightning balance";
                            return response;
                        }
                        var confirmLightningRequestAmount = await _lightningService.ConfirmLightningValue(request.Destination, (total * 100000000));
                        if (!confirmLightningRequestAmount.success)
                        {
                            response.StatusCode = (int)HttpStatusCode.BadRequest;
                            response.Message = $"Cannot process transaction. {confirmLightningRequestAmount.message}";
                            return response;
                        }
                        transactionRecord.DestinationAmount = (total * 100000000);
                        transactionRecord.DestinationAddress = request.Destination;
                        transactionRecord.SourcePaymentModeType = PaymentModeType.Fiat;
                        transactionRecord.DestinationPaymentModeType = PaymentModeType.Lightning;
                        break;
                    default:
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        response.Message = "Invalid exchange type";
                        return response;
                }
                transactionRecord.DestinationAmount = total;
                var transaction = await new TransactionHelper(_context).CreateTransaction(transactionRecord);
                var paystackRequest = new PaystackPaymentRequest
                {
                    Name = "BitRex",
                    Reference = reference,
                    Email = _config["SupportEmail"],
                    Amount = request.Amount
                };
                var paystackInitialtion = await _paystackService.MakePayment(paystackRequest);
                response.Succeeded = true;
                response.Message = "Remittance payment was initiated via paystack";
                response.Data = paystackInitialtion;
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
