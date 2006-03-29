<%@ Page language="c#" Codebehind="iFolders.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderWeb.Admin.iFolders" %>
<%@ Register TagPrefix="iFolder" TagName="TopNavigation" Src="TopNavigation.ascx" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
	
<head>

	<meta http-equiv="Content-Type" content="text/html; charset=iso-8859-1">
	<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">

	<title><%= GetString( "TITLE" ) %></title>
		
	<style type="text/css">
		@import url(iFolderAdmin.css);
		@import url(iFolders.css);
	</style>
		
</head>

<body id="ifolders" runat="server">

<form runat="server">

	<div class="container">
			
		<iFolder:TopNavigation ID="TopNav" Runat="server" />

		<div class="footer">
		</div>
				
	</div>

</form>

</body>

</html>
