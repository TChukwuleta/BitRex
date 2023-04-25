using BitRex.Application.Common.Interfaces;
using BitRex.Core.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BitRex.Application.Swap.Commands
{
    public class ListenForLightningSwapPaymentCommand : IRequest<Result>
    {
    }

    public class ListenForLightningSwapPaymentCommandHandler : IRequestHandler<ListenForLightningSwapPaymentCommand, Result>
    {
        private readonly ILightningService _lightningService;
        private readonly IAppDbContext _context;
        private readonly IBitcoinCoreClient _bitcoinCoreClient;
        public ListenForLightningSwapPaymentCommandHandler(ILightningService lightningService, IAppDbContext context, IBitcoinCoreClient bitcoinCoreClient)
        {
            _lightningService = lightningService;
            _context = context;
            _bitcoinCoreClient = bitcoinCoreClient;
        }

        public async Task<Result> Handle(ListenForLightningSwapPaymentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _lightningService.ListenForSettledInvoice();
                var transaction = await _context.Transactions.FirstOrDefaultAsync(c => c.TransactionReference == response.Reference);
                switch (transaction.DestinationPaymentModeType)
                {
                    case Core.Enums.PaymentModeType.Bitcoin:
                        var makeBitcoinPayment = await _bitcoinCoreClient.PayBitcoin(transaction.DestinationAddress, transaction.DestinationAmount);
                        break;
                    case Core.Enums.PaymentModeType.Lightning:
                        return Result.Failure("Cannot process payment of the same type");
                        break;
                    case Core.Enums.PaymentModeType.Fiat:
                        return Result.Failure("Cannot process fiat");
                    default:
                        return Result.Failure("Invalid payment mode type");
                }
                return Result.Success("Done performing the swap");
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
