using BitRex.Application.Remittance.Command;
using BitRex.Application.Swap.Commands;
using BitRex.Core.Model;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BitRex.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExchangeController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ExchangeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("exchangesummary")]
        public async Task<ActionResult<Response<object>>> ExchangeSummary(ExchangeSummaryCommand command)
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

        [HttpPost("createexchange")]
        public async Task<ActionResult<Response<object>>> RemittanceSummary(CreateExchangeCommand command)
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

        [HttpPost("confirmbitcoinpaymentandperformswap")]
        public async Task<ActionResult<Response<string>>> ConfirmBitcoinPayment(ConfirmBitcoinPaymentCommand command)
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

        [HttpPost("confirmlightningpaymentandperformswap")]
        public async Task<ActionResult<Result>> ConfirmLightningPayment(ListenForLightningSwapPaymentCommand command)
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