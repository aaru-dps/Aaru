// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : dlgSettings.xeto.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Settings dialog.
//
// --[ Description ] ----------------------------------------------------------
//
//     Implements the settings dialog.
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using DiscImageChef.Settings;
using Eto.Forms;
using Eto.Serialization.Xaml;

namespace DiscImageChef.Gui.Dialogs
{
    public class dlgSettings : Dialog
    {
        public dlgSettings(bool gdprChange)
        {
            XamlReader.Load(this);

            lblGdpr1.Text =
                "In compliance with the European Union General Data Protection Regulation 2016/679 (GDPR),\n"    +
                "we must give you the following information about DiscImageChef and ask if you want to opt-in\n" +
                "in some information sharing.";
            lblGdpr2.Text =
                "Disclaimer: Because DiscImageChef is an open source software this information, and therefore,\n" +
                "compliance with GDPR only holds true if you obtained a certificated copy from its original\n"    +
                "authors. In case of doubt, close DiscImageChef now and ask in our IRC support channel.";
            lblGdpr3.Text =
                "For any information sharing your IP address may be stored in our server, in a way that is not\n" +
                "possible for any person, manual, or automated process, to link with your identity, unless\n"     +
                "specified otherwise.";

            tabGdpr.Visible = gdprChange;

            #region Device reports
            lblSaveReportsGlobally.Text =
                "With the 'device-report' command, DiscImageChef creates a report of a device, that includes its\n"       +
                "manufacturer, model, firmware revision and/or version, attached bus, size, and supported commands.\n"    +
                "The serial number of the device is not stored in the report. If used with the debug parameter,\n"        +
                "extra information about the device will be stored in the report. This information is known to contain\n" +
                "the device serial number in non-standard places that prevent the automatic removal of it on a handful\n" +
                "of devices. A human-readable copy of the report in XML format is always created in the same directory\n" +
                "where DiscImageChef is being run from.";

            chkSaveReportsGlobally.Text =
                "Do you want to save device reports in shared folder of your computer? (Y/N): ";
            chkSaveReportsGlobally.Checked = Settings.Settings.Current.SaveReportsGlobally;

            lblShareReports.Text =
                "Sharing a report with us will send it to our server, that's in the european union territory, where it\n"      +
                "will be manually analized by an european union citizen to remove any trace of personal identification\n"      +
                "from it. Once that is done, it will be shared in our stats website, https://www.discimagechef.app\n"       +
                "These report will be used to improve DiscImageChef support, and in some cases, to provide emulation of the\n" +
                "devices to other open-source projects. In any case, no information linking the report to you will be stored.";
            chkShareReports.Text    = "Do you want to share your device reports with us? (Y/N): ";
            chkShareReports.Checked = Settings.Settings.Current.ShareReports;
            #endregion Device reports

            #region Statistics
            lblStatistics.Text =
                "DiscImageChef can store some usage statistics. These statistics are limited to the number of times a\n"        +
                "command is executed, a filesystem, partition, or device is used, the operating system version, and other.\n"   +
                "In no case, any information besides pure statistical usage numbers is stored, and they're just joint to the\n" +
                "pool with no way of using them to identify you.";
            chkSaveStats.Text = "Do you want to save stats about your DiscImageChef usage? (Y/N): ";

            if(Settings.Settings.Current.Stats != null)
            {
                chkSaveStats.Checked  = true;
                stkStatistics.Visible = true;

                chkShareStats.Text    = "Do you want to share your stats anonymously? (Y/N): ";
                chkShareStats.Checked = Settings.Settings.Current.Stats.ShareStats;

                chkBenchmarkStats.Text    = "Do you want to gather statistics about benchmarks? (Y/N): ";
                chkBenchmarkStats.Checked = Settings.Settings.Current.Stats.BenchmarkStats;

                chkCommandStats.Text    = "Do you want to gather statistics about command usage? (Y/N): ";
                chkCommandStats.Checked = Settings.Settings.Current.Stats.CommandStats;

                chkDeviceStats.Text    = "Do you want to gather statistics about found devices? (Y/N): ";
                chkDeviceStats.Checked = Settings.Settings.Current.Stats.DeviceStats;

                chkFilesystemStats.Text    = "Do you want to gather statistics about found filesystems? (Y/N): ";
                chkFilesystemStats.Checked = Settings.Settings.Current.Stats.FilesystemStats;

                chkFilterStats.Text    = "Do you want to gather statistics about found file filters? (Y/N): ";
                chkFilterStats.Checked = Settings.Settings.Current.Stats.FilterStats;

                chkMediaImageStats.Text =
                    "Do you want to gather statistics about found media image formats? (Y/N): ";
                chkMediaImageStats.Checked = Settings.Settings.Current.Stats.MediaImageStats;

                chkMediaScanStats.Text    = "Do you want to gather statistics about scanned media? (Y/N): ";
                chkMediaScanStats.Checked = Settings.Settings.Current.Stats.MediaScanStats;

                chkPartitionStats.Text =
                    "Do you want to gather statistics about found partitioning schemes? (Y/N): ";
                chkPartitionStats.Checked = Settings.Settings.Current.Stats.PartitionStats;

                chkMediaStats.Text    = "Do you want to gather statistics about media types? (Y/N): ";
                chkMediaStats.Checked = Settings.Settings.Current.Stats.MediaStats;

                chkVerifyStats.Text    = "Do you want to gather statistics about media image verifications? (Y/N): ";
                chkVerifyStats.Checked = Settings.Settings.Current.Stats.VerifyStats;
            }
            else
            {
                chkSaveStats.Checked  = false;
                stkStatistics.Visible = false;
            }
            #endregion Statistics
        }

