using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reflection;
using System.Threading.Tasks;
using Aaru.Gui.Models;
using Aaru.Gui.Views;
using Microsoft.DotNet.PlatformAbstractions;
using ReactiveUI;

namespace Aaru.Gui.ViewModels
{
    public class AboutDialogViewModel : ViewModelBase
    {
        readonly AboutDialog _view;
        string               _versionText;

        public AboutDialogViewModel(AboutDialog view)
        {
            _view = view;

            VersionText =
                (Attribute.GetCustomAttribute(typeof(App).Assembly, typeof(AssemblyInformationalVersionAttribute)) as
                     AssemblyInformationalVersionAttribute)?.InformationalVersion;

            WebsiteCommand = ReactiveCommand.Create(ExecuteWebsiteCommand);
            LicenseCommand = ReactiveCommand.Create(ExecuteLicenseCommand);
            CloseCommand   = ReactiveCommand.Create(ExecuteCloseCommand);

            Assemblies = new ObservableCollection<AssemblyModel>();

            Task.Run(() =>
            {
                foreach(Assembly assembly in AppDomain.CurrentDomain.GetAssemblies().OrderBy(a => a.FullName))
                {
                    string? name = assembly.GetName().Name;

                    string version =
                        (Attribute.GetCustomAttribute(assembly, typeof(AssemblyInformationalVersionAttribute)) as
                             AssemblyInformationalVersionAttribute)?.InformationalVersion;

                    if(name is null ||
                       version is null)
                        continue;

                    Assemblies.Add(new AssemblyModel
                    {
                        Name = name, Version = version
                    });
                }
            });
        }

        public string AboutLabel            => "About";
        public string LibrariesLabel        => "Libraries";
        public string AuthorsLabel          => "Authors";
        public string Title                 => "About Aaru";
        public string SoftwareName          => "Aaru";
        public string SuiteName             => "Aaru Data Preservation Suite";
        public string Copyright             => "© 2011-2020 Natalia Portillo";
        public string Website               => "https://aaru.app";
        public string License               => "License: GNU General Public License Version 3";
        public string CloseLabel            => "Close";
        public string AssembliesLibraryText => "Library";
        public string AssembliesVersionText => "Version";
        public string Authors => @"Developers:
    Natalia Portillo
    Michael Drüing
    Rebecca Wallander

Testers:
    Silas Laspada

Public relations:
    Noah Bacon

Logo and art:
    Juan Carlos Pastor Segura (Denymetanol)";
        public ReactiveCommand<Unit, Unit>         WebsiteCommand { get; }
        public ReactiveCommand<Unit, Unit>         LicenseCommand { get; }
        public ReactiveCommand<Unit, Unit>         CloseCommand   { get; }
        public ObservableCollection<AssemblyModel> Assemblies     { get; }

        public string VersionText
        {
            get => _versionText;
            set => this.RaiseAndSetIfChanged(ref _versionText, value);
        }

        void ExecuteWebsiteCommand()
        {
            var process = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false, CreateNoWindow = true, Arguments = "https://aaru.app"
                }
            };

            switch(RuntimeEnvironment.OperatingSystemPlatform)
            {
                case Platform.Unknown: return;
                case Platform.Windows:
                    process.StartInfo.FileName  = "cmd";
                    process.StartInfo.Arguments = $"/c start {process.StartInfo.Arguments.Replace("&", "^&")}";

                    break;
                case Platform.FreeBSD:
                case Platform.Linux:
                    process.StartInfo.FileName = "xdg-open";

                    break;
                case Platform.Darwin:
                    process.StartInfo.FileName = "open";

                    break;
                default:
                    if(Debugger.IsAttached)
                        throw new ArgumentOutOfRangeException();

                    return;
            }

            process.Start();
        }

        void ExecuteLicenseCommand()
        {
            var dialog = new LicenseDialog();
            dialog.DataContext = new LicenseDialogViewModel(dialog);
            dialog.ShowDialog(_view);
        }

        void ExecuteCloseCommand() => _view.Close();
    }
}