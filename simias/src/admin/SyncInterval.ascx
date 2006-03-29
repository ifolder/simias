<%@ Control Language="c#" AutoEventWireup="false" Codebehind="SyncInterval.ascx.cs" Inherits="Novell.iFolderWeb.Admin.SyncInterval" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>

<div id="syncnav">

	<div class="policytitle"><%= GetString( "SYNCHRONIZATION" ) %></div>
	
	<div class="policydetails">
	
		<asp:CheckBox ID="SyncIntervalCheckBox" Runat="server" AutoPostBack="True" />
		
		<div class="policylimit"><%= GetString( "INTERVALTAG" ) %></div>
			
		<asp:TextBox ID="SyncLimit" Runat="server" CssClass="policytextbox" AutoPostBack="true" />
			
		<div class="policyunits"><%= GetString( "MINUTES" ) %></div>
		
	</div>
	
</div>

