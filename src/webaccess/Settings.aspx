<%@ Page language="c#" Codebehind="Settings.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderApp.Web.SettingsPage" %>
<%@ Register TagPrefix="iFolder" TagName="HeaderControl" Src="Header.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="HomeContextControl" Src="HomeContext.ascx" %>
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

		<iFolder:HeaderControl id="Head" runat="server" />

		<iFolder:HomeContextControl id="HomeContext" runat="server" />
		
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

				<table class="when" > 
					<tr>
					<asp:CheckBox
						ID="ChangePassword"
						Runat="server"
						AutoPostBack="True"
						OnCheckedChanged="OnChangePassword_Changed" />
	
						<asp:Label ID="ChangePasswordLabel" Runat="server" /> <br>
						<th colspan="2">
						</th>
					</tr> 
					<tr>
						<td>
							<br>
							<div class="label"> 
								<asp:Label ID="CurrentPasswordLabel" CssClass="pwdlabels" runat="server" />  
								<asp:TextBox ID="CurrentPasswordText"  TextMode="Password" CssClass="curpwdtextbox" runat="server" /> 
							</div>
							<br><br>
							<div class="label">
								<asp:Label ID="NewPasswordLabel" CssClass="pwdlabels" runat="server" /> 
								<asp:TextBox ID="NewPasswordText"  TextMode="Password" CssClass="newpwdtextbox" runat="server" />
							</div>
							<br>
							<div class="label">
								<asp:Label ID="VerifyNewPasswordLabel" CssClass="pwdlabels" runat="server" /> 
								<asp:TextBox ID="VerifyNewPasswordText"  TextMode="Password" CssClass="verifynewpwdtextbox" runat="server" /><br>
							</div>
						</td>
					</tr>
				</table> 
				<div class="settingsbuttons">
					<br>
					<asp:Button ID="SaveButton" runat="server" />
					<asp:Button ID="CancelButton" runat="server" />
				</div>

			</div>
	
		</div>
	
	</form>

</div>

</body>

</html>
