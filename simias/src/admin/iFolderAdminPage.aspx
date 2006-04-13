<%@ Page language="c#" Codebehind="iFolderAdminPage.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderWeb.Admin.iFolderAdminPage" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
	
<head>

	<meta http-equiv="Content-Type" content="text/html; charset=iso-8859-1">
	<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">

	<title><%= GetString( "TITLE" ) %></title>
		
	<style type="text/css">
		@import url(css/iFolderAdmin.css);
		@import url(css/iFolderAdminPage.css);
	</style>
	
</head>

<body>

<form runat="server">

	<div id="header">
	
		iFolder Administration
		
		<ul id="tabnav">
		
			<li class="ifolders">
				<a href="iFolders.aspx">iFolders</a>
			</li>
			
			<li class="users">
				<a href="Users.aspx">Users</a>
			</li>
			
			<li class="system">
				<a href="SystemInfo.aspx">System</a></li>
			</ul>
			
	</div>

</form>

</body>

</html>
