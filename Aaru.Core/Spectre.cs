namespace Aaru.Core;

using System;
using global::Spectre.Console;

public static class Spectre
{
    public static void ProgressSingleSpinner(Action<ProgressContext> action) => AnsiConsole.Progress().AutoClear(true).
        HideCompleted(true).Columns(new TaskDescriptionColumn(), new SpinnerColumn()).Start(action);
}