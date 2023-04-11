namespace BitRex.Application.Common.Model.Response.BitcoinCommandResponses
{
    public class MultisigAddressCreationResponse
    {
        public MultisigResult result { get; set; }
        public object error { get; set; }
        public string id { get; set; }
    }

    public class MultisigResult
    {
        public string address { get; set; }
        public string redeemScript { get; set; }
        public string descriptor { get; set; }
    }
}
