<%@ Page language="c#" Codebehind="Login.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderApp.Web.Login" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01//EN" "http://www.w3.org/TR/html4/strict.dtd">
<html>

<head>
	<meta http-equiv="Content-Type" content="text/html; charset=iso-8859-1">
	
	<title><%= GetString("TITLE") %></title>
	
	<link rel="SHORTCUT ICON" href="images/ifolder.ico">

	<style type="text/css">
		@import url(css/ifolder.css);
	</style>

	<script type="text/javascript">
	<!--
		// set the focus on the password text box, unless no username exists
		function SetFocus()
		{
			var tb = document.getElementById("UserName");
			
			if (tb.value.length > 0)
			{
				document.getElementById("Password").focus();
			}
			else
			{
				tb.focus();
			}
		}
		
		// on load
		window.onload = SetFocus;
	-->
	</script>
</head>

<body id="login">

<form runat="server">
	
	<noscript>
		<input type="hidden" id="noscript" value="true" runat="server">
	</noscript>
	
	<div id="wrapper">
	
		<table class="dialog">
			<tr><td class="title" colspan="2"><div class="title"></div></td></tr>
			<tr><td class="message" colspan="2"><asp:Literal ID="Message" runat="server" /></td></tr>
			<tr>
				<td class="field"><%= GetString("LOGIN.USERNAME") %></td>
				<td class="field"><%= GetString("LOGIN.PASSWORD") %></td>
			</tr>
			<tr>
				<td class="field"><asp:TextBox ID="UserName" CssClass="field" runat="server" /></td>
				<td class="field"><asp:TextBox ID="Password" TextMode="Password" CssClass="field" runat="server" /></td>
			</tr>
			<tr><td class="button" colspan="2"><asp:Button ID="LoginButton" runat="server" /></td></tr>
			<tr><td class="copyright" colspan="2"><%= GetString("LOGIN.COPYRIGHT") %></td></tr>
		</table>
	
	</div>

</form>

</body>

</html>
