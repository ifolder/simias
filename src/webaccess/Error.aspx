<%@ Page language="C#" Codebehind="Error.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderApp.Web.Error" %>
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

		<div id="header">
			<div class="panel">
			</div>
		</div>		

		<div id="nav">
		</div>
	
		<div id="content">
		
			<div class="main">
				
				<asp:Label ID="ErrorMessage" CssClass="errorMessage" runat="server" />
				
				<asp:Button ID="LoginButton" runat="server" />
				
				<br><br>
				
				<div>
					<asp:TextBox ID="ErrorDetails" TextMode="MultiLine" Wrap="False" Rows="16" CssClass="errorDetails" runat="server" />
				</div>
				
			</div>
	
		</div>
	
	</form>

</div>

</body>

</html>