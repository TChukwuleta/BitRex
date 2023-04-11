using BitRex.Core.Model;
using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitRex.Application.Service.Queries
{
    public class GetBitcoinServiceChargesQuery : IRequest<Response<string>>
    {
    }

    public class GetBitcoinServiceChargesQueryHandler : IRequestHandler<GetBitcoinServiceChargesQuery, Response<string>>
    {
        private readonly IConfiguration _config;
        public GetBitcoinServiceChargesQueryHandler(IConfiguration config)
        {
            _config = config;
        }

        public Task<Response<string>> Handle(GetBitcoinServiceChargesQuery request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
