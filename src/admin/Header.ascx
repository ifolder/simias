<%@ Control Language="c#" Codebehind="Header.ascx.cs" AutoEventWireup="false" Inherits="Novell.iFolderWeb.Admin.Header"%>

<!-- banner -->
<div class="bannerRegion">
	<div class="bannerContent">
	
		<table border="0" cellspacing="0" cellpadding="0" width="100%" height="56" style="margin:0px; padding:0px; background-image:url(images/header_pattern.gif); background-repeat: repeat-x;">
			<tr>
				<td width="221"><img src="images/product_title.gif" alt="Novell iFolder" border="0" width="221" height="26"></td>
				<td align="right" width="100%" class="tabCell" style="background-image:url(images/header_background.gif); background-repeat: no-repeat;">
					<asp:LinkButton ID="LogoutButton" CssClass="bannerLink" runat="server" />
				</td>
				<td rowspan="2"><a href="http://www.novell.com" target="_blank"><img alt="Novell, Inc" src="images/logo_N.gif" border="0" width="48" height="56"></a></td>
			</tr>
			<tr>
				<td valign="top" width="221" class="bannerName" style="background-image:url(images/username_background.gif); background-repeat: repeat-x;">
					<asp:Literal ID="UserName" runat="server" />
				</td>
				<td style="background-image:url(images/tab_normal_pattern.gif); background-repeat: repeat-x;">
					<table border="0" cellspacing="0" cellpadding="0" width="100%">
						<tr>
							<td width="43"><img src="images/username_curve.gif" alt="" border="0" width="43" height="30"></td>
							<td nowrap align="right" width="100%" class="bannerContext"><asp:Literal ID="SystemName" runat="server" /></td>
							<td align="right" width="100%" height="30">
								<table border="0" cellspacing="0" cellpadding="0" height="30">
									<tr>
										<td nowrap align="center" class="tabOptionCell" style="background-image:url(images/tab_option_pattern.gif); background-repeat: repeat-x;">
											<asp:HyperLink ID="HelpButton" CssClass="bannerLink" Target="ifolderHelp" NavigateUrl="help/index.html" runat="server" /> 
										</td>
									</tr>
								</table>
							</td>
						</tr>
					</table>
				</td>
			</tr>
		</table>
	
	</div>
</div>
