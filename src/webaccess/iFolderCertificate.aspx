<%@ Page Language="C#" Codebehind="iFolderCertificate.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderApp.Web.iFolderCertificatePage" %>
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

	
	</script>

</head>

<body>

<div id="container">
	
	<form runat="server">

		<iFolder:HeaderControl id="Head" runat="server" />
		
		<div id="nav">
		</div>
	
		<div id="content">
			
			<iFolder:MessageControl id="Message" runat="server" />
			
			<div class="main">
		
				<div class="label" ></div>
				<asp:Literal ID="Certificate" runat="server" /> <br> <br>
				<asp:TextBox ID="CertDetails" columns="50" rows="12" ReadOnly="true" TextMode="MultiLine" BorderStyle="Ridge" runat="server" />
			
				<div class="buttons">
					<asp:Button ID="AcceptButton" runat="server" />
					<asp:Button ID="DenyButton" runat="server" />
				</div>

			</div>
	
		</div>
	
	</form>

</div>

</body>

</html>
