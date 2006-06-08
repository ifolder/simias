<%@ Register TagPrefix="iFolder" TagName="TopNavigation" Src="TopNavigation.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="Footer" Src="Footer.ascx" %>
<%@ Page language="c#" Codebehind="ServerDetails.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderWeb.Admin.ServerDetails" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" > 
<html>

<head>

	<meta http-equiv="Content-Type" content="text/html; charset=iso-8859-1">
	<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">

	<title><%= GetString( "TITLE" ) %></title>
		
	<style type="text/css">
		@import url(css/iFolderAdmin.css);
		@import url(css/ServerDetails.css);
	</style>

</head>

<body id="server" runat="server">
	
<form runat="server" ID="Form1">

	<div class="container">
			
		<iFolder:TopNavigation ID="TopNav" Runat="server" />
		
		<div class="leftnav">
		
			<div class="detailnav">
			
				<div class="pagetitle">
				
					<%= GetString( "SERVERDETAILS" ) %>
					
				</div>
				
				<table class="detailinfo">
				
					<tr>
						<th>
							<%= GetString( "NAMETAG" ) %>
						</th>
						
						<td>
							<asp:Literal ID="Name" Runat="server" />
						</td>
					</tr>
					
					<tr>
						<th>
							<%= GetString( "TYPETAG" ) %>
						</th>
						
						<td>
							<asp:Literal ID="Type" Runat="server" />
						</td>
					</tr>

					<tr>
						<th>
							<%= GetString( "DNSNAMETAG" ) %>
						</th>

						<td>
							<asp:Literal ID="DnsName" Runat="server" />
						</td>
					</tr>
					
					<tr>
						<th>
							<%= GetString( "PUBLICIPTAG" ) %>
						</th>
						
						<td>
							<asp:Literal ID="PublicIP" Runat="server" />
						</td>
					</tr>
					
					<tr>
						<th>
							<%= GetString( "PRIVATEIPTAG" ) %>
						</th>
						
						<td>
							<asp:Literal ID="PrivateIP" Runat="server" />
						</td>
					</tr>
					
				</table>

			</div>

			<div class="reportnav">
			
				<div class="pagetitle">
				
					<%= GetString( "SERVERREPORTS" ) %>
						
				</div>
					
				<asp:DropDownList 
					ID="ReportList" 
					Runat="server" 
					AutoPostBack="True" />
				
				<asp:Button 
					ID="DownloadReport" 
					Runat="server" 
					Enabled="False" />
			
			</div>
				
		</div>
		
		<div class="rightnav">
		
			<div class="detailnav">
			
				<div class="pagetitle">
				
					<%= GetString( "SERVERSTATUS" ) %>
					
				</div>
				
				<table class="detailinfo">
				
					<tr>
						<th>
							<%= GetString( "STATUSTAG" ) %>
						</th>
						
						<td>
							<asp:Literal ID="Status" Runat="server" />
						</td>
					</tr>
					
					<tr>
						<th>
							<%= GetString( "USERSTAG" ) %>
						</th>
						
						<td>
							<asp:Literal ID="UserCount" Runat="server" />
						</td>
					</tr>

					<tr>
						<th>
							<%= GetString( "IFOLDERSTAG" ) %>
						</th>

						<td>
							<asp:Literal ID="iFolderCount" Runat="server" />
						</td>
					</tr>
					
					<tr>
						<th>
							<%= GetString( "LOGGEDONUSERSTAG" ) %>
						</th>
						
						<td>
							<asp:Literal ID="LoggedOnUsersCount" Runat="server" />
						</td>
					</tr>
					
					<tr>
						<th>
							<%= GetString( "SESSIONSTAG" ) %>
						</th>
						
						<td>
							<asp:Literal ID="SessionCount" Runat="server" />
						</td>
					</tr>
					
					<tr>
						<th>
							<%= GetString( "DISKSPACEUSEDTAG" ) %>
						</th>
						
						<td>
							<asp:Literal ID="DiskSpaceUsed" Runat="server" />
						</td>
					</tr>
					
					<tr>
						<th>
							<%= GetString( "DISKSPACEAVAILABLETAG" ) %>
						</th>
						
						<td>
							<asp:Literal ID="DiskSpaceAvailable" Runat="server" />
						</td>
					</tr>
					
					<tr>
						<th>
							<%= GetString( "LDAPSTATUSTAG" ) %>
						</th>
						
						<td>
							<asp:Literal ID="LdapStatus" Runat="server" />
						</td>
					</tr>
					
					<tr>
						<th>
							<%= GetString( "MAXCONNECTIONSTAG" ) %>
						</th>
						
						<td>
							<asp:Literal ID="MaxConnectionCount" Runat="server" />
						</td>
					</tr>
					
				</table>

			</div>
			
		</div>
		
		<div class="lognav">
		
			<div class="pagetitle">
			
				<%= GetString( "SERVERLOGS" ) %>
				
			</div>
			
		</div>
		
	</div>
	
	<ifolder:Footer id="footer" runat="server" />
				
</form>
	
</body>

</html>
