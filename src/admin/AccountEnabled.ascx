<%@ Control Language="c#" AutoEventWireup="false" Codebehind="AccountEnabled.ascx.cs" Inherits="Novell.iFolderWeb.Admin.AccountEnabled" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>
<div id="AccountNav" runat="server">

	<asp:Label ID="Title" Runat="server" CssClass="policytitle" />
	
	<div class="policydetails">
	
		<table class="policytable">
		
			<tr>
				<td class="policycheckbox">
					<asp:CheckBox ID="Enabled" Runat="server" AutoPostBack="True" />
				</td>
				
				<td colspan="3">
					<asp:Label ID="DisabledTag" Runat="server" />
				</td>
			</tr>

		</table>
				
	</div>
	
</div>
