<%@ Control Language="c#" AutoEventWireup="false" Codebehind="DiskSpaceQuota.ascx.cs" Inherits="Novell.iFolderWeb.Admin.DiskSpaceQuota" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>

<div id="quotanav">

	<asp:Label ID="Title" Runat="server" CssClass="policytitle" />
	
	<div class="policydetails">
	
		
		<table class="policytable">
		
			<tr>
				<td class="policycheckbox">
					<asp:CheckBox ID="Enabled" Runat="server" AutoPostBack="True" />
				</td>
				
				<td class="policytabletag">
					<asp:Label ID="LimitTag" Runat="server" />
				</td>

				<td class="policytablevalue">
					<asp:TextBox ID="LimitValue" Runat="server" CssClass="policytextbox" AutoPostBack="true" />
				</td>
				
				<td>
					<%= GetString( "MB" ) %>
				</td>		
			</tr>
		
			<tr>
				<td>
				</td>
				
				<td class="policytabletag">
					<asp:Label ID="UsedTag" Runat="server" />
				</td>
				
				<td class="policytablevalue">
					<asp:Label ID="UsedValue" Runat="server" />
				</td>
				
				<td>
					<asp:Label ID="UsedUnits" Runat="server" />
				</td>
			</tr>
			
			<tr>
				<td>
				</td>

				<td class="policytabletag">
					<asp:Label ID="AvailableTag" Runat="server" />
				</td>
				
				<td class="policytablevalue">
					<asp:Label ID="AvailableValue" Runat="server" />
				</td>
				
				<td>
					<asp:Label ID="AvailableUnits" Runat="server" />
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

