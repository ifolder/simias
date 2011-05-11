<%@ Page language="c#" Codebehind="Login.aspx.cs" AutoEventWireup="false" ValidateRequest="false" Inherits="Novell.iFolderApp.Web.Login" %>
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
	<%--
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
	--%>
	</script>
</head>

<body id="login">

<form runat="server">
	
	<noscript>
		<input type="hidden" id="noscript" value="true" runat="server">
	</noscript>
	
	<table class="dialog"> 
		<tr>
			<td class="title" colspan="2"><asp:HyperLink ID="HelpButton"  style="float:right; margin: -25px 5px 0px 0px; color:white; position: relative; text-decoration: none; font-size: 1em; font-weight: bold;" Target="ifolderHelp" NavigateUrl="help/login.html" runat="server" /></td>
		</tr>
		<tr><td class="message" colspan="2"><asp:Label ID="Message" runat="server" /></td></tr>
		<tr>
			<td class="field" style="text-align:right"><%= GetString("LOGIN.USERNAME") %></td>
			<td class="field"><asp:TextBox ID="UserName" CssClass="field" runat="server" /></td>
		</tr>
		<tr>
			
			<td class="field" style="text-align:right"><%= GetString("LOGIN.PASSWORD") %></td>
			<td class="field"><asp:TextBox ID="Password" TextMode="Password" CssClass="field" runat="server" /></td>
		</tr>
				<tr>
					<td class="field" style="text-align:right">
						<%= GetString("LOGIN.LANGUAGE") %>
					</td>
					
					<td class="field">
						<asp:DropDownList ID="LanguageList" runat="server" AutoPostBack="true"/>
					</td>
				</tr>
		<tr>
			<td>&nbsp;</td>
			<td class="button" style="text-align:left"><asp:Button ID="LoginButton" runat="server" /></td>
		</tr>
		<tr><td class="copyright" colspan="2"><%= GetString("LOGIN.COPYRIGHT") %></td></tr>
	</table>
	
	<div class="warning">
		<asp:Label ID="BrowserWarning" runat="server" />
	</div>
	
</form>

</body>

</html>
