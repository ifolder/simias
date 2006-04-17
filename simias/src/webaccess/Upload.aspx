<%@ Page Language="C#" Codebehind="Upload.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderApp.Web.UploadPage" %>
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
			document.getElementById("UploadFile").select();
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
	
			<div class="actions">
				<div class="action">
					<asp:LinkButton ID="UploadButton" runat="server" />
				</div>
				<div class="action">
					<asp:HyperLink ID="CancelLink" runat="server" />
				</div>
			</div>

		</div>
	
		<div id="content">
		
			<iFolder:Message id="MessageBox" runat="server" />
	
			<div class="section">
				<%= GetString("UPLOAD") %>
			</div>
			
			<div class="main">
				
				<div class="path">
					<asp:Literal ID="ParentPath" runat="server" />
				</div>

				<input id="UploadFile" type="file" runat="server" onKeyDown="return SubmitKeyDown(event, 'UploadButton');" />
				
			</div>
	
		</div>
	
	</form>

</div>

</body>

</html>