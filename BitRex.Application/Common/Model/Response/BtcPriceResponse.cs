using GraphQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitRex.Application.Common.Model.Response
{
    public class BtcPriceResponse
    {
        [GraphQLMetadata("data")]
        public BtcPriceListData data { get; set; }
    }

    public class BtcPriceListData
    {
        [GraphQLMetadata("btcPriceList")]
        public List<BtcPriceList> btcPriceList { get; set; }
    }

    public class BtcPriceList
    {
        public int timestamp { get; set; }
        public Price price { get; set; }
    }

    public class Price
    {
        [GraphQLMetadata("price")]
        public long @base { get; set; }
        public int offset { get; set; }
        public string currencyUnit { get; set; }
        public string formattedAmount { get; set; }
    }
}
