using BitRex.Application.Common.Interfaces;
using BitRex.Application.Common.Model.Response.BitcoinCommandResponses;
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
                var invoice = helper.CreateSwapInvoice(newPreImage, 300, "Submarine swap service");


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

        public async Task<string> PayBitcoin(string address, decimal amount)
        {
            try
            {
                var destinationAddress = BitcoinAddress.Create(address, _network); // recipient address
                var rpc = await CreateRpcClient();
                var changeAddress = await rpc.GetRawChangeAddressAsync(); // change address
                var senderAddress = BitcoinAddress.Create("bcrt1qd3nxk4tjpnzph4hrte660s80asqerhukqsy5gy", _network); // sender address
                var amt = Money.Satoshis(amount); // the amount you want to send
                //var privKey = await rpc.DumpPrivKeyAsync(senderAddress); // Private key associated with sender address
                // Get the unspent outputs for the sender's address
                var unspentSenderOutputs = rpc.ListUnspent().Where(u => u.Address == senderAddress).ToList();
                var coins = new List<UnspentCoin>();

                var transaction = _network.CreateTransaction(); // Create a new transaction
                var count = 0;
                foreach (var output in unspentSenderOutputs)
                {
                    if (count > 2)
                    {
                        break;
                    }
                    if (output.Amount > amt)
                    {
                        transaction.Inputs.Add(new TxIn(new OutPoint(output.OutPoint.Hash, output.OutPoint.N), senderAddress.ScriptPubKey));
                        coins.Add(output);
                        count++;
                    }
                }

                // Method 1
                var txBuilder = _network.CreateTransactionBuilder()
                    .SetVersion(1)
                    .AddCoins(coins.Select(c => c.AsCoin()))
                    .Send(destinationAddress, amt)
                    .SetChange(changeAddress)
                    .SendFees(Money.Satoshis(300))
                    //.AddKeys(privKey)
                    .BuildTransaction(true);
                var txHex = txBuilder.ToHex();
                // Test and see if the transaction would be accepted by the mempool
                var testTxn = await rpc.TestMempoolAcceptAsync(txBuilder);
                if (!testTxn.IsAllowed)
                {
                    throw new ArgumentException(testTxn.RejectReason);
                }
                //Send the transaction to the Bitcoin network
                var txId = await rpc.SendRawTransactionAsync(txBuilder);


                // Method 2
                // Add the recipient output to the transaction outputs
                /*transaction.Outputs.Add(new TxOut(amt, destinationAddress.ScriptPubKey));
                // Calculate the change and add it to the transaction outputs leave out 200 sats as miner fees.
                var totalInputAmount = coins.Sum(x => x.Amount);
                var changeAmount = totalInputAmount - (amount + 350);
                var change = Money.Satoshis(changeAmount);
                transaction.Outputs.Add(new TxOut(change, changeAddress.ScriptPubKey));
                // Sign the transaction inputs
                foreach (var input in transaction.Inputs)
                {
                    input.ScriptSig = senderAddress.ScriptPubKey;
                    input.WitScript = senderAddress.ScriptPubKey.WitHash.ScriptPubKey;
                }

                // Set the transaction version to 2 and include the appropriate witness flags
                transaction.Version = 1;
                transaction.Inputs.AsIndexedInputs().ToList().ForEach(x => x.WitScript = WitScript.Empty);*/

                // Test and see if the transaction would be accepted by the mempool
                /*var testTxn = await rpc.TestMempoolAcceptAsync(transaction);
                if (!testTxn.IsAllowed)
                {
                    throw new ArgumentException(testTxn.RejectReason);
                }
                //Send the transaction to the Bitcoin network
                var txId = await rpc.SendRawTransactionAsync(transaction);*/

                Console.WriteLine($"Transaction sent with ID: {txId}");
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

        public async Task<(bool success, string response)> SwapBitcoinAddress(string address, decimal amount, string lightningPayment)
        {
            try
            {
                Key refundKey = new Key(); // Generate a new key for refund address
                PubKey refundPubKey = refundKey.PubKey;

                var validateRequest = await _lightningService.ValidateLightningAddress(lightningPayment);
                if (!validateRequest.success)
                {
                    return (false, "invalid lightning address");
                }
                var rpc = await CreateRpcClient();
                var bitcoinAddress = BitcoinAddress.Create(address, _network);
                var swapAddress = await rpc.GetRawChangeAddressAsync();
                var amt = new Money(amount, MoneyUnit.Satoshi);

                // Set a locktime for the refund transaction
                uint locktime = Utils.DateTimeToUnixTime(DateTime.UtcNow.AddSeconds(validateRequest.expiry));

                //2. Calculate the hash of the secret:
                byte[] hash = Hashes.RIPEMD160(Encoding.UTF8.GetBytes(validateRequest.hash));

                /*4. Generate a Bitcoin script: Create a script that locks the Bitcoin funds until 
                the payment hash is revealed.This script will include an HTLC(Hash Time Locked Contract) that ensures that either 
                the Lightning Network user or the Bitcoin user can claim the funds, but not both.*/

                Script swapScript = new Script(
                    // OP_HASH160 <hash> OP_EQUAL
                    OpcodeType.OP_HASH160,
                    Op.GetPushOp(hash),
                    OpcodeType.OP_EQUAL,
                    // OP_IF <swapProviderPubKey> OP_ELSE <locktime> OP_CHECKLOCKTIMEVERIFY OP_DROP <refundPubKey> OP_ENDIF OP_CHECKSIG
                    OpcodeType.OP_IF,
                    Op.GetPushOp(swapAddress.ScriptPubKey.ToBytes()),
                    OpcodeType.OP_ELSE,
                    Op.GetPushOp(locktime),
                    OpcodeType.OP_CHECKLOCKTIMEVERIFY,
                    OpcodeType.OP_DROP,
                    Op.GetPushOp(bitcoinAddress.ScriptPubKey.ToBytes()),
                    OpcodeType.OP_ENDIF,
                    OpcodeType.OP_CHECKSIG
                );

                var addrScript = swapScript.Hash.ScriptPubKey;
                var addr = addrScript.GetDestinationAddress(_network);
                return (true, addr.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<(bool success, string message)> ConfirmAddressTransaction(string address, string txid, string invoice)
        {
            try
            {
                var rpc = await CreateRpcClient();
                // Get the address you want to check
                BitcoinAddress userAddress = BitcoinAddress.Create(address, _network);
                var refundAddress = await rpc.GetNewAddressAsync();

                uint256.TryParse(txid, out uint256 txId);
                // Wait for the funding transaction to confirm
                while (true)
                {
                    var txInfo = await rpc.GetTxOutAsync(txId, 0);
                    if (txInfo != null)
                        if (txInfo.Confirmations > 0)
                        {
                            break;
                        }
                    await Task.Delay(1000);
                }

                var validateRequest = await _lightningService.ValidateLightningAddress(invoice);
                var payinvoice = await _lightningService.SendLightning(invoice);


                // Get the transaction output of the swap transaction
                var fundingTx = await rpc.GetRawTransactionAsync(txId);
                var outputIndex = 0; // change to the index of the swap output in the transaction
                var fundingOutput = fundingTx.Outputs[outputIndex];

                // Get the script from the funding transaction output
                var script = fundingOutput.ScriptPubKey;
                NBitcoin.Op[] opss = (NBitcoin.Op[])script.ToOps();

                PubKey refundPubKey = null;
                Script redeemScript = null;
                bool isRedeemScript = false;
                for (int i = 0; i < opss.Length; i++)
                {
                    if (opss[i].Code == OpcodeType.OP_DROP)
                    {
                        // Get the next item in the script which should be the refund public key
                        var item = opss[++i];
                        if (item.PushData != null) //&& item.PushData.Length == 33
                        {
                            refundPubKey = new PubKey(item.PushData);
                        }
                    }
                }
                foreach (var op in script.ToOps())
                {
                    if (op.Code == OpcodeType.OP_IF)
                    {
                        isRedeemScript = true;
                    }
                    else if (op.Code == OpcodeType.OP_ENDIF)
                    {
                        isRedeemScript = false;
                    }
                    else if (isRedeemScript)
                    {
                        redeemScript += op;
                    }
                }

                // Create the transaction to redeem the swap output and spend the funds
                var redeemTx = _network.CreateTransaction();
                redeemTx.Inputs.Add(new TxIn(new OutPoint(txId, outputIndex), script));
                // Create the output to refund the funds to
                var refundTxOut = new TxOut(fundingOutput.Value, refundPubKey);
                // Add the output to the transaction
                redeemTx.Outputs.Add(refundTxOut);
                // Sign the transaction with the private key
                redeemTx.Inputs[0].ScriptSig = script;
                // Broadcast the transaction
                var result = await rpc.SendRawTransactionAsync(redeemTx);
                Console.WriteLine("Transaction sent: " + result);
                return (true, "Submarine swap completed");
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
