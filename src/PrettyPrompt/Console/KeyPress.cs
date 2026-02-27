#region License Header
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace PrettyPrompt.Consoles;

[DebuggerDisplay("{ObjectPattern}")]
public class KeyPress
{
    //public static bool IsCtrlC { get; set; }

    /// <summary>
    /// The key press as reported by Console.ReadKey
    /// </summary>
    public ConsoleKeyInfo ConsoleKeyInfo { get; }

    /// <summary>
    /// A tuple that represents the key press.
    /// Intended to be pattern matched, e.g. (A) or (Ctrl, A) or (Ctrl | Shift, A).
    /// It's either <see cref="ConsoleKey"/> or (<see cref="ConsoleModifiers"/>, <see cref="ConsoleKey"/>).
    /// </summary>
    public object ObjectPattern { get; }

    /// <summary>
    /// Text that was pasted as a result of this key press.
    /// </summary>
    public string? PastedText { get; }

    internal bool Handled { get; set; }

    public KeyPress(ConsoleKeyInfo consoleKeyInfo, string? pastedText = null)
    {
        if (consoleKeyInfo is { Key: ConsoleKey.Enter, KeyChar: '\r' })
        {
            ConsoleKeyInfo = new ConsoleKeyInfo(
                '\n', //'\r' is unexpected and makes problems (e.g. https://github.com/waf/CSharpRepl/issues/213)
                ConsoleKey.Enter,
                consoleKeyInfo.Modifiers.HasFlag(ConsoleModifiers.Shift),
                consoleKeyInfo.Modifiers.HasFlag(ConsoleModifiers.Alt),
                consoleKeyInfo.Modifiers.HasFlag(ConsoleModifiers.Control));
        }
        else
        {
            ConsoleKeyInfo = consoleKeyInfo;
        }

        ObjectPattern =
            consoleKeyInfo.Modifiers == 0 ?
            consoleKeyInfo.Key :
            (consoleKeyInfo.Modifiers, consoleKeyInfo.Key);

        PastedText = pastedText;
    }

    //internal static CancellationTokenSource? ReadKeyCancellationTokenSource { get; set; }
    
    internal static async Task<ConsoleKeyInfo> ReadKeyAsync(bool intercept = true, CancellationToken ct = default)
    {
        var c = 0;
        while (true)
        {
            ct.ThrowIfCancellationRequested();
            //if (ct.IsCancellationRequested)
            //    return null;
            if (Console.KeyAvailable)
                return Console.ReadKey(intercept);
            if (c++ < 5)
                Thread.Sleep(1);
            else if (c < 25)
                Thread.Sleep(5);
            else if (c < 40)
                Thread.Sleep(20);
            else if (c < 60)
                Thread.Sleep(50);
            else if (c < 70)
                Thread.Sleep(100);
            else if (c < 100)
                Thread.Sleep(200);
            else
                while(true)
                {
                    Thread.Sleep(500);
                    ct.ThrowIfCancellationRequested();
                    if (Console.KeyAvailable)
                        return Console.ReadKey(intercept);
                }
                
        }
    }
    internal static async IAsyncEnumerable<KeyPress> ReadForeverAsync(IConsole console, [EnumeratorCancellation] CancellationToken ct = default)
    {
        while (true)
        {
            //console.ReadKeyCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(ct);
            //ConsoleCancelEventHandler del = (o, e) =>
            //{
            //    e.Cancel = true;
            //    console.ReadKeyCancellationTokenSource.Cancel();
            //};
            ////console.CancelKeyPress += del;
            var key = await ReadKeyAsync(true, ct).ConfigureAwait(false);
            //console.CancelKeyPress -= del;
            //if (mkey is not { } key)
            //    break;

            if (!console.KeyAvailable)
            {
                yield return new KeyPress(key);
                continue;
            }

            // If the user pastes text, we see it as a bunch of key presses. We don't want to send
            // them all individually, as it will trigger syntax highlighting and potentially intellisense
            // for each key press, which is slow. Instead, batch them up to send as single "pasted text" block.
            var keys = ReadRemainingKeys(console, key);

            if (key.Key == ConsoleKey.Escape)
            {
                if (MapInputEscapeSequence(keys) is KeyPress ansiEscapedInput)
                {
                    yield return ansiEscapedInput;
                }
            }
            else if (keys.Count < 4 || keys.All(k => char.IsControl(k.KeyChar))) // 4 is not special here, just seemed like a decent number to separate
                                                                                 // between "keys pressed simultaneously" and "pasted text"
            {
                foreach (var consoleKey in keys)
                {
                    yield return new KeyPress(consoleKey);
                }
            }
            else
            {
                // we got a bunch of keypresses, send them as a paste event (Shift+Insert)
                yield return new KeyPress(
                    ConsoleKey.Insert.ToKeyInfo('\0', shift: true),
                    pastedText: new string(keys.Select(k => k.KeyChar).ToArray())
                );
            }
        }
    }

    /// <summary>
    /// On Linux, .NET doesn't map all the ANSI escaped inputs into ConsoleKeyInfos. Map some of the missing ones here.
    /// </summary>
    private static KeyPress? MapInputEscapeSequence(List<ConsoleKeyInfo> keys)
    {
        var sequence = new string(keys.Select(key => key.KeyChar).ToArray());
        return sequence switch
        {
            "\u001b1;5P" => new KeyPress(ConsoleKey.F1.ToKeyInfo('\0', control: true)),
            "\u001b1;5Q" => new KeyPress(ConsoleKey.F2.ToKeyInfo('\0', control: true)),
            "\u001b1;5R" => new KeyPress(ConsoleKey.F3.ToKeyInfo('\0', control: true)),
            "\u001b1;5S" => new KeyPress(ConsoleKey.F4.ToKeyInfo('\0', control: true)),
            "\u001b15;5~" => new KeyPress(ConsoleKey.F5.ToKeyInfo('\0', control: true)),
            "\u001b17;5~" => new KeyPress(ConsoleKey.F6.ToKeyInfo('\0', control: true)),
            "\u001b18;5~" => new KeyPress(ConsoleKey.F7.ToKeyInfo('\0', control: true)),
            "\u001b19;5~" => new KeyPress(ConsoleKey.F8.ToKeyInfo('\0', control: true)),
            "\u001b20;5~" => new KeyPress(ConsoleKey.F9.ToKeyInfo('\0', control: true)),
            "\u001b21;5~" => new KeyPress(ConsoleKey.F10.ToKeyInfo('\0', control: true)),
            "\u001b23;5~" => new KeyPress(ConsoleKey.F11.ToKeyInfo('\0', control: true)),
            "\u001b24;5~" => new KeyPress(ConsoleKey.F12.ToKeyInfo('\0', control: true)),
            _ => null
        };
    }

    /// <summary>
    /// Read any remaining key presses in the buffer, including the provided <paramref name="key"/>.
    /// </summary>
    private static List<ConsoleKeyInfo> ReadRemainingKeys(IConsole console, ConsoleKeyInfo key)
    {
        var keys = new List<ConsoleKeyInfo> { key };
        do
        {
            keys.Add(console.ReadKey(true));
        } while (console.KeyAvailable);

        return keys;
    }
}
