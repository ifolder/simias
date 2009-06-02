<%@ Page language="c#" Codebehind="StatusPage.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderWeb.Admin.StatusPage" %>
<%@ Register TagPrefix="iFolder" TagName="TopNavigation" Src="TopNavigation.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="Footer" Src="Footer.ascx" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" > 
<html>

<head>

	<meta http-equiv="Content-Type" content="text/html; charset=iso-8859-1">
	<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">

	<title><%= GetString( "TITLE" ) %></title>
		
	<style type="text/css">
		@import url(css/iFolderAdmin.css);
		@import url(css/SystemInfo.css);
	</style>

</head>

<body id="ifolders" runat="server">
	
<form runat="server" ID="Form1">

	<div class="container">
			
		<iFolder:TopNavigation ID="TopNav" Runat="server" />
				
				<br><br><br>
                                <asp:Label ID="SuccessLabel" CssClass="statusbutton" runat="server" /> <br> <br>
				<br>
		
                                <asp:Label ID="StatusLabel" runat="server" /> <br> <br>
				<br>
                                <div class="buttons">
                                        <asp:Button ID="RepeatButton" CssClass="repeatbutton" runat="server" />
                                        <asp:Button ID="OKButton" runat="server" />
				<br><br>
                                </div>
			

	</div>
	
				
</form>
	
</body>

</html>
