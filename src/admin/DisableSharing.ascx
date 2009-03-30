<%@ Control Language="c#" AutoEventWireup="false" Codebehind="DisableSharing.ascx.cs" Inherits="Novell.iFolderWeb.Admin.DisableSharing" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>
<div id="DisableSharingNav" runat="server">

	<div class="policydetails">

		<asp:Label ID="DisableSharingTitle" Runat="server" CssClass="policytitle" />
		<table class="policytable">
			<tr>
				<td class="policycheckbox" >
					<asp:CheckBox ID="disableSharingOn" Runat="server" AutoPostBack="True" />
				</td>
				<td class="policycheckbox" >
					<asp:CheckBox ID="enforcedDisableSharing" Runat="server" AutoPostBack="True" />
				</td>
			</tr>
			<tr>
				<td class="policycheckbox" >
					<asp:CheckBox ID="disablePastSharing" Runat="server" AutoPostBack="True" />
				</td>
			</tr>
		</table>

	</div>

</div>
