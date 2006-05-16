<%@ Page Language="C#" Codebehind="Upload.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderApp.Web.UploadPage" %>
<%@ Register TagPrefix="iFolder" TagName="HeaderControl" Src="Header.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="MessageControl" Src="Message.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="QuotaControl" Src="Quota.ascx" %>
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
			document.getElementById("UploadFile1").select();
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

			<iFolder:QuotaControl runat="server" />

		</div>
	
		<div id="content">
		
			<iFolder:MessageControl id="Message" runat="server" />
	
			<div class="section">
				<%= GetString("UPLOAD") %>
			</div>
			
			<div class="main">
				
				<div class="path">
					<asp:Image ImageUrl="images/folder.png" runat="server" />
					<asp:Literal ID="ParentPath" runat="server" />
				</div>

				<div class="files">
					
					<div>
						<input id="UploadFile1" type="file" size="32" runat="server" onKeyDown="return SubmitKeyDown(event, 'UploadButton');" />
					</div>
					
					<div>
						<input id="UploadFile2" type="file" size="32" runat="server" onKeyDown="return SubmitKeyDown(event, 'UploadButton');" />
					</div>
					
					<div>
						<input id="UploadFile3" type="file" size="32" runat="server" onKeyDown="return SubmitKeyDown(event, 'UploadButton');" />
					</div>
					
					<div>
						<input id="UploadFile4" type="file" size="32" runat="server" onKeyDown="return SubmitKeyDown(event, 'UploadButton');" />
					</div>
					
					<div>
						<input id="UploadFile5" type="file" size="32" runat="server" onKeyDown="return SubmitKeyDown(event, 'UploadButton');" />
					</div>
					
				</div>

				<div class="buttons">
					<asp:Button ID="UploadButton" runat="server" />
					<asp:Button ID="CancelButton" runat="server" />
				</div>
				
			</div>
	
		</div>
	
	</form>

</div>

</body>

</html>