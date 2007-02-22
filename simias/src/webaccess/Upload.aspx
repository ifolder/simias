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

	<script type="text/javascript" src="js/multifile.js"></script>

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

				<div>
					<input id="file_0" name="file_0" type="file" size="0" runat="server" />
				</div>
				
				<div id="files">
				</div>
				
				<script type="text/javascript">
					var mf = new MultiFile(document.getElementById('files'), 10);
					mf.addElement(document.getElementById('file_0'));
					mf.button_text = "<%= GetString("FILES.REMOVE") %>";
				</script>
				<div class="labels"></div>
					<asp:Label ID="PassPhraseLabel" Visible="false" runat="server" />
					<asp:TextBox ID="PassPhraseText" Visible="false" runat="server" /><br><br>
				
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