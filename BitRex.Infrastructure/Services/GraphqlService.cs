using BitRex.Application.Common.Interfaces;
using BitRex.Application.Common.Model.Response;
using BitRex.Core.Enums;
using Google.Protobuf.WellKnownTypes;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Microsoft.Extensions.Configuration;
using System;

namespace BitRex.Infrastructure.Services
{
    public class GraphqlService : IGraphqlService
    {
        private readonly IConfiguration _config;
        public GraphqlService(IConfiguration config)
        {
            _config = config;
        }
        public async Task<BtcPriceListData> BitcoinLatestPrise(PriceGraphRangeType rangetype)
        {
            var url = _config["GaloyUrl"];
            try
            {
                var client = new GraphQLHttpClient(url, new NewtonsoftJsonSerializer());
                string query = @"
                        query btcPriceList($range: PriceGraphRange!) {
                          btcPriceList(range: $range) {
                              timestamp
                              price {
                              base
                              offset
                              currencyUnit
                              formattedAmount
                            }
                          }
                        }
                    ";
                var request = new GraphQLRequest(query);
                request.Variables = new { range = rangetype };
                var response = await client.SendQueryAsync<BtcPriceListData>(request);
                if (response.Errors != null)
                {
                    throw new ArgumentException("An error occured while trying to retrieve bitcoin prices");
                }
                return response.Data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<decimal> GetPrices(PriceGraphRangeType request)
        {
            try
            {
                var latestPrices = await BitcoinLatestPrise(request);
                var priceList = latestPrices.btcPriceList;
                int count = 0;
                decimal amount = 0;
                foreach (var price in priceList)
                {
                    decimal.TryParse(price.price.formattedAmount, out decimal pricing);
                    amount += pricing;
                    count++;
                }
                var averagePrice = (decimal)Math.Ceiling(amount / count);
                return averagePrice;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
