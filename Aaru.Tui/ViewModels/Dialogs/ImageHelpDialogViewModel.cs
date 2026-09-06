using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Iciclecreek.Avalonia.WindowManager;

namespace Aaru.Tui.ViewModels.Dialogs;

public sealed class ImageHelpDialogViewModel : ViewModelBase
{
    readonly ManagedWindow _dialog;


    public ImageHelpDialogViewModel(ManagedWindow dialog)
    {
        _dialog   = dialog;
        OkCommand = new RelayCommand(Ok);
    }

    public ICommand OkCommand { get; }

    void Ok()
    {
        _dialog.Close(true);
    }
}