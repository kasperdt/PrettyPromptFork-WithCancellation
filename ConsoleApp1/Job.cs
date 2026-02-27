using Microsoft.Extensions.Hosting;
using PrettyPrompt;
using PrettyPrompt.Consoles;
using PrettyPrompt.Highlighting;

namespace ConsoleApp1;

public class Job() : ReplService(new Prompt("./history-file", configuration: new(prompt: "--> ")))
{

    protected override FormattedString? Salutation => "Welcome!";

    protected override async Task OnResponseAsync(ConsoleKeyInfo submitKeyInfo, string text, CancellationToken stoppingToken)
    {
        Console.WriteLine(new FormattedString($"You entered: {text}", new ConsoleFormat(Foreground: AnsiColor.Green)));
    }

    protected override FormattedString? Valediction => "Goodbye!";
}
