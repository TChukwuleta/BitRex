using BitRex.Application.Paystack.Commands;
using BitRex.Application.Remittance.Command;
using BitRex.Core.Model;
using BitRex.Core.Model.Response;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BitRex.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RemittanceController : ControllerBase
    {
        private readonly IMediator _mediator;
        public RemittanceController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("remittancesummary")]
        public async Task<ActionResult<Response<object>>> RemittanceSummary(RemittanceSummaryCommand command)
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

        [HttpPost("initiateremittance")]
        public async Task<ActionResult<Response<PaystackInitializationResponse>>> InitiateRemittance(CreateRemittancePaymentCommand command)
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

        [HttpPost("verifyandfinalizepayment")]
        public async Task<ActionResult<Response<string>>> VerifyAndFinalizePayment(VerifyPaystackCommand command)
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

        /*[HttpGet("getbitcointransactions/{userid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Result>> GetBitcoinTransactions(string userid)
        {
            try
            {
                accessToken.ValidateToken(userid);
                return await _mediator.Send(new GetAllBitcoinTransactionQuery { UserId = userid });
            }
            catch (Exception ex)
            {
                return Result.Failure($"Transactions retrieval by user failed. Error: {ex?.Message ?? ex?.InnerException?.Message}");
            }
        }*/
    }
}
