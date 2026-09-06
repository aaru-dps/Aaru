using Avalonia.Input;

namespace Aaru.Tui.Views.Dialogs;

public partial class GoToPathDialog : DialogWindowBase
{
    public GoToPathDialog()
    {
        InitializeComponent();
    }

    /// <inheritdoc />
    protected override IInputElement InitialFocusTarget => PathBox;
}