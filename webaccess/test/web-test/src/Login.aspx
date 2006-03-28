<%@ Page language="c#" Codebehind="Login.aspx.cs" AutoEventWireup="false" Inherits="iFolderWebClient.LoginPage" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01//EN" "http://www.w3.org/TR/html4/strict.dtd">

<html>

<head>
	<title>iFolder 3</title>

	<link rel="SHORTCUT ICON" href="images/ifolder.ico">

	<style type="text/css">
		@import url(css/login.css);
	</style>

	<script type="text/javascript" src="js/lib/prototype.js"></script>
	
	<script type="text/javascript">
	<!--
		// set the focus on the password text box, unless no username exists
		function setFocus()
		{
			var tb = $("Username");
			
			if (tb.value.length > 0)
			{
				$("Password").focus();
			}
			else
			{
				tb.focus();
			}
		}
		
		// event
		Event.observe(window, 'load', this.setFocus);
	-->
	</script>
</head>

<body>

<form id="Form" runat="server">
	
	<noscript>
		<input type="hidden" id="noscript" value="true" runat="server">
	</noscript>
	
	<div id="container">
	
		<table id="dialog">
			<tr><td id="titleCell" colspan="2"><div id="title"></div></td></tr>
			<tr><td id="messageCell" colspan="2"><asp:Literal ID="Message" runat="server"></asp:Literal></td></tr>
			<tr>
				<td class="field">Username:</td>
				<td class="field">Password:</td>
			</tr>
			<tr>
				<td class="field"><asp:TextBox ID="Username" runat="server"></asp:TextBox></td>
				<td class="field"><asp:TextBox ID="Password" TextMode="Password" runat="server"></asp:TextBox></td>
			</tr>
			<tr><td id="buttonCell" colspan="2"><asp:Button ID="Login" Text="Login" runat="server"></asp:Button></td></tr>
			<tr><td id="copyrightCell" colspan="2">&copy;2006 Novell, Inc. All rights reserved.</td></tr>
		</table>
	
	</div>
		
</form>
</body>
</html>
