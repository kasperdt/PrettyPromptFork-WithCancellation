

using PrettyPrompt;
using PrettyPrompt.Highlighting;
using PrettyPrompt.Consoles;
using PrettyPromptRepl;

namespace ConsoleApp1.Jobs;

public class Job2(IPrompt prompt) : ReplService(prompt)
{
    protected override FormattedString? Salutation => "Welcome!";

    protected override async Task OnResponseAsync(ConsoleKeyInfo submitKeyInfo, string text, CancellationToken stoppingToken)
    {
        Console.WriteLine(new FormattedString($"You entered: {text}", new ConsoleFormat(Foreground: AnsiColor.Green)));
    }

    protected override FormattedString? Valediction => "Goodbye!";
}

