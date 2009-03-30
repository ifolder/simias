<%@ Control Language="c#" AutoEventWireup="false" Codebehind="Sharing.ascx.cs" Inherits="Novell.iFolderWeb.Admin.Sharing" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>
<div id="SharingNav" runat="server" >

	<div class="policydetails">

		<asp:Label ID="SharingTitle" Runat="server" CssClass="policytitle" />
		<table class="policytable">
			<tr>
				<td class="policycheckbox" >
					<asp:CheckBox ID="SharingOn" Runat="server" AutoPostBack="True"  />
				</td>
				<td class="sharingpolicycheckbox" >
					<asp:CheckBox ID="enforcedSharing" Runat="server" AutoPostBack="True" CssClass="sharingpolicycheckbox" />
				</td>
				<td class="policycheckbox" >
					<asp:CheckBox ID="disablePastSharing" Runat="server" AutoPostBack="True" />
				</td>
			</tr>
		</table>

	</div>

</div>
