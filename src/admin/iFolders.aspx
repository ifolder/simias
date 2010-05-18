<%@ Page language="c#" Codebehind="iFolders.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderWeb.Admin.iFolders" %>
<%@ Register TagPrefix="iFolder" TagName="ListFooter" Src="ListFooter.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="iFolderSearch" Src="iFolderSearch.ascx" %>
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
		@import url(css/iFolders.css);
	</style>
		
</head>

<body id="ifolders" runat="server">

<form runat="server">

	<div class="container">
			
		<iFolder:TopNavigation ID="TopNav" Runat="server" />

		<div class="nav">
		
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
	
					<li class="orphanedifolders">
						<asp:LinkButton
							ID="OrphanediFoldersLink"
							Runat="server"
							OnClick="OrphanediFolders_Clicked" />
					</li>
	
				</ul>

			</div>

			
			<iFolder:iFolderSearch ID="iFolderSearch" Runat="server" />
					
			<div class="ifoldersnav">
			
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
						
						<td class="memberscolumn">
							<%= GetString( "MEMBERS" ) %>
						</td>
						
						<td class="lastmodifiedcolumn">
							<%= GetString( "LASTMODIFIED" ) %>
						</td>
						
					</tr>
			
				</table>
			
				<asp:datagrid 
					id="iFolderList" 
					runat="server" 
					AutoGenerateColumns="False" 
					PageSize="15" 
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
									Runat="server" 
									ImageUrl='<%# GetiFolderImage( DataBinder.Eval( Container.DataItem, "DisabledField" ), DataBinder.Eval( Container.DataItem, "SharedField" ), DataBinder.Eval( Container.DataItem, "EncryptedField" ) ) %>'
									ToolTip='<%# DataBinder.Eval( Container.DataItem, "FullNameField" ) %>'
									Visible='<%# DataBinder.Eval( Container.DataItem, "VisibleField" ) %>'/>
							</ItemTemplate>
						</asp:TemplateColumn>
						
						<asp:HyperLinkColumn 
							ItemStyle-CssClass="ifolderitem3" 
							DataTextField="NameField" 
							DataNavigateUrlField="IDField"
							DataNavigateUrlFormatString="iFolderDetailsPage.aspx?id={0}" 
							Target="_top" />
							
						<asp:TemplateColumn ItemStyle-CssClass="ifolderitem4">
							<ItemTemplate>
								<asp:HyperLink 
									Runat="server" 
									Target="_top" 
									NavigateUrl='<%#GetOwnerUrl( DataBinder.Eval( Container.DataItem, "OwnerIDField" ) ) %>' ID="Hyperlink1">
									<%# DataBinder.Eval(Container.DataItem, "OwnerNameField" ) %>
								</asp:HyperLink>
							</ItemTemplate>
						</asp:TemplateColumn>
						
						<asp:BoundColumn ItemStyle-CssClass="ifolderitem5" DataField="MemberCountField" />
						
						<asp:BoundColumn ItemStyle-CssClass="ifolderitem6" DataField="LastModifiedField" />
						<asp:BoundColumn DataField="OwnerIDField" Visible="False" />
						<asp:BoundColumn DataField="NameField" Visible="False" />
						<asp:BoundColumn DataField="OwnerNameField" Visible="False" />
						<asp:BoundColumn DataField="SizeField" Visible="False" />
						<asp:BoundColumn DataField="FullNameField" Visible="False" />
						<asp:BoundColumn DataField="PreferenceField" Visible="False" />
						
					</Columns>
					
				</asp:datagrid>
				
				<ifolder:ListFooter ID="iFolderListFooter" Runat="server" />
				
				<asp:Button 
					ID="DeleteButton" 
					Runat="server" 
					CssClass="ifolderbuttons" 
					Enabled="False"
					Visible="True"
					OnClick="OnDeleteButton_Click" /> 
					
				<asp:Button 
					ID="DisableButton" 
					Runat="server" 
					CssClass="ifolderbuttons" 
					Enabled="False"
					OnClick="OnDisableButton_Click" />
					
				<asp:Button 
					ID="EnableButton" 
					Runat="server" 
					CssClass="ifolderbuttons" 
					Enabled="False"
					OnClick="OnEnableButton_Click" />
					
<%--				<asp:Button 
					ID="CreateButton" 
					Runat="server" 
					CssClass="ifolderbuttons"
					OnClick="OnCreateButton_Click" /> --%>
						
			</div>
					
		</div>
		
	</div>

<%-- 	<ifolder:Footer id="footer" runat="server" /> --%>
				
</form>

</body>

</html>
