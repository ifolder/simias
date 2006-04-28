<%@ Control Language="c#" AutoEventWireup="false" Codebehind="Pagging.ascx.cs" Inherits="Novell.iFolderApp.Web.PaggingControl" %>

<div class="pagging" runat="server">
	
	<asp:ImageButton ID="FirstImage" ImageUrl="images/go-first.png" runat="server" />
	<asp:Image ID="FirstImageDisabled" ImageUrl="images/go-first-disabled.png" runat="server" />
	
	<asp:ImageButton ID="PreviousImage" ImageUrl="images/go-previous.png" runat="server" />
	<asp:Image ID="PreviousImageDisabled" ImageUrl="images/go-previous-disabled.png" runat="server" />
	
	<div class="index">
		<asp:Literal ID="StartIndex" runat="server" />&ndash;<asp:Literal ID="EndIndex" runat="server" />
		<%= GetString("OF") %>
		<asp:Literal ID="TotalLabel" runat="server" />
		<asp:Literal ID="ItemLabelPlural" runat="server" /><asp:Literal ID="ItemLabelSingular" runat="server" />
	</div>
	
	<asp:ImageButton ID="NextImage" ImageUrl="images/go-next.png" runat="server" />
	<asp:Image ID="NextImageDisabled" ImageUrl="images/go-next-disabled.png" runat="server" />
	
	<asp:ImageButton ID="LastImage" ImageUrl="images/go-last.png" runat="server" />
	<asp:Image ID="LastImageDisabled" ImageUrl="images/go-last-disabled.png" runat="server" />
	
</div>
