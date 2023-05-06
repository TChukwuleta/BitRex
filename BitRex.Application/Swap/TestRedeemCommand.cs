using BitRex.Application.Common.Interfaces;
using BitRex.Core.Model;
using MediatR;

namespace BitRex.Application.Swap
{
    public class TestRedeemCommand : IRequest<Result>
    {
        public string TxId { get; set; }
        public string Invoice { get; set; }
    }

    public class TestRedeemCommandHandler : IRequestHandler<TestRedeemCommand, Result>
    {
        private readonly IBitcoinCoreClient _bitcoinCoreClient;
        public TestRedeemCommandHandler(IBitcoinCoreClient bitcoinCoreClient)
        {
            _bitcoinCoreClient = bitcoinCoreClient;
        }

        public async Task<Result> Handle(TestRedeemCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var address = await _bitcoinCoreClient.TestPayHtlcAndRedeemScript(request.TxId, request.Invoice);
                if (!address.success)
                {
                    return Result.Failure("An error occured");
                }
                return Result.Success(address.message);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
