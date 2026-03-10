using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Messaging.Abstractions;
using Messaging.Transport;
using Messaging.Transport.Hosting;

namespace Messaging.Pipes.Hosting
{
    /// <summary>
    /// DI registration helpers for adding the Messaging framework components
    /// into an ASP.NET Core host or worker service.
    /// </summary>
    public static class MessagingServiceCollectionExtensions
    {
        /// <summary>
        /// Register all necessary messaging services into the DI container.
        /// </summary>
        public static IServiceCollection AddMessaging(this IServiceCollection services)
        {
            // Core pipe services
            services.AddSingleton<IPipeSender, PipeSender>();
            services.AddSingleton<IPipeResponseStore, PipeResponseStore>();
            services.AddSingleton<IPipeFunctionInvoker, PipeFunctionInvoker>();
            services.AddSingleton<IPipeService, PipeService>();

            // PipeReceiver with factory to inject all dependencies properly
            services.AddSingleton<IPipeReceiver>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<PipeReceiver>>();
                var responseStore = sp.GetRequiredService<IPipeResponseStore>();
                var invoker = sp.GetRequiredService<IPipeFunctionInvoker>();
                
                // TODO: Pull from configuration in the future
                var pipeName = "HunterPipe";
                
                return new PipeReceiver(pipeName, logger, responseStore, invoker);
            });

            return services;
        }
    }
}
