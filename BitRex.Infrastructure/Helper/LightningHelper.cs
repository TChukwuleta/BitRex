using Google.Protobuf;
using Grpc.Core;
using Lnrpc;
using Microsoft.Extensions.Configuration;
using NBitcoin;
using System.Text;

namespace BitRex.Infrastructure.Helper
{
    public class LightningHelper
    {
        private readonly IConfiguration _config;
        private readonly string userMacaroonPath;
        private readonly string userSslCertificatePath;
        private readonly string userGRPCHost;
        private readonly string adminMacaroonPath;
        private readonly string adminSslCertificatePath;
        private readonly string adminGRPCHost;

        public LightningHelper(IConfiguration config)
        {
            _config = config;
            userMacaroonPath = _config["Lightning:UserMacaroonPath"];
            userSslCertificatePath = _config["Lightning:UserSslCertPath"];
            userGRPCHost = _config["Lightning:UserRpcHost"];
            adminMacaroonPath = _config["Lightning:AdminMacaroonPath"];
            adminSslCertificatePath = _config["Lightning:AdminSslCertPath"];
            adminGRPCHost = _config["Lightning:AdminRpcHost"];
        }

        public Lnrpc.Lightning.LightningClient GetUserClient()
        {
            var sslCreds = GetUserSslCredentials();
            var channel = new Grpc.Core.Channel(userGRPCHost, sslCreds);
            var client = new Lnrpc.Lightning.LightningClient(channel);
            return client;
        }

        public Lnrpc.Lightning.LightningClient GetAdminClient()
        {
            var sslCreds = GetAdminSslCredentials();
            var channel = new Grpc.Core.Channel(adminGRPCHost, sslCreds);
            return new Lnrpc.Lightning.LightningClient(channel);
        }

        public string GetUserMacaroon()
        {
            byte[] macaroonBytes = File.ReadAllBytes(userMacaroonPath);
            var macaroon = BitConverter.ToString(macaroonBytes).Replace("-", "");
            return macaroon;
        }

        public string GetAdminMacaroon()
        {
            byte[] macaroonBytes = File.ReadAllBytes(adminMacaroonPath);
            var macaroon = BitConverter.ToString(macaroonBytes).Replace("-", "");
            return macaroon;
        }

        public SslCredentials GetUserSslCredentials()
        {
            Environment.SetEnvironmentVariable("GRPC_SSL_CIPHER_SUITES", "HIGH+ECDSA");
            var cert = File.ReadAllText(userSslCertificatePath);
            var sslCreds = new SslCredentials(cert);
            return sslCreds;
        }

        public SslCredentials GetAdminSslCredentials()
        {
            Environment.SetEnvironmentVariable("GRPC_SSL_CIPHER_SUITES", "HIGH+ECDSA");
            var cert = File.ReadAllText(adminSslCertificatePath);
            var sslCreds = new SslCredentials(cert);
            return sslCreds;
        }

        public AddInvoiceResponse CreateBitcoinInvoice(long satoshi, string memo)
        {
            try
            {
                var client = GetUserClient();
                var invoice = new Invoice();
                invoice.CreationDate = DateTime.Now.Ticks;
                invoice.Expiry = DateTime.Now.AddHours(1).Ticks;
                invoice.Memo = memo;
                invoice.Value = satoshi; // Value in satoshis
                var metadata = new Metadata() { new Metadata.Entry("macaroon", GetUserMacaroon()) };
                var invoiceResponse = client.AddInvoice(invoice, metadata);
                return invoiceResponse;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public AddInvoiceResponse CreateAdminInvoice(long satoshi, string memo)
        {
            try
            {
                var client = GetAdminClient();
                var invoice = new Invoice();
                invoice.Memo = memo;
                invoice.Value = satoshi; // Value in satoshis
                var metadata = new Metadata() { new Metadata.Entry("macaroon", GetAdminMacaroon()) };
                var invoiceResponse = client.AddInvoice(invoice, metadata);
                return invoiceResponse;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        public AddInvoiceResponse CreateSwapInvoice(string hash, long satoshi, string memo)
        {
            try
            {
                var client = GetAdminClient();
                var invoice = new Invoice();
                invoice.Memo = memo;
                invoice.Expiry = 3600;
                invoice.PaymentRequest = hash;
                invoice.Value = satoshi; // Value in satoshis
                var metadata = new Metadata() { new Metadata.Entry("macaroon", GetAdminMacaroon()) };
                var invoiceResponse = client.AddInvoice(invoice, metadata);
                return invoiceResponse;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public AddInvoiceResponse TestSwapInvoice(string hash, long satoshi, string memo)
        {
            try
            {
                var client = GetAdminClient();
                var invoice = new Invoice();
                invoice.Memo = memo;
                invoice.Expiry = 3600;
                invoice.PaymentRequest = hash;
                invoice.Value = satoshi; // Value in satoshis
                var metadata = new Metadata() { new Metadata.Entry("macaroon", GetAdminMacaroon()) };
                var invoiceResponse = client.AddInvoice(invoice, metadata);
                return invoiceResponse;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        public async Task<(string paymentRequest, string hash, long expiraton)> TestSwap(long satoshi, byte[] hashByte, long expiry, string memo)
        {
            try
            {
                var client = GetAdminClient();
                var invoice = new Invoice();
                invoice.Memo = memo;
                invoice.Expiry = expiry;
                invoice.Value = satoshi; // Value in satoshis
                invoice.RHash = ByteString.CopyFrom(hashByte);
                var metadata = new Metadata() { new Metadata.Entry("macaroon", GetAdminMacaroon()) };
                var invoiceResponse = await client.AddInvoiceAsync(invoice, metadata);
                var request = new PayReqString
                {
                    PayReq = invoiceResponse.PaymentRequest
                };
                var decodedRequest = await client.DecodePayReqAsync(request, metadata);

                var paymentAddr = decodedRequest.PaymentAddr;
                var hash = decodedRequest.PaymentHash;
                return (invoiceResponse.PaymentRequest, hash, invoice.Expiry);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<string> ListInvoiceTestSwap(long satoshi)
        {
            try
            {
                var client = GetAdminClient();
                var invoice = new Invoice();
                invoice.Memo = "Submarine swap payment";
                invoice.Expiry = 3600;
                invoice.Value = satoshi; // Value in satoshis
                var metadata = new Metadata() { new Metadata.Entry("macaroon", GetAdminMacaroon()) };
                var invoiceResponse = await client.AddInvoiceAsync(invoice, metadata);
                var paymentHashByte = invoiceResponse.RHash.ToByteArray();
                var paymentHash = paymentHashByte.ToString();
                Console.WriteLine($"Payment hash: {paymentHash}");

                var sendResponse = new SendResponse
                {
                    PaymentPreimage = ByteString.CopyFrom(new uint256("your-preimage-here").ToBytes()),
                    PaymentHash = ByteString.CopyFrom(new uint256(paymentHash).ToBytes())
                };

                return paymentHash;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
