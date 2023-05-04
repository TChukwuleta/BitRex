using BitRex.Core.Model;
using Newtonsoft.Json.Linq;

namespace BitRex.Application.Common.Interfaces
{
    public interface IBitcoinCoreClient
    {
        Task<bool> ValidateBitcoinAddress(string address);
        Task<string> MakePayment(BitRex.Core.Entities.Transaction transaction);
        Task<long> GetWalletBalance();
        Task<string> GenerateNewAddress();
        Task<(bool success, string response)> SwapBitcoinAddress(string address, decimal amount, string lightningPayment);
        Task<long> BitcoinToLnBtcSwap();
        Task<string> PayBitcoin(string address, decimal amount);
        Task<(bool success, string message)> BitcoinAddressTransactionConfirmation(string address, string amount);
        Task<(bool success, string message)> PayHtlcAndRedeemScript(string address, string txid, string invoice);
    }
}