        protected void OnBtnCancel(object sender, EventArgs e)
        {
            Close();
        }

        protected void OnBtnSave(object sender, EventArgs e)
        {
            Settings.Settings.Current.SaveReportsGlobally = chkSaveReportsGlobally.Checked == true;
            Settings.Settings.Current.ShareReports        = chkShareReports.Checked        == true;

            if(chkSaveStats.Checked == true)
                Settings.Settings.Current.Stats = new StatsSettings
                {
                    ShareStats      = chkShareStats.Checked      == true,
                    BenchmarkStats  = chkBenchmarkStats.Checked  == true,
                    CommandStats    = chkCommandStats.Checked    == true,
                    DeviceStats     = chkDeviceStats.Checked     == true,
                    FilesystemStats = chkFilesystemStats.Checked == true,
                    FilterStats     = chkFilterStats.Checked     == true,
                    MediaImageStats = chkMediaImageStats.Checked == true,
                    MediaScanStats  = chkMediaScanStats.Checked  == true,
                    PartitionStats  = chkPartitionStats.Checked  == true,
                    MediaStats      = chkMediaStats.Checked      == true,
                    VerifyStats     = chkVerifyStats.Checked     == true
                };
            else Settings.Settings.Current.Stats = null;

            Settings.Settings.Current.GdprCompliance = DicSettings.GdprLevel;
            Settings.Settings.SaveSettings();
            Close();
        }

        #region XAML controls
        Label       lblGdpr1;
        Label       lblGdpr2;
        Label       lblGdpr3;
        CheckBox    chkSaveReportsGlobally;
        CheckBox    chkShareReports;
        CheckBox    chkSaveStats;
        CheckBox    chkShareStats;
        CheckBox    chkBenchmarkStats;
        CheckBox    chkCommandStats;
        CheckBox    chkDeviceStats;
        CheckBox    chkFilesystemStats;
        CheckBox    chkFilterStats;
        CheckBox    chkMediaScanStats;
        CheckBox    chkPartitionStats;
        CheckBox    chkMediaStats;
        CheckBox    chkVerifyStats;
        TabPage     tabGdpr;
        TabPage     tabReports;
        Label       lblSaveReportsGlobally;
        Label       lblShareReports;
        TabPage     tabStatistics;
        StackLayout stkButtons;
        StackLayout stkStatistics;
        Label       lblStatistics;
        CheckBox    chkMediaImageStats;
        #endregion
    }
}