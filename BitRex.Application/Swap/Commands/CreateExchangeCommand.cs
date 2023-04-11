using BitRex.Core.Enums;
using BitRex.Core.Model;
using MediatR;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitRex.Application.Swap.Commands
{
    internal class CreateExchangeCommand : IRequest<Response<object>>
    {
        public decimal Amount { get; set; }
        public string Destination { get; set; }
        public ExchangeType FromExchange { get; set; }
        public ExchangeType ToExchange { get; set; }
    }

    public class CreateExchangeCommandHandler : IRequestHandler<CreateExchangeCommand, Response<object>>
    {
        private readonly IConfiguration _config;
        public CreateExchangeCommandHandler(IConfiguration config)
        {
            _config = config;
        }

        Task<Response<object>> IRequestHandler<CreateExchangeCommand, Response<object>>.Handle(CreateExchangeCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
