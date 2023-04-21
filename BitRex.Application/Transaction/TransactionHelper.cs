using BitRex.Application.Common.Interfaces;
using BitRex.Core.Enums;
using BitRex.Core.Model.Request;

namespace BitRex.Application.Transaction
{
    internal class TransactionHelper
    {
        private readonly IAppDbContext _context;
        public TransactionHelper(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool success, string message)> CreateTransaction(CreateTransactionDto request)
        {
            try
            {
                var transaction = new Core.Entities.Transaction
                {
                    DestinationAddress = request.DestinationAddress,
                    DestinationPaymentModeType = request.DestinationPaymentModeType,
                    SourceAddress = request.SourceAddress,
                    SourcePaymentModeType = request.SourcePaymentModeType,
                    DestinationAmount = request.DestinationAmount,
                    SourceAmount = request.SourceAmount,
                    Hash = request.Hash,
                    TransactionStatus = request.TransactionStatus,
                    Narration = request.Narration,
                    TransactionReference = request.TransactionReference,
                    CreatedDate = DateTime.Now,
                    Status = Status.Active,
                    LastModifiedDate = DateTime.Now
                };
                await _context.Transactions.AddAsync(transaction);
                await _context.SaveChangesAsync(new CancellationToken());
                return (true, "Transaction creation was successful");
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
