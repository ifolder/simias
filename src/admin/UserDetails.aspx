<%@ Register TagPrefix="iFolder" TagName="Footer" Src="Footer.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="Policy" Src="Policy.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="ListFooter" Src="ListFooter.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="TopNavigation" Src="TopNavigation.ascx" %>
<%@ Page language="c#" Codebehind="UserDetails.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderWeb.Admin.Details" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>

<head>
	
	<meta http-equiv="Content-Type" content="text/html; charset=iso-8859-1">
	<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">
	
	<title><%= GetString( "TITLE" ) %></title>
	
	<style type="text/css">
		@import url(css/iFolderAdmin.css);
		@import url(css/UserDetails.css);
	</style>
        <script language="javascript">

        function EnableSaveButton()
        {
               document.getElementById( "GroupDiskQuotaSave" ).disabled = false;
        }

        </script>

	
</head>

<body id="users" runat="server" style="height:100%">

<form runat="server">

	<div class="container">
	
	<iFolder:TopNavigation ID="TopNav" Runat="server" />
	
		<div class="leftnav">
	
			<div class="detailnav">
		
				<div class="pagetitle">
				
					<%= GetString( "USERDETAILS" ) %>
				
				</div>
			
				<table class="detailinfo">
			
					<tr>
						<th>
							<%= GetString( "USERNAMETAG" ) %>
						</th>
					
						<td>
							<asp:Literal ID="UserName" Runat="server" />
						</td>
					</tr>
			
					<tr>
						<th>
							<%= GetString( "FULLNAMETAG" ) %>
						</th>
					
						<td>
							<asp:Literal ID="FullName" Runat="server" />
						</td>
					</tr>
					<tr>
						<th>
							<%= GetString( "MEMBERTYPETAG" ) %>
						</th>
					
						<td>
							<asp:Literal ID="MemberType" Runat="server" />
						</td>
					</tr>
					
					<tr>
						<th>
							<%= GetString( "LDAPCONTEXTTAG" ) %>
						</th>
					
						<td>
								<asp:Literal ID="LdapContext" Runat="server" />
						</td>
					</tr>
				
					<tr>
						<th>
							<%= GetString( "LASTLOGINTIMETAG" ) %>
						</th>
					
						<td>
							<asp:Literal ID="LastLogin" Runat="server" />
						</td>
					</tr>

					<tr>
						<th>
							<asp:Literal ID="GroupDiskQuotaHeader" Visible="false"  Runat="server" />	
						</th>
					
						<td>
							<asp:TextBox ID="GroupDiskQuotaText" Visible="false" size="6" onkeypress="EnableSaveButton()" OnTextChanged="LimitChanged" Runat="server" />
							<asp:Literal ID="GroupDiskQuotaLiteral" Visible="false" Runat="server" />
						</td>
						<td>
							<asp:Button ID="GroupDiskQuotaSave" Enabled="False" Visible="false" OnClick="SaveGroupDiskQuota" Runat="server" />
						</td>
					</tr>

					<tr>
						<th>
							<asp:Literal ID="DiskQuotaUsedHeader" Visible="false"  Runat="server" />	
						</th>
					
						<td>
							<asp:Literal ID="DiskQuotaUsedLiteral" Visible="false" Runat="server" />
						</td>
					</tr>

					<tr>
						<th>
							<asp:Literal ID="MembersTag" Runat="server" />
						</th>
					
						<td>
							<asp:Literal ID="MembersList" Runat="server" />
						</td>
					</tr>
				
				</table>
			
			</div>
		
			<div class="ifolderlistnav">
		
				<div class="pagetitle">
			
					<%= GetString( "IFOLDERS" ) %>
				
				</div>
			
				<div id="CurrentTab" runat="server" class="ifoldertabnav">
				
					<ul id="ifolderlisttab">
					
						<li class="allifolders">
							<asp:LinkButton 
								ID="AlliFoldersLink" 
								Runat="server" 
								OnClick="AlliFolders_Clicked" />
						</li>
						
						<li class="ownedifolders">
							<asp:LinkButton 
								ID="OwnediFoldersLink" 
								Runat="server" 
								OnClick="OwnediFolders_Clicked" />
						</li>
						
						<li class="sharedifolders">
							<asp:LinkButton 
								ID="SharediFoldersLink" 
								Runat="server" 
								OnClick="SharediFolders_Clicked" />
						</li>
						
					</ul>
				
				</div>

				<table class="ifolderlistheader" cellpadding="0" cellspacing="0" border="0">
			
					<tr>
						<td class="checkboxcolumn">
							<asp:CheckBox 
								ID="AlliFoldersCheckBox" 
								Runat="server" 
								OnCheckedChanged="OnAlliFoldersChecked" 
								AutoPostBack="True"	/>
						</td>
					
						<td class="typecolumn">
							<%= GetString( "TYPE" ) %>
						</td>
						
						<td class="namecolumn">
							<%= GetString( "NAME" ) %>
						</td>
						
						<td class="ownercolumn">
							<%= GetString( "OWNER" ) %>
						</td>
					</tr>
			
				</table>

				<asp:datagrid 
					id="iFolderList" 
					runat="server" 
					AutoGenerateColumns="False" 
					PageSize="11" 
					CellPadding="0"
					CellSpacing="0" 
					GridLines="None" 
					ShowHeader="False" 
					CssClass="ifolderlist" 
					AlternatingItemStyle-CssClass="ifolderlistaltitem" 
					ItemStyle-CssClass="ifolderlistitem">
					
					<Columns>
					
						<asp:BoundColumn DataField="IDField" Visible="False" />
						
						<asp:BoundColumn DataField="DisabledField" Visible="False" />
						
						<asp:TemplateColumn ItemStyle-CssClass="ifolderitem1">
							<ItemTemplate>
								<asp:CheckBox 
									ID="iFolderListCheckBox" 
									Runat="server" 
									OnCheckedChanged="OniFolderChecked" 
									AutoPostBack="True" 
									Visible='<%# DataBinder.Eval( Container.DataItem, "VisibleField" ) %>' 
									Enabled='<%# IsiFolderEnabled( DataBinder.Eval( Container.DataItem, "PreferenceField" ) ) %>'
									Checked='<%# GetMemberCheckedState( DataBinder.Eval( Container.DataItem, "IDField" ) ) %>' />
							</ItemTemplate>
						</asp:TemplateColumn>
						
						<asp:TemplateColumn ItemStyle-CssClass="ifolderitem2">
							<ItemTemplate>
								<asp:Image 
									ID="iFolderListImage" 
									ToolTip=<%# DataBinder.Eval(Container.DataItem, "FullNameField") %>
									Runat="server"  
									ImageUrl='<%# GetiFolderImage( DataBinder.Eval( Container.DataItem, "DisabledField" ), DataBinder.Eval( Container.DataItem, "SharedField" ), DataBinder.Eval( Container.DataItem, "EncryptedField" ) ) %>'
									Visible='<%# DataBinder.Eval( Container.DataItem, "VisibleField" ) %>'/>
							</ItemTemplate>
						</asp:TemplateColumn>
						
							<asp:TemplateColumn ItemStyle-CssClass="ifolderitem3">
							<ItemTemplate>
								<asp:HyperLink 
									ToolTip=<%# DataBinder.Eval(Container.DataItem, "FullNameField") %>
									Runat="server" 
									Target="_top" 
									NavigateUrl='<%#GetiFolderUrl( DataBinder.Eval( Container.DataItem, "ReachableField" ), DataBinder.Eval( Container.DataItem, "IDField" ) ) %>'>

									<%# DataBinder.Eval(Container.DataItem, "NameField" ) %>
								</asp:HyperLink>
							</ItemTemplate>
						</asp:TemplateColumn>
							
						<asp:TemplateColumn ItemStyle-CssClass="ifolderitem4">
							<ItemTemplate>
								<asp:HyperLink 
									Runat="server" 
									Target="_top" 
									NavigateUrl='<%#GetOwnerUrl( DataBinder.Eval( Container.DataItem, "OwnerIDField" ) ) %>'>
									<%# DataBinder.Eval(Container.DataItem, "OwnerNameField" ) %>
								</asp:HyperLink>
							</ItemTemplate>
						</asp:TemplateColumn>

						<asp:BoundColumn DataField="OwnerIDField" Visible="False" />
						<asp:BoundColumn DataField="SharedField" Visible="False" />
						<asp:BoundColumn DataField="ReachableField" Visible="False" />
						<asp:BoundColumn DataField="FullNameField" Visible="False" />
						<asp:BoundColumn DataField="PreferenceField" Visible="False" />
						
					</Columns>
					
				</asp:datagrid>
				
				<ifolder:ListFooter ID="iFolderListFooter" Runat="server" />
				
				<asp:Button 
					ID="DeleteiFolderButton" 
					Runat="server" 
					Enabled="False" 
					CssClass="actionbuttons"
					OnClick="OnDeleteiFolderButton_Click" />
					
				<asp:Button 
					ID="DisableiFolderButton" 
					Runat="server" 
					CssClass="actionbuttons" 
					Enabled="False"
					OnClick="OnDisableiFolder" />
					
				<asp:Button 
					ID="EnableiFolderButton" 
					Runat="server" 
					CssClass="actionbuttons" 
					Enabled="False"
					OnClick="OnEnableiFolder" />
					
<%--				<asp:Button 
					ID="CreateiFolderButton" 
					Runat="server" 
					CssClass="actionbuttons" 
					OnClick="OnCreateiFolder" /> --%>
					
			</div>
			
		</div>
		
		<div class="rightnav">
		
			<ifolder:Policy ID="Policy" Runat="server" />
			
		</div>
		
	</div>
	
<%--	<ifolder:Footer id="footer" runat="server" /> --%>
		
</form>

</body>

</html>
