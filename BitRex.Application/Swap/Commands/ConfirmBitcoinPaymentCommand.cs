using BitRex.Application.Common.Interfaces;
using BitRex.Core.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace BitRex.Application.Swap.Commands
{
    public class ConfirmBitcoinPaymentCommand : IRequest<Response<string>>
    {
        public string TxId { get; set; }
        public string Address { get; set; }
    }

    public class ConfirmBitcoinPaymentCommandHandler : IRequestHandler<ConfirmBitcoinPaymentCommand, Response<string>>
    {
        private readonly IConfiguration _config;
        private readonly IBitcoinCoreClient _bitcoinCoreClient;
        private readonly ILightningService _lightningService;
        private readonly IAppDbContext _context;
        public ConfirmBitcoinPaymentCommandHandler(IConfiguration config, IBitcoinCoreClient bitcoinCoreClient, 
            ILightningService lightningService, IAppDbContext context)
        {
            _config = config;
            _bitcoinCoreClient = bitcoinCoreClient;
            _lightningService = lightningService;
            _context = context;
        }

        public async Task<Response<string>> Handle(ConfirmBitcoinPaymentCommand request, CancellationToken cancellationToken)
        {
            var response = new Response<string> { Succeeded = false };
            try
            {
                var findAddress = await _context.Transactions.FirstOrDefaultAsync(c => c.SourceAddress == request.Address);
                if (findAddress == null)
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "Invalid address specified";
                    return response;
                }

                if (findAddress.TransactionStatus != Core.Enums.TransactionStatus.Initiated)
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = $"Swap transaction has already been {findAddress.TransactionStatusDesc}(ed)";
                    return response;
                }
                var validatePayment = await _bitcoinCoreClient.PayHtlcAndRedeemScript(findAddress.SourceAddress, request.TxId);
                if (!validatePayment.success)
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = validatePayment.message;
                    return response;
                }
                findAddress.TransactionStatus = Core.Enums.TransactionStatus.Success;
                _context.Transactions.Update(findAddress);
                await _context.SaveChangesAsync(cancellationToken);
                response.Succeeded = true;
                response.Message = validatePayment.message;
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
