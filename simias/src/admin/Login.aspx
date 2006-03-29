<%@ Page language="c#" Codebehind="Login.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderWeb.Admin.Login" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<HTML>
	<HEAD>
		<title>
			<%= GetString("TITLE") %>
		</title>
		
		<meta http-equiv="Content-Type" content="text/html; charset=iso-8859-1">
		<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">
		
		<link rel="SHORTCUT ICON" href="images/N_url_shortcut.ico">
		
		<style type="text/css">
			@import url(iFolderAdmin.css);
			@import url(Login.css);
		</style>
		
		<script language="javascript">
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
	</HEAD>
	<body>
		<form runat="server">
			<noscript>
				<input type="hidden" id="noscript" value="true" runat="server">
			</noscript>
			<table border="0" cellspacing="0" cellpadding="0" width="100%" height="90%">
				<tr>
					<td width="100%" align="center" valign="middle">
						<table border="0" cellpadding="0" cellspacing="0" class="loginDialog">
							<tr bgcolor="#e50000">
								<td><img src="images/login_title.gif" alt='<%= GetString("TITLE") %>'></td>
								<td align="center" valign="middle"><asp:HyperLink ID="HelpButton" CssClass="loginHelp" Target="ifolderHelp" NavigateUrl="help/login.html"
										runat="server" /></td>
							</tr>
							<tr>
								<td colspan="2">
									<table border="0" cellpadding="8" cellspacing="0" class="loginContent">
										<tr bgcolor="#edeeec">
											<td class="loginMessageType" align="right"><asp:Literal ID="MessageType" runat="server" /></td>
											<td class="loginMessage"><asp:Literal ID="MessageText" runat="server" /></td>
											<td>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</td>
										</tr>
										<tr bgcolor="#edeeec">
											<td align="right"><%= GetString("LOGINUSERNAME") %></td>
											<td><asp:TextBox ID="UserName" runat="server" Width="320" /></td>
											<td>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</td>
										</tr>
										<tr bgcolor="#edeeec">
											<td align="right"><%= GetString("LOGINPASSWORD") %></td>
											<td><asp:TextBox ID="Password" runat="server" TextMode="Password" Width="320" /></td>
											<td>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</td>
										</tr>
										<tr bgcolor="#edeeec">
											<td align="right"><%= GetString("LOGINLANGUAGE") %></td>
											<td><asp:DropDownList ID="LanguageList" runat="server" Width="320" /></td>
											<td>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</td>
										</tr>
										<tr bgcolor="#edeeec">
											<td colspan="2" align="right"><asp:Button ID="LoginButton" runat="server" CssClass="button" /></td>
											<td>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</td>
										</tr>
									</table>
								</td>
							</tr>
							<tr>
								<td class="loginContent">
									<span class="loginContext">
										<%= GetString("LOGINIFOLDERSERVER") %>
										&nbsp;<asp:Literal ID="ServerUrl" runat="server" /></span>
								</td>
								<td class="loginContent" align="center"><img border="0" width="39" height="36" src="images/logo_N_login.gif"></td>
							</tr>
							<tr>
								<td colspan="2" class="copyrightText"><%= GetString("LOGINCOPYRIGHT") %></td>
							</tr>
						</table>
					</td>
				</tr>
			</table>
		</form>
	</body>
</HTML>
