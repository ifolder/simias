<%@ Control Language="c#" AutoEventWireup="false" Codebehind="FileSizeFilter.ascx.cs" Inherits="Novell.iFolderWeb.Admin.FileSizeFilter" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>

<div id="filesizenav">

	<div class="policytitle"><%= GetString( "FILESIZEFILTER" ) %></div>
	
	<div class="policydetails">
	
		<asp:CheckBox ID="FileSizeEnabled" Runat="server" AutoPostBack="True" />
		
		<div class="policylimit"><%= GetString( "LIMITTAG" ) %></div>
		
		<asp:TextBox ID="FileSizeLimit" Runat="server" CssClass="policytextbox" AutoPostBack="true" />
			
		<div class="policyunits"><%= GetString( "MB" ) %></div>
		
	</div>
	
</div>

