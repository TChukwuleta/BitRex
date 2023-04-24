using BitRex.Application.Common.Interfaces;
using BitRex.Core.Model;
using MediatR;
using System.Net;

namespace BitRex.Application.Test
{
    public class TestFlowCommand : IRequest<Response<string>>
    {
        public string Address { get; set; }
        public decimal Amount { get; set; }
    }

    public class TestFlowCommandHandler : IRequestHandler<TestFlowCommand, Response<string>>
    {
        private readonly IBitcoinCoreClient _bitcoinCoreClient;
        public TestFlowCommandHandler(IBitcoinCoreClient bitcoinCoreClient)
        {
            _bitcoinCoreClient = bitcoinCoreClient;
        }

        public async Task<Response<string>> Handle(TestFlowCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var response = new Response<string> { Succeeded = true };
                var swap = await _bitcoinCoreClient.PayBitcoin(request.Address, request.Amount);
                response.StatusCode = (int)HttpStatusCode.OK;
                response.Message = "Swap address generated successfully";
                response.Data = "Sent";
                return response;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
