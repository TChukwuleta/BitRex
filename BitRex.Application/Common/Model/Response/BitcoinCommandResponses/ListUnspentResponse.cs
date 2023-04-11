namespace BitRex.Application.Common.Model.Response.BitcoinCommandResponses
{
    public class ListUnspentResponse
    {
        public List<UnspentOutput> result { get; set; }
        public string error { get; set; }
        public string id { get; set; }
    }

    public class UnspentOutput
    {
        public string txid { get; set; }
        public int vout { get; set; }
        public string address { get; set; }
        public string label { get; set; }
        public string scriptPubKey { get; set; }
        public double amount { get; set; }
        public int confirmations { get; set; }
        public bool spendable { get; set; }
        public bool solvable { get; set; }
        public string desc { get; set; }
        public bool safe { get; set; }
    }
}
