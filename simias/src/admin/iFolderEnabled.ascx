<%@ Control Language="c#" AutoEventWireup="false" Codebehind="iFolderEnabled.ascx.cs" Inherits="Novell.iFolderWeb.Admin.iFolderEnabled" TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>
<div id="iFolderEnabledNav" runat="server">

	<div class="policytitle"><%= GetString( "IFOLDER" ) %></div>
	
	<div class="policydetails">
	
		<asp:CheckBox ID="DisabledCheckBox" Runat="server" AutoPostBack="True" />
	
		<div class="policyunits"><%= GetString( "IFOLDERDISABLED" ) %></div>
		
	</div>
	
</div>
