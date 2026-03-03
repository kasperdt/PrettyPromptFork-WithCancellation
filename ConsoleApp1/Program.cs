// See https://aka.ms/new-console-template for more information
using ConsoleApp1.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using PrettyPrompt;
using PrettyPromptRepl;


//Console.CancelKeyPress += (o, e) => { e.};

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddFilter<ConsoleLoggerProvider>(l => false);
//builder.Services.AddLogging(,);
builder.Services.AddHostedService<Job2>();
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


