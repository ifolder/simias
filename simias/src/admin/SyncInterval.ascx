<%@ Control Language="c#" AutoEventWireup="false" Codebehind="SyncInterval.ascx.cs" Inherits="Novell.iFolderWeb.Admin.SyncInterval" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>

<div id="syncnav">

	<asp:Label ID="Title" Runat="server" CssClass="policytitle" />
	
	<div class="policydetails">
	
		<table class="policytable">
		
			<tr>
				<td class="policycheckbox">
					<asp:CheckBox ID="Enabled" Runat="server" AutoPostBack="True" CssClass="policycheckbox" />
				</td>
				
				<td class="policytabletag">
					<asp:Label ID="LimitTag" Runat="server" />
				</td>
				
				<td class="policytablevalue">
					<asp:TextBox ID="LimitValue" Runat="server" CssClass="policytextbox" AutoPostBack="true" />
				</td>
				
				<td>
					<%= GetString( "MINUTES" ) %>
				</td>
			</tr>
			
			<tr>
				<td>
				</td>
				
				<td class="policytabletag">
					<asp:Label ID="EffectiveTag" Runat="server" />
				</td>
				
				<td class="policytablevalue">
					<asp:Label ID="EffectiveValue" Runat="server" />
				</td>
				
				<td>
					<asp:Label ID="EffectiveUnits" Runat="server" />
				</td>
			</tr>
		
		</table>
		
	</div>
	
</div>

