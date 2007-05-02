<%@ Page language="c#" Codebehind="Error.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderWeb.Admin.Error" %>
<%@ Register TagPrefix="iFolder" TagName="TopNavigation" Src="TopNavigation.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="Footer" Src="Footer.ascx" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" > 
<html>

<head>

	<meta http-equiv="Content-Type" content="text/html; charset=iso-8859-1">
	<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">

	<title><%= GetString( "TITLE" ) %></title>
		
	<style type="text/css">
		@import url(css/iFolderAdmin.css);
		@import url(css/Error.css);
	</style>

</head>

<body id="users" runat="server">
	
<form runat="server">

	<div class="container">
			
		<iFolder:TopNavigation ID="TopNav" Runat="server" />
	
		<div id="ExceptionNav" runat="server" class="exceptionnav">
		
			<div class="pagetitle">

				<%= GetString( "ERRORDETAIL" ) %>
					
			</div>
		
			<asp:TextBox 
				ID="StackDump" 
				Runat="server" 
				CssClass="stackdump" 
				ReadOnly="True" 
				Rows="15" 
				TextMode="MultiLine" 
				Wrap="True" />
	
		</div>
		
	</div>
	
	<ifolder:Footer id="footer" runat="server" />
				
</form>
	
</body>

</html>
