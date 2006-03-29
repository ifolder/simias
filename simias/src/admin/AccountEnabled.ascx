<%@ Control Language="c#" AutoEventWireup="false" Codebehind="AccountEnabled.ascx.cs" Inherits="Novell.iFolderWeb.Admin.AccountEnabled" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>
<div id="AccountNav" runat="server">

	<div class="policytitle"><%= GetString( "ACCOUNT" ) %></div>
	
	<div class="policydetails">
	
		<asp:CheckBox ID="AccountCheckBox" Runat="server" AutoPostBack="True" />
		
		<div class="policyunits"><%= GetString( "USERLOGINDISABLED" ) %></div>
		
	</div>
	
</div>
