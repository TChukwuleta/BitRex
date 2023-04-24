using BitRex.Application.Common.Interfaces;
using BitRex.Core.Enums;
using BitRex.Core.Model;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BitRex.Application.Swap.Commands
{
    public class ListenForBitcoinSwapPaymentCommand : IRequest<Result>
    {
    }

    public class ListenForBitcoinSwapPaymentCommandHandler : IRequestHandler<ListenForBitcoinSwapPaymentCommand, Result>
    {
        private readonly ILightningService _lightningService;
        private readonly IAppDbContext _context;
        private readonly IBitcoinCoreClient _bitcoinCoreClient;
        public ListenForBitcoinSwapPaymentCommandHandler(ILightningService lightningService, IAppDbContext context, IBitcoinCoreClient bitcoinCoreClient)
        {
            _lightningService = lightningService;
            _context = context;
            _bitcoinCoreClient = bitcoinCoreClient;
        }

        public async Task<Result> Handle(ListenForBitcoinSwapPaymentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                string reference = default;
                var transactions = await _context.Transactions.Where(c => c.SourcePaymentModeType == PaymentModeType.Lightning).ToListAsync();
                foreach (var txn in transactions)
                {
                    if (txn.TransactionStatus != TransactionStatus.Initiated)
                    {
                        continue;
                    }
                    switch (txn.DestinationPaymentModeType)
                    {
                        case PaymentModeType.Bitcoin:
                            do
                            {
                                var lightningPaymentConfirmation = await _lightningService.ListenForSettledInvoice();
                                reference = lightningPaymentConfirmation.Reference;
                            } while (txn.TransactionReference != reference);
                            var makeBitcoinPayment = await _bitcoinCoreClient.PayBitcoin(txn.DestinationAddress, txn.DestinationAmount);
                            break;
                        case PaymentModeType.Lightning:
                            var makeLightningPayment = await _lightningService.SendLightning(txn.DestinationAddress);
                            break;
                        case PaymentModeType.Fiat:
                            return Result.Failure("Cannot process fiat");
                        default:
                            return Result.Failure("Invalid payment mode type");
                    }
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
