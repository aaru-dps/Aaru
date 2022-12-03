// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : AboutViewModel.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI view models.
//
// --[ Description ] ----------------------------------------------------------
//
//     View model and code for the about dialog.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General public License for more details.
//
//     You should have received a copy of the GNU General public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Aaru.Gui.Models;
using Aaru.Gui.Views.Dialogs;
using Aaru.Localization;
using JetBrains.Annotations;
using ReactiveUI;

namespace Aaru.Gui.ViewModels.Dialogs;

public sealed class AboutViewModel : ViewModelBase
{
    readonly About _view;
    string         _versionText;

    public AboutViewModel(About view)
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
                string name = assembly.GetName().Name;

                string version =
                    (Attribute.GetCustomAttribute(assembly, typeof(AssemblyInformationalVersionAttribute)) as
                         AssemblyInformationalVersionAttribute)?.InformationalVersion;

                if(name is null ||
                   version is null)
                    continue;

                Assemblies.Add(new AssemblyModel
                {
                    Name    = name,
                    Version = version
                });
            }
        });
    }

    [NotNull]
    public string AboutLabel => UI.Label_About;
    [NotNull]
    public string LibrariesLabel => UI.Label_Libraries;
    [NotNull]
    public string AuthorsLabel => UI.Label_Authors;
    [NotNull]
    public string Title => UI.Title_About_Aaru;
    [NotNull]
    public string SoftwareName => "Aaru";
    [NotNull]
    public string SuiteName => "Aaru Data Preservation Suite";
    [NotNull]
    public string Copyright => "© 2011-2023 Natalia Portillo";
    [NotNull]
    public string Website => "https://aaru.app";
    [NotNull]
    public string License => UI.Label_License;
    [NotNull]
    public string CloseLabel => UI.ButtonLabel_Close;
    [NotNull]
    public string AssembliesLibraryText => UI.Title_Library;
    [NotNull]
    public string AssembliesVersionText => UI.Title_Version;
    [NotNull]
    public string Authors => UI.Text_Authors;
    public ReactiveCommand<Unit, Unit>         WebsiteCommand { get; }
    public ReactiveCommand<Unit, Unit>         LicenseCommand { get; }
    public ReactiveCommand<Unit, Unit>         CloseCommand   { get; }
    public ObservableCollection<AssemblyModel> Assemblies     { get; }

    public string VersionText
    {
        get => _versionText;
        set => this.RaiseAndSetIfChanged(ref _versionText, value);
    }

    static void ExecuteWebsiteCommand()
    {
        var process = new Process
        {
            StartInfo =
            {
                UseShellExecute = false,
                CreateNoWindow  = true,
                Arguments       = "https://aaru.app"
            }
        };

        if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            process.StartInfo.FileName  = "cmd";
            process.StartInfo.Arguments = $"/c start {process.StartInfo.Arguments.Replace("&", "^&")}";
        }
        else if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            process.StartInfo.FileName = "xdg-open";
        else if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            process.StartInfo.FileName = "open";
        else
            return;

        process.Start();
    }

    void ExecuteLicenseCommand()
    {
        var dialog = new LicenseDialog();
        dialog.DataContext = new LicenseViewModel(dialog);
        dialog.ShowDialog(_view);
    }

    void ExecuteCloseCommand() => _view.Close();
}