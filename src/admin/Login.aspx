<%@ Page language="c#" Codebehind="Login.aspx.cs" ValidateRequest="false" AutoEventWireup="false" Inherits="Novell.iFolderWeb.Admin.Login" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
	
<head>
		
	<meta http-equiv="Content-Type" content="text/html; charset=iso-8859-1">
	<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">
	
	<title>
		<%= GetString("TITLE") %>
	</title>
		
	<link rel="SHORTCUT ICON" href="images/N_url_shortcut.ico">
		
	<style type="text/css">
		@import url(css/iFolderAdmin.css);
		@import url(css/Login.css);
	</style>
		
	<script language="javascript">
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

<body>

<form runat="server">

	<noscript>

		<input 
			type="hidden" 
			id="noscript" 
			value="true" 
			runat="server">

	</noscript>

	<div class="loginpage" align="center">

		<div class="loginbanner">
				
			<%-- <img src="images/login_title.png" alt='<%= GetString("TITLE") %>'> --%>

			<div>
			<asp:HyperLink 
				ID="HelpButton" 
				CssClass="loginHelp" 
				Target="ifolderHelp" 
				NavigateUrl="help/login.html"
				runat="server" />

			</div>
			<div class="ServerIP">
				<%= GetString("LOGINIFOLDERSERVER") %>
			<asp:Literal ID="ServerUrl" runat="server" />
			</div>
		
		</div>

		<div class="logincontent">
		
			<table cellpadding="8px" cellspacing="0" width="80%">
			
				<tr>
					<td class="logincontentlabel" style="display:none">
						<asp:Label ID="MessageType" runat="server" />
					</td>
				
					
					<td class="logincontenttext" colspan="2">
						<asp:Label ID="MessageText" runat="server" />
						&nbsp;
					</td>
				</tr>
				
				<tr>
					<td nowrap class="logincontentlabel">
						<%= GetString("LOGINUSERNAME") %>
					</td>
					
					<td class="logincontenttext">
						<asp:TextBox ID="UserName" runat="server" />
					</td>
				</tr>
				
				<tr>
					<td class="logincontentlabel">
						<%= GetString("LOGINPASSWORD") %>
					</td>
					
					<td class="logincontenttext">
						<asp:TextBox ID="Password" runat="server" TextMode="Password" />
					</td>
				</tr>
				
				<tr>
					<td class="logincontentlabel">
						<%= GetString("LOGINLANGUAGE") %>
					</td>
					
					<td class="logincontenttext">
						<asp:DropDownList ID="LanguageList" runat="server" AutoPostBack="true"/>
					</td>
				</tr>
				
				<tr>
					<td colspan="2" align="right" class="loginbutton">
						<asp:Button ID="LoginButton" runat="server" CssClass="button" />
					</td>
					
				</tr>
				
			</table>
			
		</div>
			
		<div class="loginfooter">
	
			
			&nbsp;&nbsp;<%= GetString("LOGINCOPYRIGHT") %>
		</div>
			
	</div>

</form>

</body>

</html>
