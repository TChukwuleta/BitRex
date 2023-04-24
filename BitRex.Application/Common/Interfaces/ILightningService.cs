

using BitRex.Application.Common.Model.Response;

namespace BitRex.Application.Common.Interfaces
{
    public interface ILightningService
    {
        Task<string> CreateInvoice(long satoshis, string message);
        Task<long> GetChannelBalance();
        Task<long> GetWalletBalance();
        Task<(bool success, string error, string hash, byte[] preimage)> SendLightning(string paymentRequest);
        Task<byte[]> TestSendLightning(string paymentRequest);
        Task<(bool success, string hash, long expiry, long amount)> ValidateLightningAddress(string paymentRequest);
        Task<string> CreateSwapInvoice(string hash, long satoshis, string message);
        Task<InvoiceSettlementResponse> ListenForSettledInvoice();
        Task<string> SomeTestSendLightning(string paymentRequest, string hash);
        Task<(bool success, string message)> ConfirmLightningValue(string paymentRequest, decimal amount);
    }
}
