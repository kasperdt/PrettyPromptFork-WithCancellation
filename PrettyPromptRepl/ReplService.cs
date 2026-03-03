using Microsoft.Extensions.Hosting;
using PrettyPrompt;
using PrettyPrompt.Consoles;
using PrettyPrompt.Highlighting;

namespace PrettyPromptRepl;

public abstract class ReplService(IPrompt prompt, IConsole console) : BackgroundService
{
    protected ReplService(IPrompt prompt) : this(prompt, new SystemConsole()) { }
    protected IConsole Console => console;
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (true)
        {
            try
            {
                if (stoppingToken.IsCancellationRequested)
                    break;
                var response = await prompt.ReadLineAsync(stoppingToken);
                if (response is KeyPressCallbackResult callback)
                    await OnKeyPressCallbackResult(callback.Text, callback.Output, callback, stoppingToken);
                else
                    await OnResponseAsync(response.SubmitKeyInfo, response.Text, stoppingToken);

            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }


    protected virtual FormattedString? Valediction => null;
    protected virtual FormattedString? Salutation => null;
    protected virtual FormattedString? CancellationSymbol => new FormattedString("^C", new ConsoleFormat(Bold: true));

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        if (CancellationSymbol is { } cancellationsymbol)
            console.WriteLine(cancellationsymbol);
        if (Valediction is { } valediction)
            console.WriteLine(valediction);
    }


    public override Task StartAsync(CancellationToken cancellationToken)
    {
        if (Salutation is { } salutation)
            console.WriteLine(salutation);
        return base.StartAsync(cancellationToken);
    }

    protected abstract Task OnResponseAsync(ConsoleKeyInfo submitKeyInfo, string text, CancellationToken stoppingToken);
    protected virtual async Task OnKeyPressCallbackResult(string input, string? output, KeyPressCallbackResult instance, CancellationToken stoppingToken) => await OnResponseAsync(instance.SubmitKeyInfo, input, stoppingToken);
}