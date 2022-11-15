// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : App.xaml.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : GUI.
//
// --[ Description ] ----------------------------------------------------------
//
//     GUI initialization and globals.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

using System;
using Aaru.Gui.ViewModels.Windows;
using Aaru.Gui.Views.Windows;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace Aaru.Gui;

public sealed class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if(ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var splashWindow = new SplashWindow();
            var swvm         = new SplashWindowViewModel(splashWindow);
            swvm.WorkFinished        += OnSplashFinished;
            splashWindow.DataContext =  swvm;
            desktop.MainWindow       =  splashWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }

    void OnSplashFinished(object sender, EventArgs e)
    {
        if(ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return;

        // Ensure not exit
        desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

        // Close splash window
        desktop.MainWindow.Close();

        // Create and show main window
        desktop.MainWindow             = new MainWindow();
        desktop.MainWindow.DataContext = new MainWindowViewModel(desktop.MainWindow as MainWindow);
        desktop.MainWindow.Show();

        // Now can close when all windows are closed
        desktop.ShutdownMode = ShutdownMode.OnLastWindowClose;
    }

    void OnAboutClicked(object sender, EventArgs args)
    {
        if(ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime
           {
               MainWindow: MainWindow { DataContext: MainWindowViewModel mainWindowViewModel }
           })
            return;

        mainWindowViewModel.ExecuteAboutCommand();
    }

    void OnQuitClicked(object sender, EventArgs args)
    {
        if(ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime
           {
               MainWindow: MainWindow { DataContext: MainWindowViewModel mainWindowViewModel }
           })
            return;

        mainWindowViewModel.ExecuteExitCommand();
    }

    void OnPreferencesClicked(object sender, EventArgs args)
    {
        if(ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime
           {
               MainWindow: MainWindow { DataContext: MainWindowViewModel mainWindowViewModel }
           })
            return;

        mainWindowViewModel.ExecuteSettingsCommand();
    }
}