// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Text User Interface.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Iciclecreek.Avalonia.WindowManager;

namespace Aaru.Tui.Views.Dialogs;

/// <summary>Common behaviour for the TUI modal dialogs hosted by the managed window manager.</summary>
public abstract class DialogWindowBase : ManagedWindow
{
    protected DialogWindowBase()
    {
        // Open/close animations never finish under Consolonia's render loop, which leaves dialogs stuck open.
        AnimateWindow = false;

        Opened += (_, _) => Dispatcher.UIThread.Post(() => InitialFocusTarget?.Focus(), DispatcherPriority.Loaded);
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        // ManagedWindow adds a tunnelling KeyDown handler in its OnApplyTemplate that reacts to Enter/Escape by raising
        // the default/cancel button's Click event directly. That bypasses Button.OnClick, so a Command-bound button
        // never executes. Tunnelling handlers on the same element run newest first, so registering after the base
        // class makes this one run before it.
        AddHandler(KeyDownEvent, OnDialogKeyDown, RoutingStrategies.Tunnel);
    }

    /// <summary>Control that receives keyboard focus when the dialog opens, or null to keep the default.</summary>
    protected virtual IInputElement? InitialFocusTarget => null;

    void OnDialogKeyDown(object? sender, KeyEventArgs e)
    {
        if(e.Handled || e.KeyModifiers != KeyModifiers.None) return;

        Button? target = e.Key switch
                         {
                             Key.Enter  => FocusManager?.GetFocusedElement() as Button ?? FindButton(static b => b.IsDefault),
                             Key.Escape => FindButton(static b => b.IsCancel),
                             _          => null
                         };

        if(target is null) return;

        InvokeButton(target);
        e.Handled = true;
    }

    Button? FindButton(Func<Button, bool> predicate) =>
        this.GetVisualDescendants().OfType<Button>().FirstOrDefault(b => b.IsEffectivelyEnabled && predicate(b));

    /// <summary>Mirrors what <see cref="Button" /> does on a click: raise the event, then run the command.</summary>
    static void InvokeButton(Button button)
    {
        var args = new RoutedEventArgs(Button.ClickEvent);
        button.RaiseEvent(args);

        if(args.Handled) return;

        ICommand? command = button.Command;

        if(command?.CanExecute(button.CommandParameter) == true) command.Execute(button.CommandParameter);
    }
}
