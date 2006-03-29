<%@ Page language="c#" Codebehind="SystemInfo.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderWeb.Admin.SystemInfo" %>
<%@ Register TagPrefix="iFolder" TagName="TopNavigation" Src="TopNavigation.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="Policy" Src="Policy.ascx" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
	
<head>
	
	<meta http-equiv="Content-Type" content="text/html; charset=iso-8859-1">
	<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">
	
	<title><%= GetString( "TITLE" ) %></title>
	
	<style type="text/css">
		@import url(iFolderAdmin.css);
		@import url(SystemInfo.css);
	</style>
	
</head>

<body id="system" runat="server">
	
<form runat="server">
			
	<div class="container">
			
		<iFolder:TopNavigation ID="TopNav" Runat="server" />

		<div class="leftnav">
		
			<div class="detailnav">
			
				<h3><%= GetString( "SYSTEMSETTINGS" ) %></h3>
				
				<table class="detailinfo">
				
					<tr>
						<th>
							<%= GetString( "NAMETAG" ) %>
						</th>
						
						<td>
							<asp:TextBox ID="Name" Runat="server" CssClass="edittext" />
						</td>
					</tr>
					
					<tr>
						<th>
							<%= GetString( "DESCRIPTIONTAG" ) %>
						</th>
						
						<td>
							<textarea id="Description" runat="server" class="edittext" Rows="2" wrap="soft"></textarea>
						</td>
					</tr>
					
					<tr>
						<th>
							<%= GetString( "TOTALUSERSTAG" ) %>
						</th>
						
						<td>
							<asp:Literal ID="NumberOfUsers" Runat="server" />
						</td>
					</tr>
					
					<tr>
						<th>
							<%= GetString( "TOTALIFOLDERSTAG" ) %>
						</th>
						
						<td>
							<asp:Literal ID="NumberOfiFolders" Runat="server" />
						</td>
					</tr>
					
				</table>

			</div>

		</div>
		
		<ifolder:Policy ID="Policy" Runat="server" />		
		
		<div class="footer">
		</div>
				
	</div>
		
</form>

</body>

</html>
