namespace BitRex.Application.Common.Model.Response.BitcoinCommandResponses
{
    public class GetWalletResponse
    {
        public GetWallerInfoResult result { get; set; }
        public object error { get; set; }
        public string id { get; set; }
    }

    public class GetWallerInfoResult
    {
        public string walletname { get; set; }
        public int walletversion { get; set; }
        public string format { get; set; }
        public double balance { get; set; }
        public double unconfirmed_balance { get; set; }
        public double immature_balance { get; set; }
        public int txcount { get; set; }
        public int keypoololdest { get; set; }
        public int keypoolsize { get; set; }
        public string hdseedid { get; set; }
        public int keypoolsize_hd_internal { get; set; }
        public double paytxfee { get; set; }
        public bool private_keys_enabled { get; set; }
        public bool avoid_reuse { get; set; }
        public bool scanning { get; set; }
        public bool descriptors { get; set; }
    }
}
