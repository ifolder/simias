<%@ Control Language="c#" Codebehind="Quota.ascx.cs" AutoEventWireup="false" Inherits="Novell.iFolderApp.Web.QuotaControl"%>

<!-- quota -->

<div id="quota" class="group">
	
	<div class="box">
	
		<div class="title"><asp:Literal ID="Title" runat="server" /></div>

		<div class="label"><%= GetString("USED") %>:</div>
		<div class="data"><asp:Literal ID="SpaceUsed" runat="server" /></div>
	
		<div class="label"><%= GetString("AVAILABLE") %>:</div>
		<div class="data"><asp:Literal ID="SpaceAvailable" runat="server" /></div>
	
	</div>
		
</div>