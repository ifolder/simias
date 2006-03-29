<%@ Page language="c#" Codebehind="Error.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderApp.Web.Error" %>
<%@ Register TagPrefix="iFolder" TagName="Header" Src="Header.ascx" %>
<html>
<head>
<title><%= GetString("TITLE") %></title>
<link rel="SHORTCUT ICON" href="images/N_url_shortcut.ico">
<link rel="stylesheet" type="text/css" href="iFolderWeb.css">
</head>
<body>
<form runat="server">

	<div class="pageRegion">
		
		<ifolder:header runat="server" />
			
		<div class="mainRegion">
		
			<div class="mainContent">
				
				<table border="0" cellpadding="0" cellspacing="0">
				
					<tr><td>
						<asp:Label ID="ErrorType" CssClass="errorType" runat="server" />
					</td></tr>
				
					<tr><td>&nbsp;</td></tr>
					
					<tr><td>
						<asp:Label ID="ErrorInstructions" CssClass="errorInstructions" runat="server" />
					</td></tr>
				
					<tr><td>&nbsp;</td></tr>
					
					<tr><td>
					</td></tr>
	
					<tr><td>&nbsp;</td></tr>
					
					<tr><td class="errorDetail"><pre><asp:Literal ID="ErrorMessage" runat="server" /></pre></td></tr>

				</table>
				
			</div>
	
		</div>

	</div>
	
</form>
</body>
</html>
