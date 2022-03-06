using System;
using Spectre.Console;

namespace Aaru.Core;

public static class Spectre
{
    public static void ProgressSingleSpinner(Action<ProgressContext> action) => AnsiConsole.Progress().AutoClear(true).HideCompleted(true).
        Columns(new TaskDescriptionColumn(), new SpinnerColumn()).Start(action);
}