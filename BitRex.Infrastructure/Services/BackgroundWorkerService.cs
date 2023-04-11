using BitRex.Application.Common.Interfaces;
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
                    var _emailService = scope.ServiceProvider.GetService<IEmailService>();
                    var _configuration = scope.ServiceProvider.GetService<IConfiguration>();


                    //Call WorkRequests ExpiryCommand
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                    Console.WriteLine("Testing background service");

                }

            }
        }
    }
}
