<%@ Page Language="C#" Codebehind="iFolderNew.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderApp.Web.iFolderNewPage" %>
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
				__doPostBack(b,'');
				//document.getElementById(b).click();
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

		<iFolder:Header runat="server" />
		
		<div id="nav">
	
			<div class="actions">
				<div class="action">
					<asp:LinkButton ID="CreateButton" runat="server" />
				</div>
				<div class="action">
					<asp:HyperLink ID="CancelLink" NavigateUrl="iFolders.aspx" runat="server" />
				</div>
			</div>

		</div>
	
		<div id="content">
		
			<iFolder:Message id="MessageBox" runat="server" />
	
			<div class="section">
				<%= GetString("NEWIFOLDER") %>
			</div>
			
			<div class="main">
				
				<div class="label"><%= GetString("NAME") %></div>
				<asp:TextBox ID="NewiFolderName" onkeydown="return SubmitKeyDown(event, 'CreateButton');" runat="server" />
				
				<div class="label"><%= GetString("DESCRIPTION") %></div>
				<asp:TextBox ID="NewiFolderDescription" TextMode="MultiLine" Rows="4" onkeydown="return SubmitKeyDown(event, 'CreateButton');" runat="server" />
				
			</div>
	
		</div>
	
	</form>

</div>

</body>

</html>
