<%@ Page Language="C#" Codebehind="iFolderEdit.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderApp.Web.iFolderEditPage" %>
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
			document.getElementById("iFolderDescription").select();
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
				<%= GetString("EDIT") %>
			</div>
			
			<div class="main">
				
				<div class="path">
					<asp:Image ImageUrl="images/ifolder.png" runat="server" />
					<asp:Literal ID="iFolderName" runat="server" />
				</div>

				<div class="label"><%= GetString("DESCRIPTION") %></div>
				<asp:TextBox ID="iFolderDescription" TextMode="MultiLine" Rows="4" Width="30em" onkeydown="return SubmitKeyDown(event, 'SaveButton');" runat="server" />
	
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