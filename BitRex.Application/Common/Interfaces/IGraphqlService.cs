using BitRex.Application.Common.Model.Response;
using BitRex.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq; 
using System.Text;
using System.Threading.Tasks;

namespace BitRex.Application.Common.Interfaces
{
    public interface IGraphqlService
    {
        Task<BtcPriceListData> BitcoinLatestPrise(PriceGraphRangeType rangetype);
        Task<decimal> GetPrices(PriceGraphRangeType request);
    }
}
