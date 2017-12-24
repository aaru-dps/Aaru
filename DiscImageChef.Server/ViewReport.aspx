<%@ Page Language="C#" Inherits="DiscImageChef.Server.ViewReport" %>
<%--
// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ViewReport.aspx
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : DiscImageChef Server.
//
// --[ Description ] ----------------------------------------------------------
//
//     Renders a device report.
//
// --[ License ] --------------------------------------------------------------
//
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/
--%>
<!DOCTYPE html>
<html>
<meta charset="UTF-8">
<head runat="server">
    <link href="dos.css" rel="stylesheet" type="text/css"/>
    <title>DiscImageChef Device Report</title>
    <!-- Global site tag (gtag.js) - Google Analytics -->
    <script async src="https://www.googletagmanager.com/gtag/js?id=UA-111466173-1"></script>
    <script>
        window.dataLayer = window.dataLayer || [];

        function gtag() { dataLayer.push(arguments); }

        gtag('js', new Date());

        gtag('config', 'UA-111466173-1');
    </script>
</head>
<body id="content" runat="server">
DiscImageChef Report for <asp:Label id="lblManufacturer" runat="server"/> <asp:Label id="lblModel" runat="server"/> <asp:Label id="lblRevision" runat="server"/>
<div id="divUsb" runat="server">
    <br/>
    <b>USB characteristics:</b><br/>
    <i>Manufacturer:</i> <asp:Label id="lblUsbManufacturer" runat="server"/><br/>
    <i>Product:</i> <asp:Label id="lblUsbProduct" runat="server"/><br/>
    <i>Vendor ID:</i> <asp:Label id="lblUsbVendor" runat="server"/> <asp:Label id="lblUsbVendorDescription" runat="server"/><br/>
    <i>Product ID:</i> <asp:Label id="lblUsbProductId" runat="server"/> <asp:Label id="lblUsbProductDescription" runat="server"/>
</div>
<div id="divFirewire" runat="server">
    <br/>
    <b>FireWire characteristics:</b><br/>
    <i>Manufacturer:</i> <asp:Label id="lblFirewireManufacturer" runat="server"/><br/>
    <i>Product:</i> <asp:Label id="lblFirewireProduct" runat="server"/><br/>
    <i>Vendor ID:</i> <asp:Label id="lblFirewireVendor" runat="server"/><br/>
    <i>Product ID:</i> <asp:Label id="lblFirewireProductId" runat="server"/>
</div>
<div id="divPcmcia" runat="server">
    <br/>
    <b>PCMCIA characteristics:</b><br/>
    <i>Manufacturer:</i> <asp:Label id="lblPcmciaManufacturer" runat="server"/><br/>
    <i>Product:</i> <asp:Label id="lblPcmciaProduct" runat="server"/><br/>
    <i>Manufacturer code:</i> <asp:Label id="lblPcmciaManufacturerCode" runat="server"/><br/>
    <i>Card code:</i> <asp:Label id="lblPcmciaCardCode" runat="server"/><br/>
    <i>Compliance:</i> <asp:Label id="lblPcmciaCompliance" runat="server"/>
    <asp:Repeater ID="repPcmciaTuples" runat="server">
        <ItemTemplate>
            <i>
                <asp:Label runat="server" Text='<%# Eval("key") %>'/>
            </i>: <asp:Label runat="server" Text='<%# Eval("value") %>'/><br/>
        </ItemTemplate>
    </asp:Repeater>

</div>
<div id="divAta" runat="server">
    <br/>
    <b>ATA<asp:Label id="lblAtapi" runat="server"/> characteristics:</b><br/>
    <asp:Label id="lblAtaDeviceType" runat="server"/><br/>
    <asp:Repeater ID="repAtaTwo" runat="server">
        <ItemTemplate>
            <i>
                <asp:Label runat="server" Text='<%# Eval("key") %>'/>
            </i>: <asp:Label runat="server" Text='<%# Eval("value") %>'/><br/>
        </ItemTemplate>
    </asp:Repeater>
    <br/>
    <asp:Repeater ID="repAtaOne" runat="server">
        <ItemTemplate>
            <%# Container.DataItem?.ToString() ?? string.Empty %><br/>
        </ItemTemplate>
    </asp:Repeater>
