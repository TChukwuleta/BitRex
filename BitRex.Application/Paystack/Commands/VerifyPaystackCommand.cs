using BitRex.Application.Common.Interfaces;
using BitRex.Core.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace BitRex.Application.Paystack.Commands
{
    public class VerifyPaystackCommand : IRequest<Response<string>>
    {
        public string Reference { get; set; }
    }

    public class VerifyPaystackCommandHandler : IRequestHandler<VerifyPaystackCommand, Response<string>>
    {
        private readonly IPaystackService _paystackService;
        private readonly ILightningService _lightningService;
        private readonly IBitcoinCoreClient _bitcoinCoreClient;
        private readonly IAppDbContext _context;
        public VerifyPaystackCommandHandler(IPaystackService paystackService, ILightningService lightningService, 
            IAppDbContext context, IBitcoinCoreClient bitcoinCoreClient)
        {
            _paystackService = paystackService;
            _lightningService = lightningService;
            _bitcoinCoreClient = bitcoinCoreClient;
            _context = context;
        }

        public async Task<Response<string>> Handle(VerifyPaystackCommand request, CancellationToken cancellationToken)
        {
            var response = new Response<string> { Succeeded = false };
            try
            {
                var transaction = await _context.Transactions.FirstOrDefaultAsync(c => c.TransactionReference == request.Reference);
                if (transaction == null)
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "Invalid transaction details";
                    return response;
                }
                if (transaction.TransactionStatus == Core.Enums.TransactionStatus.Success)
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "Transaction has been completed";
                    return response;
                }
                if (transaction.TransactionStatus != Core.Enums.TransactionStatus.Initiated)
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "Invalid transaction";
                    return response;
                }
                var verifyPayment = await _paystackService.VerifyPayment(request.Reference);
                if (!verifyPayment)
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "Error validating transaction";
                    return response;
                }

                switch (transaction.DestinationPaymentModeType)
                {
                    case Core.Enums.PaymentModeType.Bitcoin:
                        var bitcoinBalance = await _bitcoinCoreClient.GetWalletBalance();
                        if (bitcoinBalance <= transaction.DestinationAmount)
                        {
                            response.StatusCode = (int)HttpStatusCode.BadRequest;
                            response.Message = "Insufficient balance. Kindly contact support";
                            return response;
                        }
                        var bitcoinPayment = await _bitcoinCoreClient.MakePayment(transaction);
                        if (string.IsNullOrEmpty(bitcoinPayment))
                        {
                            response.StatusCode = (int)HttpStatusCode.BadRequest;
                            response.Message = "Error processing bitcoin payment";
                            return response;
                        }
                        response.Data = bitcoinPayment;
                        break;
                    case Core.Enums.PaymentModeType.Lightning:
                        var lightningBalance = await _lightningService.GetWalletBalance();
                        if (lightningBalance <= transaction.DestinationAmount)
                        {
                            response.StatusCode = (int)HttpStatusCode.BadRequest;
                            response.Message = "Insufficient balance. Kindly contact support\"";
                            return response;
                        }
                        var lightningPayment = await _lightningService.SendLightning(transaction.DestinationAddress);
                        if (!lightningPayment.success)
                        {
                            response.StatusCode = (int)HttpStatusCode.BadRequest;
                            response.Message = $"Error sending payment. {lightningPayment.error}";
                            return response;
                        }
                        break;
                    case Core.Enums.PaymentModeType.Fiat:
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        response.Message = "Cannot process fiat transaction";
                        return response;
                    default:
                        break;
                }

                transaction.TransactionStatus = Core.Enums.TransactionStatus.Success;
                _context.Transactions.Update(transaction);
                await _context.SaveChangesAsync(cancellationToken);
                response.Succeeded = true;
                response.Message = $"Remittance was finalized sucessfully";
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
