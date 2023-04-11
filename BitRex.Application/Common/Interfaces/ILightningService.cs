

using BitRex.Application.Common.Model.Response;

namespace BitRex.Application.Common.Interfaces
{
    public interface ILightningService
    {
        Task<string> CreateInvoice(long satoshis, string message);
        Task<long> GetChannelBalance();
        Task<long> GetWalletBalance();
        Task<string> SendLightning(string paymentRequest);
        Task<InvoiceSettlementResponse> ListenForSettledInvoice();
    }
}
