using BitRex.Application.Common.Interfaces;
using BitRex.Application.Swap.Commands;
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

                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                    var bitcoinRequest = new ListenForBitcoinSwapPaymentCommand();
                    var bitcoinTransactionInitiationReversal = new ListenForBitcoinSwapPaymentCommandHandler(_lightningService, _context, _bitcoinService);
                    await bitcoinTransactionInitiationReversal.Handle(bitcoinRequest, stoppingToken);
                    Console.WriteLine("Done running round for bitcoin service");

                    Console.WriteLine("About to start running round of background service");
                    /*await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                    var lightningRequest = new ListenForLightningSwapPaymentCommand();
                    var lightningTransactionInitiationReversal = new ListenForLightningSwapPaymentCommandHandler(_lightningService, _context, _bitcoinService);
                    await lightningTransactionInitiationReversal.Handle(lightningRequest, stoppingToken);
                    Console.WriteLine("Done running round for lightning service");*/

                    Console.WriteLine("Done running round of background service");
                }

            }
        }
    }
}
