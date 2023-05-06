using BitRex.Application.Common.Interfaces;
using BitRex.Core.Model;
using MediatR;

namespace BitRex.Application.Swap
{
    public class TestCommand : IRequest<Result>
    {
        public decimal Amount { get; set; }
        public string Address { get; set; }
        public string Invoice { get; set; }
    }

    public class TestCommandHandler : IRequestHandler<TestCommand, Result>
    {
        private readonly IAppDbContext _context;
        private readonly ILightningService _lightningService;
        private readonly IBitcoinCoreClient _bitcoinCoreClient;
        public TestCommandHandler(IAppDbContext context, ILightningService lightningService, IBitcoinCoreClient bitcoinCoreClient)
        {
            _context = context;
            _lightningService = lightningService;
            _bitcoinCoreClient = bitcoinCoreClient;
        }

        public async Task<Result> Handle(TestCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var address = await _bitcoinCoreClient.SwapBitcoinAddress(request.Address, request.Amount, request.Invoice);
                if (!address.success)
                {
                    return Result.Failure("An error occured");
                }
                return Result.Success(address.response);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
