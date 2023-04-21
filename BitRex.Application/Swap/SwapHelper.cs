using BitRex.Application.Common.Interfaces;
using BitRex.Core.Entities;
using BitRex.Core.Enums;
using BitRex.Core.Model.Request;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitRex.Application.Swap
{
    internal class SwapHelper
    {
        private readonly IAppDbContext _context;
        private readonly IBitcoinCoreClient _bitcoinCoreClient;
        private readonly ILightningService _lightningService;
        public SwapHelper(IAppDbContext context, IBitcoinCoreClient bitcoinCoreClient, ILightningService lightningService)
        {
            _context = context;
            _bitcoinCoreClient = bitcoinCoreClient;
            _lightningService = lightningService;
        }

        public async Task<(bool success, string message)> ExchangeBitcoinForLightningHelper(string reference)
        {
            try
            {
                var transaaction = await _context.Transactions.FirstOrDefaultAsync(c => c.TransactionReference == reference);
                if (transaaction == null)
                {
                    return (false, "Transaction not found");
                }

                if (transaaction.TransactionStatus == TransactionStatus.Success)
                {
                    return (true, "This transaction has been completed already"); 
                }
                if (transaaction.TransactionStatus != TransactionStatus.Initiated)
                {
                    return (false, $"Transaction cannot be processed. The transaction was {transaaction.TransactionStatus.ToString()}");
                }

                var builder = Network.Main.CreateTransactionBuilder();

                /*var sendBtc = await _bitcoinCoreClient.BitcoinRequestServer();
                await _context.SaveChangesAsync(new CancellationToken());*/
                return (true, "Transaction creation was successful");
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        public async Task<(bool success, string message)> FinalizeBitCoinForLightning(string reference)
        {
            try
            {
                var transaction = await _context.Transactions.FirstOrDefaultAsync(c => c.TransactionReference == reference);
                if (transaction == null)
                {
                    return (false, "Transaction not found");
                }
                if (transaction.TransactionStatus == TransactionStatus.Success)
                {
                    return (true, "This transaction has been completed already");
                }
                if (transaction.TransactionStatus != TransactionStatus.Initiated)
                {
                    return (false, $"Transaction cannot be processed. The transaction was {transaction.TransactionStatus.ToString()}");
                }
                var confirmedTransaction = await _bitcoinCoreClient.BitcoinAddressTransactionConfirmation(transaction.SourceAddress, $"{transaction.SourceAmount}");
                if (!confirmedTransaction.success)
                {
                    var paidAmount = decimal.Parse(confirmedTransaction.message);
                    transaction.SourceAmount -= paidAmount;
                    _context.Transactions.Update(transaction);
                    await _context.SaveChangesAsync(new CancellationToken());
                    return (false, $"Money is not yet complete oo... Kindly balance up {transaction.SourceAmount}");
                }
                var sendLightning = await _lightningService.SendLightning(transaction.DestinationAddress);
                return (true, "Transaction creation was successful");
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<(bool success, string message)> FinalizeLightningForBitcoin()
        {
            try
            {
                var confirmedTransaction = await _lightningService.ListenForSettledInvoice();
                var transaction = await _context.Transactions.FirstOrDefaultAsync(c => c.TransactionReference == confirmedTransaction.Reference);
                if (transaction == null)
                {
                    return (false, "Transaction not found");
                }
                if (transaction.TransactionStatus == TransactionStatus.Success)
                {
                    return (true, "This transaction has been completed already");
                }
                if (transaction.TransactionStatus != TransactionStatus.Initiated)
                {
                    return (false, $"Transaction cannot be processed. The transaction was {transaction.TransactionStatus.ToString()}");
                }
                var sendLightning = await _bitcoinCoreClient.MakePayment(transaction);
                return (true, "Transaction creation was successful");
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
