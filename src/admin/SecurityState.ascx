<%@ Control Language="c#" AutoEventWireup="false" Codebehind="SecurityState.ascx.cs" Inherits="Novell.iFolderWeb.Admin.SecurityState" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>

<div id="EncryptNav" runat="server">

	<div class="policydetails">

		<asp:Label ID="EncryptionTitle" Runat="server" CssClass="policytitle" />
		<table class="policytable">
			<tr>
				<td class="policycheckbox" >
					<asp:CheckBox ID="encryption" Runat="server" AutoPostBack="True" CssClass="policycheckboxChinese"/>
				</td>
				<td class="policycheckbox" >
					<asp:CheckBox ID="enforceEncryption" Runat="server" AutoPostBack="True" CssClass="policycheckboxChinese"/>
				</td>
			</tr>
		</table>
		<br>
		<asp:Label ID="SSLTitle" Runat="server" CssClass="policytitle" />
		<table class="policytable">
			<tr>
				<td class="policycheckbox" >
					<asp:CheckBox ID="ssl" Runat="server" AutoPostBack="True" />
				</td>
				<td class="policycheckbox" >
					<asp:CheckBox ID="enforceSSL" Runat="server" AutoPostBack="True" />
				</td>
			</tr>

		</table>

	</div>

	
</div>
