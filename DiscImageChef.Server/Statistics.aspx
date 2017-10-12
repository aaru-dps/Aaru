<%@ Page Language="C#" Inherits="DiscImageChef.Server.Statistics" %>
<!DOCTYPE html>
<html>
<meta charset="UTF-8">
<head runat="server">
    <link type='text/css' rel='stylesheet' href='dos.css' />
	<title>DiscImageChef Statistics</title>
</head>
<body id="body" runat="server">
    <h1 align="center">Welcome to <i><a href="http://github.com/claunia/discimagechef" target="_blank">DiscImageChef</a></i> Server version <asp:Label id="lblVersion" runat="server"/></h1>
    <br/>
	<div id="content" runat="server">
            <div id="divOperatingSystems" runat="server">
                <table>
                    <asp:Repeater id="repOperatingSystems" runat="server">
                        <ItemTemplate>
                            <tr>
                                <td>DiscImageChef has run on <i><asp:Label runat="server" Text='<%# Eval("name") %>' /></i> <asp:Label runat="server" Text='<%# Eval("Value") %>' /> times.</td>
                            </tr>
                        </ItemTemplate>
                    </asp:Repeater>
                </table>
                <br/>
            </div>
            <div id="divVersions" runat="server">
                <table>
                    <asp:Repeater id="repVersions" runat="server">
                        <ItemTemplate>
                            <tr>
                                <td>DiscImageChef version <i><asp:Label runat="server" Text='<%# Eval("name") %>' /></i> has been used <asp:Label runat="server" Text='<%# Eval("Value") %>' /> times.</td>
                            </tr>
                        </ItemTemplate>
                    </asp:Repeater>
                </table>
                <br/>
            </div>
			<div id="divCommands" runat="server">
				<h4>Commands run:</h4>
                <p>
                    <i>analyze</i> command has been run <asp:Label id="lblAnalyze" runat="server"/> times<br/>
                    <i>benchmark</i> command has been run <asp:Label id="lblBenchmark" runat="server"/> times<br/>
                    <i>checksum</i> command has been run <asp:Label id="lblChecksum" runat="server"/> times<br/>
                    <i>compare</i> command has been run <asp:Label id="lblCompare" runat="server"/> times<br/>
                    <i>create-sidecar</i> command has been run <asp:Label id="lblCreateSidecar" runat="server"/> times<br/>
                    <i>decode</i> command has been run <asp:Label id="lblDecode" runat="server"/> times<br/>
                    <i>device-info</i> command has been run <asp:Label id="lblDeviceInfo" runat="server"/> times<br/>
                    <i>device-report</i> command has been run <asp:Label id="lblDeviceReport" runat="server"/> times<br/>
                    <i>dump-media</i> command has been run <asp:Label id="lblDumpMedia" runat="server"/> times<br/>
                    <i>entropy</i> command has been run <asp:Label id="lblEntropy" runat="server"/> times<br/>
                    <i>extract-files</i> command has been run <asp:Label id="lblExtractFiles" runat="server"/> times<br/>
                    <i>formats</i> command has been run <asp:Label id="lblFormats" runat="server"/> times<br/>
                    <i>list-devices</i> command has been run <asp:Label id="lblListDevices" runat="server"/> times<br/>
                    <i>list-encodings</i> command has been run <asp:Label id="lblListEncodings" runat="server"/> times<br/>
                    <i>ls</i> command has been run <asp:Label id="lblLs" runat="server"/> times<br/>
                    <i>media-info</i> command has been run <asp:Label id="lblMediaInfo" runat="server"/> times<br/>
                    <i>media-scan</i> command has been run <asp:Label id="lblMediaScan" runat="server"/> times<br/>
                    <i>printhex</i> command has been run <asp:Label id="lblPrintHex" runat="server"/> times<br/>
                    <i>verify</i> command has been run <asp:Label id="lblVerify" runat="server"/> times
				</p>
            </div>
            <div id="divFilters" runat="server">
                <h3>Filters found:</h3>
                <table align="center" border="1">
                    <tr>
                        <th>Filter</th>
                        <th>Times</th>
                    </tr>
                    <asp:Repeater ID="repFilters" runat="server">
                        <ItemTemplate>
                            <tr>
                                <td><asp:Label runat="server" Text='<%# Eval("name") %>' /></td>
                                <td><asp:Label runat="server" Text='<%# string.Format("{0}", Eval("Value")) %>' /></td>
                            </tr>
                        </ItemTemplate>
                    </asp:Repeater>
                </table>                
            </div>
            <div id="divMediaImages" runat="server">
                <h3>Media image formats found:</h3>
                <table align="center" border="1">
                    <tr>
                        <th>Media image format</th>
                        <th>Times</th>
                    </tr>
                    <asp:Repeater ID="repMediaImages" runat="server">
                        <ItemTemplate>
                            <tr>
                                <td><asp:Label runat="server" Text='<%# Eval("name") %>' /></td>
                                <td><asp:Label runat="server" Text='<%# string.Format("{0}", Eval("value")) %>' /></td>
                            </tr>
                        </ItemTemplate>
                    </asp:Repeater>
                </table>                
            </div>
            <div id="divPartitions" runat="server">
                <h3>Partition schemes found:</h3>
                <table align="center" border="1">
                    <tr>
                        <th>Partition scheme</th>
                        <th>Times</th>
                    </tr>
                    <asp:Repeater ID="repPartitions" runat="server">
                        <ItemTemplate>
                            <tr>
                                <td><asp:Label runat="server" Text='<%# Eval("name") %>' /></td>
                                <td><asp:Label runat="server" Text='<%# string.Format("{0}", Eval("value")) %>' /></td>
                            </tr>
                        </ItemTemplate>
                    </asp:Repeater>
                </table>                
            </div>
            <div id="divFilesystems" runat="server">
                <h3>Filesystems found:</h3>
                <table align="center" border="1">
					<tr>
						<th>Filesystem name</th>
						<th>Times</th>
					</tr>
                    <asp:Repeater ID="repFilesystems" runat="server">
                        <ItemTemplate>
                            <tr>
							    <td><asp:Label runat="server" Text='<%# Eval("name") %>' /></td>
                                <td><asp:Label runat="server" Text='<%# string.Format("{0}", Eval("value")) %>' /></td>
							</tr>
                        </ItemTemplate>
                    </asp:Repeater>
                </table>                
            </div>
            <div id="divVirtualMedia" runat="server">
                <h3>Media types found in images:</h3>
                <table align="center" border="1">
                    <tr>
                        <th>Physical type</th>
                        <th>Logical type</th>
                        <th>Times</th>
                    </tr>
                    <asp:Repeater ID="repVirtualMedia" runat="server">
                        <ItemTemplate>
                            <tr>
                                <td><asp:Label runat="server" Text='<%# Eval("Type") %>' /></td>
                                <td><asp:Label runat="server" Text='<%# Eval("SubType") %>' /></td>
                                <td><asp:Label runat="server" Text='<%# string.Format("{0}", Eval("Count")) %>' /></td>
                            </tr>
                        </ItemTemplate>
                    </asp:Repeater>
                </table>                
            </div>
            <div id="divRealMedia" runat="server">
                <h3>Media types found in devices:</h3>
                <table align="center" border="1">
                    <tr>
                        <th>Physical type</th>
                        <th>Logical type</th>
                        <th>Times</th>
                    </tr>
                    <asp:Repeater ID="repRealMedia" runat="server">
                        <ItemTemplate>
                            <tr>
                                <td><asp:Label runat="server" Text='<%# Eval("Type") %>' /></td>
                                <td><asp:Label runat="server" Text='<%# Eval("SubType") %>' /></td>
                                <td><asp:Label runat="server" Text='<%# string.Format("{0}", Eval("Count")) %>' /></td>
                            </tr>
                        </ItemTemplate>
                    </asp:Repeater>
                </table>                
            </div>
            <div id="divDevices" runat="server">
                <h3>Found devices:</h3>
                <table align="center" border="1">
                    <tr>
                        <th>Manufacturer</th>
                        <th>Model</th>
                        <th>Revision</th>
						<th>Bus</th>
						<th>Report</th>
                    </tr>
                    <asp:Repeater ID="repDevices" runat="server">
                        <ItemTemplate>
                            <tr>
                                <td><asp:Label runat="server" Text='<%# Eval("Manufacturer") %>' /></td>
                                <td><asp:Label runat="server" Text='<%# Eval("Model") %>' /></td>
                                <td><asp:Label runat="server" Text='<%# Eval("Revision") %>' /></td>
                                <td><asp:Label runat="server" Text='<%# Eval("Bus") %>' /></td>
                                <td><asp:Label runat="server" Text='<%# Eval("ReportLink") %>' /></td>
							</tr>
                        </ItemTemplate>
                    </asp:Repeater>
                </table>                
            </div>
		</div>
		<hr/>
		<footer>
			© 2011-2017 <a href="http://www.claunia.com" target="_blank">Claunia.com</a><br/>
			Fonts are © 2015-2016 <a href="http://int10h.org" target="_blank">VileR</a><br/>
			CSS © 2017 <a href="http://www.freedos.org" target="_blank">The FreeDOS Project</a>
		</footer>
</body>
</html>
