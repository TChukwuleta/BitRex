using BitRex.Application.Common.Interfaces;
using BitRex.Application.Common.Model.Response;
using BitRex.Infrastructure.Helper;
using Grpc.Core;
using Lnrpc;
using Microsoft.Extensions.Configuration;

namespace BitRex.Infrastructure.Services
{
    public class LightningService : ILightningService
    {
        private readonly IConfiguration _config;
        public LightningService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<string> CreateInvoice(long satoshis, string message)
        {
            string paymentRequest = default;
            var helper = new LightningHelper(_config);
            try
            {
                var adminInvoice = helper.CreateAdminInvoice(satoshis, message);
                paymentRequest = adminInvoice.PaymentRequest;
                return paymentRequest;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<string> CreateSwapInvoice(string hash, long satoshis, string message)
        {
            string paymentRequest = default;
            var helper = new LightningHelper(_config);
            try
            {
                var adminInvoice = helper.CreateSwapInvoice(hash, satoshis, message);
                paymentRequest = adminInvoice.PaymentRequest;
                return paymentRequest;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> ValidateLightningAddress(string paymentRequest)
        {
            var helper = new LightningHelper(_config);
            try
            {
                var userClient = helper.GetUserClient();
                var request = new PayReqString
                {
                    PayReq = paymentRequest
                };
                var response = await userClient.DecodePayReqAsync(request, new Metadata() { new Metadata.Entry("macaroon", helper.GetUserMacaroon()) });
                if (response == null)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<(bool success, string message)> ConfirmLightningValue(string paymentRequest, decimal amount)
        {
            var helper = new LightningHelper(_config);
            try
            {
                var userClient = helper.GetUserClient();
                var request = new PayReqString
                {
                    PayReq = paymentRequest
                };
                var response = await userClient.DecodePayReqAsync(request, new Metadata() { new Metadata.Entry("macaroon", helper.GetUserMacaroon()) });
                if (response.NumSatoshis > Math.Ceiling(amount))
                {
                    return (false, "Invoice amount is greater than the fiat equivalent of sats to be paid.");
                }
                if ((response.NumSatoshis + 1) < Math.Ceiling(amount))
                {
                    return (false, "Invoice amount is less than the fiat equivalent of sats to be paid.");
                }
                return (true, "");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<long> GetChannelBalance()
        {
            long response = default;
            var helper = new LightningHelper(_config);
            var channelBalanceRequest = new ChannelBalanceRequest();
            try
            {
                var userClient = helper.GetUserClient();
                response = userClient.ChannelBalance(channelBalanceRequest, new Metadata() { new Metadata.Entry("macaroon", helper.GetUserMacaroon()) }).Balance;
                return response;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<long> GetWalletBalance()
        {
            var helper = new LightningHelper(_config);
            var walletBalanceRequest = new WalletBalanceRequest();
            long response = default;
            try
            {
                var userClient = helper.GetUserClient();
                response = userClient.WalletBalance(walletBalanceRequest, new Metadata() { new Metadata.Entry("macaroon", helper.GetUserMacaroon()) }).TotalBalance;
                return response;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<InvoiceSettlementResponse> ListenForSettledInvoice()
        {
            var settledInvoiceResponse = new InvoiceSettlementResponse();
            try
            {
                var helper = new LightningHelper(_config);
                var txnReq = new InvoiceSubscription();

                var adminClient = helper.GetAdminClient();
                var settledAdminInvoioce = adminClient.SubscribeInvoices(txnReq, new Metadata() { new Metadata.Entry("macaroon", helper.GetAdminMacaroon()) });
                using (var call = settledAdminInvoioce)
                {
                    while (await call.ResponseStream.MoveNext())
                    {
                        var invoice = call.ResponseStream.Current;
                        if (invoice.State == Invoice.Types.InvoiceState.Settled)
                        {
                            Console.WriteLine(invoice.ToString());
                            settledInvoiceResponse.PaymentRequest = invoice.PaymentRequest;
                            settledInvoiceResponse.IsKeysend = invoice.IsKeysend;
                            settledInvoiceResponse.Value = invoice.Value;
                            settledInvoiceResponse.Expiry = invoice.Expiry;
                            settledInvoiceResponse.Settled = invoice.Settled;
                            settledInvoiceResponse.SettledDate = invoice.SettleDate;
                            settledInvoiceResponse.SettledIndex = (long)invoice.SettleIndex;
                            settledInvoiceResponse.Private = invoice.Private;
                            settledInvoiceResponse.AmountInSat = invoice.AmtPaidSat;
                            settledInvoiceResponse.Reference = invoice.Memo;
                            return settledInvoiceResponse;
                        }
                    }
                }
                return settledInvoiceResponse;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<string> SendLightning(string paymentRequest)
        {
            string result = default;
            var helper = new LightningHelper(_config);
            var sendRequest = new SendRequest();
            var paymentReq = new PayReqString();
            var walletBalance = await GetWalletBalance();
            try
            {
                var userClient = helper.GetUserClient();
                paymentReq.PayReq = paymentRequest;
                var decodedPaymentReq = userClient.DecodePayReq(paymentReq, new Metadata() { new Metadata.Entry("macaroon", helper.GetUserMacaroon()) });
                if (walletBalance < decodedPaymentReq.NumSatoshis)
                {
                    throw new ArgumentException("Unable to complete lightning payment. Insufficient funds");
                }
                sendRequest.Amt = decodedPaymentReq.NumSatoshis;
                sendRequest.PaymentRequest = paymentRequest;
                var response = userClient.SendPaymentSync(sendRequest, new Metadata() { new Metadata.Entry("macaroon", helper.GetUserMacaroon()) });
                result = response.PaymentError;
                var yo = response.PaymentPreimage.ToByteArray();
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<byte[]> TestSendLightning(string paymentRequest)
        {
            string result = default;
            var helper = new LightningHelper(_config);
            var sendRequest = new SendRequest();
            var paymentReq = new PayReqString();
            var walletBalance = await GetWalletBalance();
            try
            {
                var userClient = helper.GetUserClient();
                paymentReq.PayReq = paymentRequest;
                var decodedPaymentReq = userClient.DecodePayReq(paymentReq, new Metadata() { new Metadata.Entry("macaroon", helper.GetUserMacaroon()) });
                if (walletBalance < decodedPaymentReq.NumSatoshis)
                {
                    throw new ArgumentException("Unable to complete lightning payment. Insufficient funds");
                }
                sendRequest.Amt = decodedPaymentReq.NumSatoshis;
                sendRequest.PaymentRequest = paymentRequest;
                var response = userClient.SendPaymentSync(sendRequest, new Metadata() { new Metadata.Entry("macaroon", helper.GetUserMacaroon()) });
                result = response.PaymentError;
                if (!string.IsNullOrEmpty(result))
                {
                    throw new ArgumentException($"An error occured. {result}");
                }
                var yo = response.PaymentPreimage.ToByteArray();
                return yo;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
