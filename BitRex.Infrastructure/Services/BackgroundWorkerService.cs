using BitRex.Application.Common.Interfaces;
using BitRex.Application.Paystack.Commands;
using BitRex.Application.Swap.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BitRex.Infrastructure.Services
{
    public class BackgroundWorkerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public BackgroundWorkerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }


        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {

                using (var scope = _serviceProvider.CreateScope())
                {
                    var _context = scope.ServiceProvider.GetService<IAppDbContext>();
                    var _configuration = scope.ServiceProvider.GetService<IConfiguration>();
                    var _lightningService = scope.ServiceProvider.GetService<ILightningService>();
                    var _bitcoinService = scope.ServiceProvider.GetService<IBitcoinCoreClient>();
                    var _paystackService = scope.ServiceProvider.GetService<IPaystackService>();

                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

                    Console.WriteLine("About to start running round of background service");

                    // Automate fiat settlement
                    var transactions = await _context.Transactions.Where(c => c.TransactionStatus == Core.Enums.TransactionStatus.Initiated && c.SourceAddress.ToLower().Contains("fiat")).ToListAsync();
                    if (transactions != null && transactions.Count() > 0)
                    {
                        Console.WriteLine("About to start running round for fiat settlement automation");
                        foreach (var transaction in transactions)
                        {
                            var fiatSettlementRequest = new VerifyPaystackCommand { Reference = transaction.TransactionReference };
                            var fiatSettlementHandler = new VerifyPaystackCommandHandler(_paystackService, _lightningService, _context, _bitcoinService);
                            await fiatSettlementHandler.Handle(fiatSettlementRequest, stoppingToken);
                        }
                        Console.WriteLine("Done running round for fiat settlement automation");
                    }

                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                    Console.WriteLine("About to start running round for bitcoin settlement automation");
                    var bitcoinRequest = new ListenForBitcoinSwapPaymentCommand();
                    var bitcoinTransactionInitiationReversal = new ListenForBitcoinSwapPaymentCommandHandler(_lightningService, _context, _bitcoinService);
                    //await bitcoinTransactionInitiationReversal.Handle(bitcoinRequest, stoppingToken);
                    Console.WriteLine("Done running round for bitcoin settlement automation");


                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                    /*
                     * Console.WriteLine("About to start running round for lightning settlement automation");
                     * var lightningRequest = new ListenForLightningSwapPaymentCommand();
                    var lightningTransactionInitiationReversal = new ListenForLightningSwapPaymentCommandHandler(_lightningService, _context, _bitcoinService);
                    await lightningTransactionInitiationReversal.Handle(lightningRequest, stoppingToken);
                    Console.WriteLine("Done running round for lightning settlement automation");*/

                    Console.WriteLine("Done running round of background service");
                }

            }
        }
    }
}
