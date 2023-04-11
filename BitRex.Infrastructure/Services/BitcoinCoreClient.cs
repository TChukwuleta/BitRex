using BitRex.Application.Common.Interfaces;
using BitRex.Core.Model;
using Microsoft.Extensions.Configuration;
using NBitcoin;
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
        public BitcoinCoreClient(IConfiguration config)
        {
            _config = config;
            apiRequestDto = new ApiRequestDto();
            serverIp = _config["Bitcoin:URl"];
            username = _config["Bitcoin:username"];
            password = _config["Bitcoin:password"];
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

        private async Task<string> Something()
        {
            try
            {
                var key = new Key();
                Console.WriteLine(key.PubKey.ScriptPubKey.WitHash.ScriptPubKey);

                /*var newScript = Script
                Script redeemScript = PayToPubkeyHashTemplate.Instance*/
                var newPubKey = new Key().PubKey;
                var j = new Key();
                var publicKeyHash = new Key().PubKey.Hash;
                var bitcoinAddress = publicKeyHash.GetAddress(Network.Main);
                Console.WriteLine(publicKeyHash); // 41e0d7ab8af1ba5452b824116a31357dc931cf28
                Console.WriteLine(bitcoinAddress); // 171LGoEKyVzgQstGwnTHVh3TFTgo5PsqiY
                return "true";
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
