<%@ Page language="c#" Codebehind="Settings.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderApp.Web.SettingsPage" %>
<%@ Register TagPrefix="iFolder" TagName="HeaderControl" Src="Header.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="MessageControl" Src="Message.ascx" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01//EN" "http://www.w3.org/TR/html4/strict.dtd">
<html>

<head>
	<meta http-equiv="Content-Type" content="text/html; charset=iso-8859-1">
	
	<title><%= GetString("TITLE") %></title>
	
	<link rel="SHORTCUT ICON" href="images/ifolder.ico">

	<style type="text/css">
		@import url(css/ifolder.css);
	</style>

</head>

<body>

<div id="container">
	
	<form runat="server">

		<iFolder:HeaderControl runat="server" />
		
		<div id="nav">
		</div>
	
		<div id="content">
		
			<iFolder:MessageControl id="Message" runat="server" />
	
			<div class="section">
				<%= GetString("SETTINGS") %>
			</div>
			
			<div class="main">
				
				<div>
					<asp:Label ID="PageSizeLabel" runat="server" />:
					<asp:DropDownList ID="PageSizeList" runat="server" />
				</div>
				
				<br>
				
				<div class="buttons">
					<asp:Button ID="SaveButton" runat="server" />
					<asp:Button ID="CancelButton" runat="server" />
				</div>

			</div>
	
		</div>
	
	</form>

</div>

</body>

</html>
