<%@ Page language="c#" Codebehind="AdminRights.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderWeb.Admin.AdminRights" %>
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
		@import url(css/Reports.css);
	</style>

	<script language="javascript">
		function EnableSystemButtons()
		{
			document.getElementById( "SaveAdminRights" ).disabled = false;
			document.getElementById( "CancelAdminRights" ).disabled = false;
		}
	</script>

</head>

<body id="system" runat="server">
	
<form runat="server" >

	<div class="container">
			
		<iFolder:TopNavigation ID="TopNav" Runat="server" />
		
		<div class="leftnav">

<%-- 			<div class="detailnav"> --%>

				<div class="pagetitle">
				
<%--					<%= GetString( "EDITADMINRIGHTS" ) %>--%>
					<asp:Label ID="AdminFullNameLabel" runat="server" />
					
				</div>
		
					<tr>
						<td class="whenlabel">
						
							Select Group
							
						</td>
						
						<td>
						
							<asp:DropDownList 
								ID="GroupList" 
								Runat="server" 
								AutoPostBack="True" 
								OnSelectedIndexChanged="OnGroupList_Changed" />
						<br>
						</td>
					</tr>

					<tr>
						<td>
							<div class="label">
								<asp:Label ID="AggregateDiskQuotaLabel" runat="server" />
								<asp:TextBox ID="AggregateDiskQuotaText"  runat="server"/>
								<asp:Label ID="StorageUnitLabel" runat="server" /><br>
							</div>
							<br>		
						</td>
					</tr>
				
				Set Policy Rights For Secondary Administrator
				<table class="policysmallclass" >
				
					<tr>
						<th colspan="2">
						
							iFolders per User Policy
							
						</th>
					</tr>
			
					<tr>
						<td>
						
							<asp:CheckBoxList 
								ID="NoOfiFoldersList" 
								Runat="server" 
								AutoPostBack="True" 
								RepeatDirection="Vertical"
								OnSelectedIndexChanged="OnNoOfiFoldersList_Changed">
								
								<asp:ListItem></asp:ListItem>
								
							</asp:CheckBoxList>
							
						</td>	
					</tr>
					
				</table>

				<table class="policysmallclass" >
				
					<tr>
						<th colspan="2">
						
							Disk Quota Policy
							
						</th>
					</tr>
			
					<tr>
						<td>
						
							<asp:CheckBoxList 
								ID="DiskQuotaRightsList" 
								Runat="server" 
								AutoPostBack="True" 
								RepeatDirection="Vertical"
								OnSelectedIndexChanged="OnDiskQuotaRightsList_Changed">
								
								<asp:ListItem></asp:ListItem>
								
							</asp:CheckBoxList>
							
						</td>	
					</tr>
					
				</table>

				<table class="policysmallclass" >
				
					<tr>
						<th colspan="2">
								
							File Size Policy

						</th>
					</tr>
			
					<tr>
						<td>
						
							<asp:CheckBoxList 
								ID="FileSizeRightsList" 
								Runat="server" 
								AutoPostBack="True" 
								RepeatDirection="Vertical"
								OnSelectedIndexChanged="OnFileSizeRightsList_Changed">
								
								<asp:ListItem></asp:ListItem>
							</asp:CheckBoxList>
							
						</td>	
					</tr>
					
				</table>

				<table class="policysmallclass" >
				
					<tr>
						<th colspan="2">
								
							Sync Interval Policy 

						</th>
					</tr>
			
					<tr>
						<td>
						
							<asp:CheckBoxList 
								ID="SyncIntervalRightsList" 
								Runat="server" 
								AutoPostBack="True" 
								RepeatDirection="Vertical"
								OnSelectedIndexChanged="OnSyncIntervalRightsList_Changed">
								
								<asp:ListItem></asp:ListItem>
								
							</asp:CheckBoxList>
							
						</td>	
					</tr>
					
				</table>

				<table class="policysmallclass" >
				
					<tr>
						<th colspan="2">
								
							Excluded File List Policy

						</th>
					</tr>
			
					<tr>
						<td>
						
							<asp:CheckBoxList 
								ID="FileListRightsList" 
								Runat="server" 
								AutoPostBack="True" 
								RepeatDirection="Vertical"
								OnSelectedIndexChanged="OnFileListRightsList_Changed">
								
								<asp:ListItem></asp:ListItem>
								
							</asp:CheckBoxList>
							
						</td>	
					</tr>
					
				</table>

				<table class="policysmallclass" >
				
					<tr>
						<th colspan="2">
								
							Sharing Policy

						</th>
					</tr>
			
					<tr>
						<td>
						
							<asp:CheckBoxList 
								ID="SharingRightsList" 
								Runat="server" 
								AutoPostBack="True" 
								RepeatDirection="Vertical"
								OnSelectedIndexChanged="OnSharingRightsList_Changed">
								
								<asp:ListItem></asp:ListItem>
								
							</asp:CheckBoxList>
							
						</td>	
					</tr>
					
				</table>

				<table class="policyclass" >
				
					<tr>
						<th colspan="2">
								
							Encryption Policy

						</th>
					</tr>
			
					<tr>
						<td>
						
							<asp:CheckBoxList 
								ID="EncryptionRightsList" 
								Runat="server" 
								AutoPostBack="True" 
								RepeatDirection="Vertical"
								OnSelectedIndexChanged="OnEncryptionRightsList_Changed">
								
								<asp:ListItem></asp:ListItem>
								
							</asp:CheckBoxList>
							
						</td>	
					</tr>
					
				</table>

				<table class="policyclass" >
				
					<tr>
						<th colspan="2">
								
							Provisioning Rights	

						</th>
					</tr>
			
					<tr>
						<td>
						
							<asp:CheckBoxList 
								ID="ProvisioningRightsList" 
								Runat="server" 
								AutoPostBack="True" 
								RepeatDirection="Vertical"
								OnSelectedIndexChanged="OnProvisioningRightsList_Changed">
								
								<asp:ListItem></asp:ListItem>
								<asp:ListItem></asp:ListItem>
								
							</asp:CheckBoxList>
							
						</td>	
					</tr>
					
				</table>

				<table class="policyclass" >

					<tr>
						<th colspan="2">
								
							Rights on iFolders

						</th>
					</tr>
			
					<tr>
						<td>
						
							<asp:CheckBoxList 
								ID="iFolderRightsList" 
								Runat="server" 
								AutoPostBack="True" 
								RepeatDirection="Vertical"
								OnSelectedIndexChanged="OniFolderRightsList_Changed">
								
								<asp:ListItem></asp:ListItem>
								<asp:ListItem></asp:ListItem>
								<asp:ListItem></asp:ListItem>
								<asp:ListItem></asp:ListItem>
								
							</asp:CheckBoxList>
							
						</td>	
					</tr>
					
				</table>

				<div class="reportbuttons">
					<br><br>	
					<asp:Button 
						ID="SaveAdminRights" 
						Runat="server" 
						CssClass="ifoldersavebutton"
						Enabled="False"
						OnClick="OnSaveAdminRights_Click" />
						
					<asp:Button 
						ID="CancelAdminRights" 
						Runat="server" 
						CssClass="ifoldersavebutton"
						OnClick="OnCancelAdminRights_Click" />
					
				</div>
				
<%--			</div> --%>
			
		</div>

	</div>
	
<%-- 	<ifolder:Footer id="footer" runat="server" /> --%>
				
</form>
	
</body>

</html>
