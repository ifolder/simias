<%@ Control Language="c#" Codebehind="Quota.ascx.cs" AutoEventWireup="false" Inherits="Novell.iFolderApp.Web.QuotaControl"%>

<!-- quota -->

<div class="quota">
	
	<div class="box">
	
		<div class="label"><%= GetString("USED") %>:</div>
		<div><asp:Literal ID="SpaceUsed" runat="server" /></div>
	
		<div class="label"><%= GetString("AVAILABLE") %>:</div>
		<div><asp:Literal ID="SpaceAvailable" runat="server" /></div>
	
	</div>
		
</div>