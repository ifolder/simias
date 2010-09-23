<%@ Register TagPrefix="iFolder" TagName="Footer" Src="Footer.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="Policy" Src="Policy.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="ListFooter" Src="ListFooter.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="TopNavigation" Src="TopNavigation.ascx" %>
<%@ Page language="c#" Codebehind="userMoveDetails.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderWeb.Admin.UserMoveDetails" %>
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
	
</head>

<body id="system" runat="server" style="height:100%">

<form runat="server">

	<div class="container">
	
	<iFolder:TopNavigation ID="TopNav" Runat="server" />
	
	     <div class="leftnavuserdetails">

			<div class="nav"> 
		
				<div class="pagetitle">
				
					<%= GetString( "USERMOVEDETAILS" ) %>
				
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
							<%= GetString( "LDAPCONTEXTTAG" ) %>
						</th>
					
						<td>
								<asp:Literal ID="LdapContext" Runat="server" />
						</td>
					</tr>
                    
	                				
					<tr>
						<th>
							<%= GetString( "CURRENTHOMETAG" ) %>
						</th>
					
						<td>
								<asp:Literal ID="CurrentHome" Runat="server" />
						</td>
					</tr>


					<tr>
						<th>
							<%= GetString( "NEWHOMETAG" ) %>
						</th>
					
						<td>
								<asp:Literal ID="NewHome" Runat="server" />
						</td>
					</tr>


					<tr>
						<th>
							<%= GetString( "COMPLETEDTAG" ) %>
						</th>
					
						<td>
								<asp:Literal ID="Completed" Runat="server" />
						</td>
					</tr>
                    
					 
					<tr>
						<th>
							<%= GetString( "REPROVISOINSTATETAG" ) %>
						</th>
					
						<td>
								<asp:Literal ID="ReprovState" Runat="server" />
						</td>
					</tr>

				
				</table>
			
			</div> 
		
			<div class="ifolderlistnav">
		
				<div class="pagetitle">
			
					<%= GetString( "IFOLDERS" ) %>
				
				</div>
			
			<%-- 	<div id="CurrentTab" runat="server" class="ifoldertabnav">
				
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
				
				</div> --%>

				<table class="ifolderlistheader" cellpadding="0" cellspacing="0" border="0">
			
					<tr>
						<td class="typecolumn">
							<%= GetString( "TYPE" ) %>
						</td>
						
						<td class="namecolumnuserdetails">
							<%= GetString( "NAME" ) %>
						</td>
						
						<td class="ownercolumn">
							<%= GetString( "OWNER" ) %>
						</td>

						<td class="ownercolumn">
							<%= GetString( "SIZE" ) %>
						</td>

						<td class="ownercolumn">
							<%= GetString( "STATUS" ) %>
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
						
							<asp:TemplateColumn ItemStyle-CssClass="ifolderitem5">
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

                       <asp:BoundColumn ItemStyle-CssClass="ifolderitem4" DataField="folderSize"/> 
                       <asp:BoundColumn ItemStyle-CssClass="ifolderitem4" DataField="folderMoveState"/> 
						
					</Columns>
					
				</asp:datagrid>
				
				<ifolder:ListFooter ID="iFolderListFooter" Runat="server" />
				
<%--				<asp:Button 
					ID="DeleteiFolderButton" 
					Runat="server" 
					Enabled="False" 
					CssClass="actionbuttons"
					OnClick="OnDeleteiFolder" /> 
					
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
					
				<asp:Button 
					ID="CreateiFolderButton" 
					Runat="server" 
					CssClass="actionbuttons" 
					OnClick="OnCreateiFolder" /> --%>
					
			</div>
			
	</div>
	</div>
	
<%--	<ifolder:Footer id="footer" runat="server" /> --%>
		
</form>

</body>

</html>
