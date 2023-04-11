namespace BitRex.Application.Common.Model.Response
{
    public class TxOutResponse
    {
        public string TransactionId { get; set; }
        public bool IsMature { get; set; }
        public long Height { get; set; }
        public decimal BalanceChange { get; set; }
        public bool Replaceable { get; set; }
        public string ReplacedBy { get; set; }
        public string Replacing { get; set; }
        public long Confirmation { get; set; }
        public List<Output> Outputs { get; set; }
        public List<Output> Inputs { get; set; }
    }

    public class Output
    {
        public string KeyPath { get; set; }
        public int Index { get; set; }
        public string ScriptPubKey { get; set; }
        public decimal Value { get; set; }
        public string Address { get; set; }
    }
}
