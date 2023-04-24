using BitRex.Application.Paystack.Commands;
using BitRex.Application.Test;
using BitRex.Core.Model;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BitRex.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExternalController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ExternalController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("test")]
        public async Task<ActionResult<Response<string>>> VerifyAndFinalizePayment(TestFlowCommand command)
        {
            try
            {
                return await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                return BadRequest($"{ex?.Message ?? ex?.InnerException.Message}");
            }
        }
    }
}