</div>
<div id="divScsi" runat="server">
    <br/>
    <b>SCSI characteristics:</b><br/>
    <i>Vendor:</i> <asp:Label id="lblScsiVendor" runat="server"/><br/>
    <i>Product:</i> <asp:Label id="lblScsiProduct" runat="server"/><br/>
    <i>Revision:</i> <asp:Label id="lblScsiRevision" runat="server"/><br/>
    <asp:Repeater ID="repScsi" runat="server">
        <ItemTemplate>
            <%# Container.DataItem?.ToString() ?? string.Empty %><br/>
        </ItemTemplate>
    </asp:Repeater>
    <div id="divScsiModeSense" runat="server">
        <br/><i>SCSI mode sense pages:</i>
        <table border="1">
            <tr>
                <th>Mode</th>
                <th>Contents</th>
            </tr>
            <asp:Repeater ID="repModeSense" runat="server">
                <ItemTemplate>
                    <tr>
                        <td>
                            <asp:Label runat="server" Text='<%# Eval("key") %>'/>
                        </td>
                        <td>
                            <asp:Label runat="server" Text='<%# Eval("value") %>'/>
                        </td>
                    </tr>
                </ItemTemplate>
            </asp:Repeater>
        </table>
    </div>
    <div id="divScsiEvpd" runat="server">
        <br/><i>SCSI extended vital product data pages:</i>
        <table border="1">
            <tr>
                <th>EVPD</th>
                <th>Contents</th>
            </tr>
            <asp:Repeater ID="repEvpd" runat="server">
                <ItemTemplate>
                    <tr>
                        <td>
                            <asp:Label runat="server" Text='<%# Eval("key") %>'/>
                        </td>
                        <td>
                            <asp:Label runat="server" Text='<%# Eval("value") %>'/>
                        </td>
                    </tr>
                </ItemTemplate>
            </asp:Repeater>
        </table>
    </div>
    <div id="divScsiMmcMode" runat="server">
        <br/><b>SCSI CD-ROM capabilities:</b><br/>
        <asp:Repeater ID="repScsiMmcMode" runat="server">
            <ItemTemplate>
                <%# Container.DataItem?.ToString() ?? string.Empty %><br/>
            </ItemTemplate>
        </asp:Repeater>
    </div>
    <div id="divScsiMmcFeatures" runat="server">
        <br/><b>SCSI MMC features:</b><br/>
        <asp:Repeater ID="repScsiMmcFeatures" runat="server">
            <ItemTemplate>
                <%# Container.DataItem?.ToString() ?? string.Empty %><br/>
            </ItemTemplate>
        </asp:Repeater>
    </div>
    <div id="divScsiSsc" runat="server">
        <br/><b>SCSI Streaming device capabilities:</b><br/>
        Block size granularity: <asp:Label id="lblScsiSscGranularity" runat="server"/><br/>
        Maximum block length: <asp:Label id="lblScsiSscMaxBlock" runat="server"/> bytes<br/>
        Minimum block length: <asp:Label id="lblScsiSscMinBlock" runat="server"/> bytes<br/>
        <asp:Repeater ID="repScsiSscDensities" runat="server">
            <ItemTemplate>
                <br/><b>Information for supported density with primary code <asp:Label runat="server" Text='<%# string.Format("{0:X2h}", Eval("PrimaryCode")) %>'/> and secondary code <asp:Label runat="server" Text='<%# string.Format("{0:X2h}", Eval("SecondaryCode")) %>'/></b><br/>
                Drive can write this density: <asp:Label runat="server" Text='<%# string.Format("{0}", Eval("Writable")) %>'/><br/>
                Duplicate density: <asp:Label runat="server" Text='<%# string.Format("{0}", Eval("Duplicate")) %>'/><br/>
                Default density: <asp:Label runat="server" Text='<%# string.Format("{0}", Eval("DefaultDensity")) %>'/><br/>
                Density has <asp:Label runat="server" Text='<%# Eval("BitsPerMm") %>'/> bits per mm, with <asp:Label runat="server" Text='<%# Eval("Tracks") %>'/> tracks in a <asp:Label runat="server" Text='<%# Eval("Width") %>'/> mm width tape
                Name: <asp:Label runat="server" Text='<%# Eval("Name") %>'/><br/>
                Organization: <asp:Label runat="server" Text='<%# Eval("Organization") %>'/><br/>
                Description: <asp:Label runat="server" Text='<%# Eval("Description") %>'/><br/>
                Maximum capacity: <asp:Label runat="server" Text='<%# Eval("Capacity") %>'/> megabytes<br/>
            </ItemTemplate>
        </asp:Repeater>
        <asp:Repeater ID="repScsiSscMedias" runat="server">
            <ItemTemplate>
                <br/><b>Information for supported media with type code <asp:Label runat="server" Text='<%# string.Format("{0:X2h}", Eval("MediumType")) %>'/></b><br/>
                Drive can write this density: <asp:Label runat="server" Text='<%# string.Format("{0}", Eval("Writable")) %>'/><br/>
                Media is <asp:Label runat="server" Text='<%# Eval("Length") %>'/> meters long in a <asp:Label runat="server" Text='<%# Eval("Width") %>'/> mm width tape
                Name: <asp:Label runat="server" Text='<%# Eval("Name") %>'/><br/>
                Organization: <asp:Label runat="server" Text='<%# Eval("Organization") %>'/><br/>
                Description: <asp:Label runat="server" Text='<%# Eval("Description") %>'/><br/>
            </ItemTemplate>
        </asp:Repeater>
    </div>
</div>
<div id="divTestedMedia" runat="server">
    <br/><b>Tested media:</b><br/>
    <asp:Repeater ID="repTestedMedia" runat="server">
        <ItemTemplate>
            <%# Container.DataItem?.ToString() ?? string.Empty %><br/>
        </ItemTemplate>
    </asp:Repeater>
</div>
<div id="divMMC" runat="server">
    <br/>
    <b>MultiMediaCard device:</b><br/>
    <asp:Repeater ID="repMMC" runat="server">
        <ItemTemplate>
            <%# Container.DataItem?.ToString() ?? string.Empty %><br/>
        </ItemTemplate>
    </asp:Repeater>
</div>
<div id="divSD" runat="server">
    <br/>
    <b>SecureDigital device:</b><br/>
    <asp:Repeater ID="repSD" runat="server">
        <ItemTemplate>
            <%# Container.DataItem?.ToString() ?? string.Empty %><br/>
        </ItemTemplate>
    </asp:Repeater>
</div>
</body>
</html>