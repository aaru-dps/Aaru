namespace Aaru.Core;

using System;
using global::Spectre.Console;

/// <summary>
/// Implement core operations using the Spectre console
/// </summary>
public static class Spectre
{
    /// <summary>
    /// Initializes a progress bar with a single spinner
    /// </summary>
    /// <param name="action">Action to execute in the progress bar</param>
    public static void ProgressSingleSpinner(Action<ProgressContext> action) => AnsiConsole.Progress().AutoClear(true).
        HideCompleted(true).Columns(new TaskDescriptionColumn(), new SpinnerColumn()).Start(action);
}