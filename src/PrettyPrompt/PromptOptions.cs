using PrettyPrompt.Consoles;

namespace PrettyPrompt;

public class PromptOptions : IPromptOptions
{
    public virtual string? PersistentHistoryFilepath { get; set; }
    public virtual PromptCallbacks? Callbacks { get; set; }
    public virtual IConsole? Console { get; set; }
    public virtual PromptConfiguration? Configuration { get; set; }
}

public interface IPromptOptions
{
    string? PersistentHistoryFilepath => null;
    PromptCallbacks? Callbacks => null;
    IConsole? Console => null;
    PromptConfiguration? Configuration => null;
}
