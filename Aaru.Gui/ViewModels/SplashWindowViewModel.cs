using Avalonia.Threading;
using ReactiveUI;

namespace Aaru.Gui.ViewModels
{
    public class SplashWindowViewModel : ViewModelBase
    {
        double _currentProgress;
        double _maxProgress;
        string _message;

        public string Message
        {
            get => _message;
            set => this.RaiseAndSetIfChanged(ref _message, value);
        }

        public double MaxProgress
        {
            get => _maxProgress;
            set => this.RaiseAndSetIfChanged(ref _maxProgress, value);
        }

        public double CurrentProgress
        {
            get => _currentProgress;
            set => this.RaiseAndSetIfChanged(ref _currentProgress, value);
        }

        internal void OnOpened()
        {
            Message         = "Welcome to Aaru!";
            MaxProgress     = 2;
            CurrentProgress = 0;

            Dispatcher.UIThread.Post(InitializeConsole);
        }

        void InitializeConsole()
        {
            CurrentProgress++;
            Message = "Initializing console...";
        }
    }
}