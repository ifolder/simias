<%@ Control Language="c#" AutoEventWireup="false" Codebehind="Pagging.ascx.cs" Inherits="Novell.iFolderApp.Web.Pagging" %>

<div class="pagging" runat="server">
	
	<asp:ImageButton ID="FirstImage" ImageUrl="images/16/go-first.png" runat="server" />
	<asp:Image ID="FirstImageDisabled" ImageUrl="images/16/go-first-disabled.png" runat="server" />
	
	<asp:ImageButton ID="PreviousImage" ImageUrl="images/16/go-previous.png" runat="server" />
	<asp:Image ID="PreviousImageDisabled" ImageUrl="images/16/go-previous-disabled.png" runat="server" />
	
	<span class="page">
		<asp:Literal ID="StartIndex" runat="server" />&ndash;<asp:Literal ID="EndIndex" runat="server" />
		<%= GetString("OF") %>
		<asp:Literal ID="TotalLabel" runat="server" />
		<asp:Literal ID="ItemLabelPlural" runat="server" /><asp:Literal ID="ItemLabelSingular" runat="server" />
	</span>
	
	<asp:ImageButton ID="NextImage" ImageUrl="images/16/go-next.png" runat="server" />
	<asp:Image ID="NextImageDisabled" ImageUrl="images/16/go-next-disabled.png" runat="server" />
	
	<asp:ImageButton ID="LastImage" ImageUrl="images/16/go-last.png" runat="server" />
	<asp:Image ID="LastImageDisabled" ImageUrl="images/16/go-last-disabled.png" runat="server" />
	
</div>
