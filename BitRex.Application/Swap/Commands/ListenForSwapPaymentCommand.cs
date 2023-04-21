using BitRex.Application.Common.Interfaces;
using BitRex.Core.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BitRex.Application.Swap.Commands
{
    public class ListenForSwapPaymentCommand : IRequest<Result>
    {
    }

    public class ListenForSwapPaymentCommandHandler : IRequestHandler<ListenForSwapPaymentCommand, Result>
    {
        private readonly ILightningService _lightningService;
        private readonly IAppDbContext _context;
        private readonly IBitcoinCoreClient _bitcoinCoreClient;
        public ListenForSwapPaymentCommandHandler(ILightningService lightningService, IAppDbContext context, IBitcoinCoreClient bitcoinCoreClient)
        {
            _lightningService = lightningService;
            _context = context;
            _bitcoinCoreClient = bitcoinCoreClient;
        }

        public async Task<Result> Handle(ListenForSwapPaymentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var allTransactions = await _context.Transactions.ToListAsync();

                foreach (var txn in allTransactions)
                {

                }

                var response = await _lightningService.ListenForSettledInvoice();
                var transaction = await _context.Transactions.FirstOrDefaultAsync(c => c.TransactionReference == response.Reference);
                switch (transaction.DestinationPaymentModeType)
                {
                    case Core.Enums.PaymentModeType.Bitcoin:
                        var makeBitcoinPayment = await _bitcoinCoreClient.LnBtcToBitcoinSwap(transaction.DestinationAddress, transaction.DestinationAmount);
                        break;
                    case Core.Enums.PaymentModeType.Lightning:
                        var makeLightningPayment = await _lightningService.SendLightning(transaction.DestinationAddress);
                        break;
                    case Core.Enums.PaymentModeType.Fiat:
                        return Result.Failure("Cannot process fiat");
                    default:
                        return Result.Failure("Invalid payment mode type");
                }
                return Result.Success("Done performing the call");
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
