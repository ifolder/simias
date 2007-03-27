<%@ Page Language="C#" Codebehind="iFolderNew.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderApp.Web.iFolderNewPage" %>
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

	<script type="text/javascript">

		function SubmitKeyDown(e, b)
		{
			var result = true;
			
			if ((e.which && e.which == 13) || (e.keyCode && e.keyCode == 13))
			{
				document.getElementById(b).click();
				result = false;
			} 
			
			return result;
		}
		
		
		function SetFocus()
		{
			document.getElementById("NewiFolderName").select();
		}
		
		// on load
		window.onload = SetFocus;
	
	</script>

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
				<%= GetString("NEWIFOLDER") %>
			</div>
			
			<div class="main">
				
				<div class="label" ><b><%= GetString("NAME") %></b></div>
					<asp:TextBox ID="NewiFolderName" onkeydown="return SubmitKeyDown(event, 'CreateButton');" runat="server" />
				
				<div class="label"><b><%= GetString("DESCRIPTION") %></b></div>
					<asp:TextBox ID="NewiFolderDescription" TextMode="MultiLine" Rows="4" onkeydown="return SubmitKeyDown(event, 'CreateButton');" runat="server" /><br><br>
		
				<div class="label"><b><%= GetString("TYPE") %></b></div>
					<asp:RadioButton ID="Encryption" runat="server" GroupName="SecurityGroup" AutoPostBack="true" />
					<asp:RadioButton ID="shared" runat="server" GroupName="SecurityGroup" AutoPostBack="true" /> <br> <br>
				
			
				<div class="label">
					<asp:Label ID="SelectLabel" runat="server" />
					<asp:DropDownList ID="RAList" runat="server" /> <br><br>
				</div>
				
				<div class="label">
					<asp:Label ID="PassPhraseLabel" runat="server" />
					<asp:TextBox ID="PassPhraseText"  TextMode="Password" runat="server" /><br><br>
				</div>
				
				<div class="label">
					<asp:Label ID="VerifyPassPhraseLabel" runat="server" />
					<asp:TextBox ID="VerifyPassPhraseText"  TextMode="Password" runat="server" onkeydown="return SubmitKeyDown(event, 'CreateButton');"/><br><br>
				</div>
				
				<div class="label">
					<asp:Button ID="CreateButton" runat="server" />
					<asp:Button ID="CancelButton" runat="server" />
				</div>

			</div>
	
		</div>
	
	</form>

</div>

</body>

</html>
