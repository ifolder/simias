<%@ Page Language="C#" Codebehind="NewFolder.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderApp.Web.NewFolderPage" %>
<%@ Register TagPrefix="iFolder" TagName="Header" Src="Header.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="Message" Src="Message.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="Pagging" Src="Pagging.ascx" %>
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
			document.getElementById("NewFolderName").select();
		}
		
		// on load
		window.onload = SetFocus;
	
	</script>

</head>

<body>

<div id="container">
	
	<form runat="server">

		<iFolder:Header runat="server" />
		
		<div id="nav">
		</div>
	
		<div id="content">
		
			<iFolder:Message id="MessageBox" runat="server" />
	
			<div class="section">
				<%= GetString("NEWFOLDER") %>
			</div>
			
			<div class="main">
				
				<div class="path">
					<asp:Literal ID="ParentPath" runat="server" />
				</div>

				<div class="label"><%= GetString("NAME") %></div>
				<asp:TextBox ID="NewFolderName" onkeydown="return SubmitKeyDown(event, 'CreateButton');" runat="server" />
				
				<div class="buttons">
					<asp:Button ID="CreateButton" runat="server" />
					<asp:Button ID="CancelButton" runat="server" />
				</div>

			</div>
	
		</div>
	
	</form>

</div>

</body>

</html>