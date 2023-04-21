using BitRex.Application.Common.Interfaces;
using BitRex.Core.Model;
using BitRex.Infrastructure.Helper;
using Microsoft.Extensions.Configuration;
using NBitcoin;
using NBitcoin.Crypto;
using NBitcoin.RPC;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;

namespace BitRex.Infrastructure.Services
{
    public class BitcoinCoreClient : IBitcoinCoreClient
    {
        private readonly IConfiguration _config;
        public ApiRequestDto apiRequestDto { get; set; }
        private readonly string serverIp;
        private readonly string username;
        private readonly string password;
        private readonly string walletname;
        private readonly ILightningService _lightningService;
        private readonly Network _network;

        public BitcoinCoreClient(IConfiguration config, ILightningService lightningService)
        {
            _config = config;
            _lightningService = lightningService;
            apiRequestDto = new ApiRequestDto();
            serverIp = _config["Bitcoin:URL"];
            username = _config["Bitcoin:username"];
            password = _config["Bitcoin:password"];
            walletname = _config["Bitcoin:wallet"];
            _network = Network.RegTest;
        }

        public async Task<string> BitcoinRequestServer(string walletname, string methodName, List<JToken> parameters, int count)
        {
            string response = default;
            var url = serverIp;
            if (!string.IsNullOrEmpty(walletname))
            {
                url = $"{url}/wallet/{walletname}";
            }
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                webRequest.Credentials = new NetworkCredential(username, password);
                webRequest.Method = "POST";
                webRequest.ContentType = "application/json-rpc";

                JObject joe = new JObject();
                joe.Add(new JProperty("jsonrpc", "1.0"));
                joe.Add(new JProperty("id", "curltest"));
                joe.Add(new JProperty("method", methodName));
                JArray props = new JArray();
                foreach (var parameter in parameters)
                {
                    props.Add(parameter);
                }
                JArray paramsProps = new JArray();
                paramsProps.Add(count);
                paramsProps.Add(props);
                joe.Add(new JProperty("params", paramsProps));

                string s = JsonConvert.SerializeObject(joe);
                byte[] byteArray = Encoding.UTF8.GetBytes(s);
                webRequest.ContentLength = byteArray.Length;
                Stream stream = webRequest.GetRequestStream();
                stream.Write(byteArray, 0, byteArray.Length);
                stream.Close();

                StreamReader streamReader = null;
                WebResponse webResponse = webRequest.GetResponse();
                streamReader = new StreamReader(webResponse.GetResponseStream(), true);
                response = streamReader.ReadToEnd();
                var data = JsonConvert.DeserializeObject(response).ToString();
                return data;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<string> BitcoinRequestServer(string walletname, string methodName, List<string> parameters, int count)
        {
            try
            {
                return await BitcoinRequestServer(walletname, methodName, parameters.Select(c => new JValue(c)).ToList<JToken>(), count);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<string> BitcoinRequestServer(string methodName, List<JToken> parameters)
        {
            string response = default;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(serverIp);
                webRequest.Credentials = new NetworkCredential(username, password);
                webRequest.Method = "POST";
                webRequest.ContentType = "application/json-rpc";

                JObject joe = new JObject();
                joe.Add(new JProperty("jsonrpc", "1.0"));
                joe.Add(new JProperty("id", "curltest"));
                joe.Add(new JProperty("method", methodName));
                JArray props = new JArray();
                foreach (var parameter in parameters)
                {
                    props.Add(parameter);
                }
                joe.Add(new JProperty("params", props));

                string s = JsonConvert.SerializeObject(joe);
                byte[] byteArray = Encoding.UTF8.GetBytes(s);
                webRequest.ContentLength = byteArray.Length;
                Stream stream = webRequest.GetRequestStream();
                stream.Write(byteArray, 0, byteArray.Length);
                stream.Close();

                StreamReader streamReader = null; 
                WebResponse webResponse = webRequest.GetResponse();
                streamReader = new StreamReader(webResponse.GetResponseStream(), true);
                response = streamReader.ReadToEnd();
                var data = JsonConvert.DeserializeObject(response).ToString();
                return data;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<string> BitcoinRequestServer(string methodName, List<string> parameters)
        {
            try
            {
                return await BitcoinRequestServer(methodName, parameters.Select(c => new JValue(c)).ToList<JToken>());
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<string> BitcoinRequestServer(string walletname, string methodName, string parameters)
        {
            try
            {
                var url = serverIp;
                if (!string.IsNullOrEmpty(walletname))
                {
                    url = $"{url}/wallet/{walletname}";
                }
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                webRequest.Credentials = new NetworkCredential(username, password);
                webRequest.ContentType = "application/json-rpc";
                webRequest.Method = "POST";
                string responseValue = string.Empty;
                JObject joe = new JObject();
                joe.Add(new JProperty("jsonrpc", "1.0"));
                joe.Add(new JProperty("id", "curltext"));
                joe.Add(new JProperty("method", methodName));
                JArray props = new JArray();
                props.Add(parameters);
                joe.Add(new JProperty("params", props));
                // Serialize json for request
                string s = JsonConvert.SerializeObject(joe);
                byte[] byteArray = Encoding.UTF8.GetBytes(s);
                webRequest.ContentLength = byteArray.Length;
                Stream dataStream = webRequest.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                // Deserialize the response
                StreamReader streamReader = null;
                WebResponse webResponse = webRequest.GetResponse();
                streamReader = new StreamReader(webResponse.GetResponseStream(), true);
                responseValue = streamReader.ReadToEnd();
                var data = JsonConvert.DeserializeObject(responseValue).ToString();
                return data;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<string> BitcoinRequestServer(string methodName)
        {
            string response = default;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(serverIp);
                webRequest.Credentials = new NetworkCredential(username, password);
                webRequest.Method = "POST";
                webRequest.ContentType = "application/json-rpc";
                JObject joe = new JObject();
                JArray props = new JArray();
                joe.Add(new JProperty("jsonrpc", "1.0"));
                joe.Add(new JProperty("id", "curltext"));
                joe.Add(new JProperty("method", methodName));
                joe.Add(new JProperty("params", props));
                string s = JsonConvert.SerializeObject(joe);
                byte[] bytes = Encoding.UTF8.GetBytes(s);
                webRequest.ContentLength = bytes.Length;
                Stream stream = webRequest.GetRequestStream();
                stream.Write(bytes, 0, bytes.Length);
                stream.Close();

                StreamReader streamReader = null;
                WebResponse webResponse = webRequest.GetResponse();
                streamReader = new StreamReader(webResponse.GetResponseStream(), true);
                response = streamReader.ReadToEnd();
                var data = JsonConvert.DeserializeObject(response).ToString();
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<string> BitcoinRequestServer(string methodName, string parameters, int value)
        {
            string response = default;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(serverIp);
                webRequest.Credentials = new NetworkCredential(username, password);
                webRequest.Method = "POST";
                webRequest.ContentType = "application/json-rpc";
                JObject joe = new JObject();
                JArray props = new JArray();
                joe.Add(new JProperty("jsonrpc", "1.0"));
                joe.Add(new JProperty("id", "curltext"));
                joe.Add(new JProperty("method", methodName));
                props.Add(value);
                props.Add(parameters);
                joe.Add(new JProperty("params", props));
                string s = JsonConvert.SerializeObject(joe);
                byte[] bytes = Encoding.UTF8.GetBytes(s);
                webRequest.ContentLength = bytes.Length;
                Stream stream = webRequest.GetRequestStream();
                stream.Write(bytes, 0, bytes.Length);
                stream.Close();

                StreamReader streamReader = null;
                WebResponse webResponse = webRequest.GetResponse();
                streamReader = new StreamReader(webResponse.GetResponseStream(), true);
                response = streamReader.ReadToEnd();
                var data = JsonConvert.DeserializeObject(response).ToString();
                return data;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<bool> ValidateBitcoinAddress(string address)
        {
            try
            {
                var validateAddress = BitcoinAddress.Create(address, _network);
                if (validateAddress == null)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Invalid bitcoin address");
            }
        }

        public async Task<string> WalletInformation(string walletname, string methodname)
        {
            string response = default;
            var url = serverIp;
            if (!string.IsNullOrEmpty(walletname))
            {
                url = $"{serverIp}/wallet/{walletname}";
            }
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                webRequest.Credentials = new NetworkCredential(username, password);
                webRequest.Method = "POST";
                webRequest.ContentType = "application/json-rpc";
                JObject joe = new JObject();
                JArray props = new JArray();
                joe.Add(new JProperty("jsonrpc", "1.0"));
                joe.Add(new JProperty("id", "curltext"));
                joe.Add(new JProperty("method", methodname));
                joe.Add(new JProperty("params", props));
                string s = JsonConvert.SerializeObject(joe);
                byte[] bytes = Encoding.UTF8.GetBytes(s);
                webRequest.ContentLength = bytes.Length;
                Stream stream = webRequest.GetRequestStream();
                stream.Write(bytes, 0, bytes.Length);
                stream.Close();

                StreamReader streamReader = null;
                WebResponse webResponse = webRequest.GetResponse();
                streamReader = new StreamReader(webResponse.GetResponseStream(), true);
                response = streamReader.ReadToEnd();
                var data = JsonConvert.DeserializeObject(response).ToString();
                return data;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<long> GetWalletBalance()
        {
            try
            {
                var node = await CreateRpcClient();
                var balance = await node.GetBalanceAsync();
                return balance.Satoshi;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<string> MakePayment(Core.Entities.Transaction transaction)
        {
            try
            {
                var rpcClient = await CreateRpcClient();
                var money = new Money(transaction.DestinationAmount, MoneyUnit.BTC);
                var destinationAddress = BitcoinAddress.Create(transaction.DestinationAddress, _network);
                var sendMoney = await rpcClient.SendToAddressAsync(destinationAddress, money);
                return sendMoney.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task<RPCClient> CreateRpcClient()
        {
            try
            {
                var credential = new NetworkCredential
                {
                    UserName = username,
                    Password = password
                };
                var rpc = new RPCClient(credential, $"{serverIp}/wallet/{walletname}", _network);
                return rpc;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<long> WalletTransfer()
        {
            try
            {

                // Generate a preimage for the swap contract
                var preimage = new Key().PubKey.ToBytes(); // new Key().PubKey.ToHex();

                var newPreImage = new Key().PubKey.ToHex();

                // Generate a random payment hash for the swap contract
                var paymentHash = new uint256(Hashes.SHA256(preimage));

                // Create a new lightning invoice
                var helper = new LightningHelper(_config);
                var invoice = helper.CreateSwapInvoice(newPreImage, 300, "Sumarine swap service");


                var rpc = await CreateRpcClient();
                // Generate a Bitcoin address to receive funds for the swap
                var swapAddress = await rpc.GetNewAddressAsync();

                // Convert bitcoin address to a scriptpubkey
                var swapScriptPubKey = swapAddress.ScriptPubKey;


                var fundingTx = NBitcoin.Transaction.Create(_network);

                // construct the bitcoin funding transaction
                var fundingTxOut = new TxOut(Money.Coins(0.1m), swapScriptPubKey);

                // Add a locktime to the transaction to prevent it from being spent until a certain time has elapsed
                fundingTx.LockTime = new LockTime(DateTimeOffset.UtcNow.AddHours(1));

                fundingTx.Inputs[0].ScriptSig = swapScriptPubKey;
                fundingTx.Outputs.Add(fundingTxOut);

                await rpc.SendRawTransactionAsync(fundingTx);

                return 10;

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<long> BitcoinToLnBtcSwap()
        {
            try
            {
                var rpc = await CreateRpcClient();
                // Generate a Bitcoin address for refund
                var refundAddress = await rpc.GetNewAddressAsync();

                var received = await rpc.GetReceivedByAddressAsync(refundAddress);

                // Define the swap parameters
                var swapAmount = Money.Satoshis(100000); // Swap amount in satoshis
                var swapFeeRate = new FeeRate(Money.Satoshis(100)); // Fee rate for the swap transaction
                var swapPubKey = new Key().PubKey; // Public key for the swap transaction
                var swapLockTime = Utils.DateTimeToUnixTime(DateTime.UtcNow.AddHours(1)); // Locktime for the swap transaction

                // Create a Bitcoin transaction that funds the swap
                var txBuilder = _network.CreateTransactionBuilder();
                txBuilder.AddCoins((ICoin?[])await rpc.ListUnspentAsync());
                txBuilder.Send(swapPubKey, swapAmount);
                txBuilder.SetChange(swapPubKey);
                txBuilder.SendEstimatedFees(swapFeeRate);
                txBuilder.SetLockTime(swapLockTime);

                var unsignedTx = txBuilder.BuildTransaction(true);

                unsignedTx.Inputs[0].ScriptSig = swapPubKey.ScriptPubKey;
                var signedTx = txBuilder.SignTransaction(unsignedTx);
                return 10;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<string> LnBtcToBitcoinSwap(string address, decimal amount)
        {
            try
            {
                var destinationAddress = BitcoinAddress.Create(address, _network);
                var newPreImage = new Key().PubKey.ToHex();
                var helper = new LightningHelper(_config);
                var rpc = await CreateRpcClient();
                var refundAddress = await rpc.GetNewAddressAsync();
                var unspent = await rpc.ListUnspentAsync();
                var amt = Money.Satoshis(amount);
                //var fundingTxOut = new TxOut(amount, destinationAddress.ScriptPubKey.Hash);
                var txBuilder = _network.CreateTransactionBuilder()
                    .AddCoins(unspent.Select(c => c.AsCoin()))
                    .Send(destinationAddress.ScriptPubKey, amt)
                    .SendFees(Money.Satoshis(200))
                    .SetChange(refundAddress)
                    .BuildTransaction(true);
                /*txBuilder.Inputs[0].Sequence = new Sequence(0xFFFFFFFE);
                txBuilder.Inputs[0].ScriptSig = refundAddress.ScriptPubKey;*/
                var txId = await rpc.SendRawTransactionAsync(txBuilder);
                return txId.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
        }

        public async Task<string> GenerateNewAddress()
        {
            try
            {
                var rpc = await CreateRpcClient();
                var address = await rpc.GetNewAddressAsync();
                return address.ToString();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        public async Task<(bool success, string message)> BitcoinAddressTransactionConfirmation(string address, string amount)
        {
            try
            {
                var value = Money.Parse(amount);
                var validateAddress = await ValidateBitcoinAddress(address);
                if (!validateAddress)
                {
                    throw new ArgumentException("Invalid bitcoin address");
                }
                var rpc = await CreateRpcClient();
                var bitcoinAddress = BitcoinAddress.Create(address, _network);
                var addressInfo = rpc.GetAddressInfo(bitcoinAddress);
                var receivedMoney = rpc.GetReceivedByAddress(bitcoinAddress);
                if (receivedMoney < Money.Zero)
                {
                    throw new ArgumentException("No money received yet");
                }
                if (receivedMoney < value)
                {
                    return (false, receivedMoney.ToString());
                }
                return (true, "Confirmed");
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
