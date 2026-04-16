using Messaging.Abstractions;
using Messaging.Transport.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Messaging.Transport;

namespace Messaging.Pipes.Hosting
{
    /// <summary>
    /// PipeBootstrap
    /// --------------
    /// Lifecycle wrapper around <see cref="PipeReceiver"/>.
    ///
    /// Purpose:
    ///   - Hooks into Microsoft.Extensions.Hosting (IHostedService).
    ///   - Ensures PipeReceiver starts automatically when application starts.
    ///   - Cleanly shuts down when application exits.
    ///   - Simplifies DI setup for Hunter pipelines.
    ///
    /// Pattern: "Fixed framework"
    ///   - Pipe name is currently hardcoded ("HunterPipe").
    ///   - Factory creates a logger for PipeReceiver and wires invoker.
    /// </summary>
    public class PipeBootstrap : IHostedService
    {
        private readonly PipeReceiver _receiver;
        private readonly ILogger<PipeBootstrap> _logger;

        public PipeBootstrap(
            ILogger<PipeBootstrap> logger, 
            IPipeFunctionInvoker invoker,
            IPipeResponseStore responseStore)  // FIX: Add missing responseStore
        {
            _logger = logger;

            // Assign pipe name (could later pull from config).
            var pipeName = "HunterPipe";

            var receiverLogger = LoggerFactory.Create(builder => builder.AddConsole())
                                              .CreateLogger<PipeReceiver>();

            // FIX: Pass all 4 required arguments to PipeReceiver
            _receiver = new PipeReceiver(pipeName, receiverLogger, responseStore, invoker);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("PipeBootstrap starting receiver");
            return _receiver.StartAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("PipeBootstrap stopping receiver");
            _receiver.Stop(); // FIX: Call Stop() method on receiver
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// DI registration extensions.
    /// Allows consumers to do:
    ///    services.AddPipeServices();
    /// to get the invoker + hosted service registered.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPipeServices(this IServiceCollection services)
        {
            // TODO: Ensure PipeFunctionInvoker implementation is visible here.
            services.AddSingleton<IPipeFunctionInvoker, PipeFunctionInvoker>();
            services.AddSingleton<IPipeResponseStore, PipeResponseStore>(); // FIX: Add missing registration

            services.AddHostedService<PipeBootstrap>();
            return services;
        }
    }
}
