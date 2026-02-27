// See https://aka.ms/new-console-template for more information
using ConsoleApp1;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using PrettyPrompt;


//Console.CancelKeyPress += (o, e) => { e.};

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddFilter<ConsoleLoggerProvider>(l => false);
//builder.Services.AddLogging(,);
builder.Services.AddHostedService<Job>();
builder.Services.AddPrompt(config =>
{
    config.PersistentHistoryFilepath = "./history-file";
    config.Configuration = new(prompt: new(">>> "));
});
//builder.Services.AddSingleton<IPromptOptions, PromptOptions>();
//builder.Services.AddSingleton<IPrompt, Prompt>();
//builder.Logging.AddFilter

//builder.Logging.AddConsole(o => { });

//builder.Configuration.Get

var app = builder.Build();
//IHostLifetime

app.Run();

internal static class Ext
{
    extension(IServiceCollection sc)
    {
        public IServiceCollection AddPrompt(Action<PromptOptions> config)
        {
            sc.Configure(config);
            sc.AddSingleton<IPrompt>(sp => 
            {
                var config = sp.GetRequiredService<IOptions<PromptOptions>>().Value;
                return new Prompt(
                    config.PersistentHistoryFilepath,
                    config.Callbacks,
                    config.Console,
                    config.Configuration);
            });
            return sc;
        }
        public IServiceCollection AddPrompt() => sc.AddPrompt(_ => { });
    }
}
