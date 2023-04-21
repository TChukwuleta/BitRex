

using BitRex.Application.Common.Model.Response;

namespace BitRex.Application.Common.Interfaces
{
    public interface ILightningService
    {
        Task<string> CreateInvoice(long satoshis, string message);
        Task<long> GetChannelBalance();
        Task<long> GetWalletBalance();
        Task<string> SendLightning(string paymentRequest);
        Task<byte[]> TestSendLightning(string paymentRequest);
        Task<bool> ValidateLightningAddress(string paymentRequest);
        Task<string> CreateSwapInvoice(string hash, long satoshis, string message);
        Task<InvoiceSettlementResponse> ListenForSettledInvoice();
        Task<(bool success, string message)> ConfirmLightningValue(string paymentRequest, decimal amount);
    }
}
