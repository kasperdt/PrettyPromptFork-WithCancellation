using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PrettyPrompt;

namespace PrettyPromptRepl;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddPrompt(Action<PromptOptions> config)
        {
            services.Configure(config);
            services.AddSingleton<IPrompt>(sp =>
            {
                var config = sp.GetRequiredService<IOptions<PromptOptions>>().Value;
                return new Prompt(
                    config.PersistentHistoryFilepath,
                    config.Callbacks,
                    config.Console,
                    config.Configuration);
            });
            return services;
        }
        public IServiceCollection AddPrompt() => services.AddPrompt(_ => { });
    }
}
