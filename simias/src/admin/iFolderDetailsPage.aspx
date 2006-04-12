<%@ Register TagPrefix="iFolder" TagName="Policy" Src="Policy.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="PageFooter" Src="PageFooter.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="TopNavigation" Src="TopNavigation.ascx" %>
<%@ Page language="c#" Codebehind="iFolderDetailsPage.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderWeb.Admin.iFolderDetailsPage" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>

<head>

	<meta http-equiv="Content-Type" content="text/html; charset=iso-8859-1">
	<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">

	<title><%= GetString( "TITLE" ) %></title>
		
	<style type="text/css">
		@import url(iFolderAdmin.css);
		@import url(iFolderDetailsPage.css);
	</style>
	
</head>

<body id="ifolders">

<form runat="server">

	<div class="container">
	
		<iFolder:TopNavigation ID="TopNav" Runat="server" />
		
		<div class="leftnav">
		
			<div class="detailnav">
			
				<h3><%= GetString( "IFOLDERDETAILS" ) %></h3>
				
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
							<%= GetString( "DESCRIPTIONTAG" ) %>
						</th>
						
						<td>
							<asp:TextBox 
								ID="Description" 
								Runat="server" 
								CssClass="edittext" 
								AutoPostBack="true" 
								OnTextChanged="DescriptionChanged" />
								
							<asp:Button 
								ID="DescriptionButton" 
								Runat="server" 
								CssClass="ifolderbuttons"
								Enabled="False"
								OnClick="SaveDescription" />
						</td>
					</tr>

					<tr>
						<th>
							<%= GetString( "OWNERTAG" ) %>
						</th>

						<td>
							<asp:HyperLink 
								ID="Owner" 
								Runat="server" 
								Target="_top" />
						</td>
					</tr>
					
					<tr>
						<th>
							<%= GetString( "PATHTAG" ) %>
						</th>
						
						<td>
							<%= GetiFolderPath() %>
						</td>
					</tr>

					<tr>
						<th>
							<%= GetString( "SIZETAG" ) %>
						</th>

						<td>
							<asp:Literal ID="Size" Runat="server" />
						</td>
					</tr>

					<tr>
						<th>
							<%= GetString( "SHAREDTAG" ) %>
						</th>

						<td>
							<asp:Literal ID="Shared" Runat="server" />
						</td>
					</tr>
					
				</table>

			</div>

			<div class="ifoldermemberlistnav">

				<h3><%= GetString( "MEMBERS" ) %></h3>
	
				<asp:DataGrid 
					ID="iFolderMemberList" 
					Runat="server" 
					CellPadding="0" 
					CellSpacing="0" 
					GridLines="None"
					AutoGenerateColumns="False" 
					PageSize="11" 
					ShowHeader="True" 
					CssClass="ifoldermemberlist" 
					HeaderStyle-CssClass="ifoldermemberlistheader"
					AlternatingItemStyle-CssClass="ifoldermemberlistaltitem" 
					ItemStyle-CssClass="ifoldermemberlistitem">
					
					<Columns>
			
						<asp:BoundColumn DataField="IDField" Visible="False" />
						
						<asp:BoundColumn DataField="OwnerField" Visible="False" />
						
						<asp:TemplateColumn ItemStyle-CssClass="ifoldermembersitem1">
							<HeaderTemplate>
								<asp:CheckBox 
									ID="MemberAllCheckBox" 
									Runat="server" 
									AutoPostBack="True" 
									OnCheckedChanged="AllMembersChecked" />
							</HeaderTemplate>
							
							<ItemTemplate>
								<asp:CheckBox 
									ID="iFolderMemberListCheckBox" 
									Runat="server" 
									AutoPostBack="True" 
									OnCheckedChanged="MemberChecked" 
									Visible='<%# DataBinder.Eval( Container.DataItem, "VisibleField" )%>' 
									Checked='<%# GetMemberCheckedState( DataBinder.Eval( Container.DataItem, "IDField" ) ) %>' />
							</ItemTemplate>
						</asp:TemplateColumn>
						
						<asp:TemplateColumn ItemStyle-CssClass="ifoldermembersitem2">
							<ItemTemplate>
								<asp:Image 
									ID="UserImage" 
									Runat="server" 
									ImageUrl='<%# GetUserImage( DataBinder.Eval( Container.DataItem, "OwnerField" ) ) %>'/>
							</ItemTemplate>
						</asp:TemplateColumn>
						
						<asp:BoundColumn ItemStyle-CssClass="ifoldermembersitem3" DataField="FullNameField" />
						
						<asp:BoundColumn ItemStyle-CssClass="ifoldermembersitem4" DataField="RightsField" />
						
					</Columns>
					
				</asp:DataGrid>

				<ifolder:PageFooter ID="iFolderMemberListFooter" Runat="server" />

				<asp:DropDownList 
					ID="MemberRightsList" 
					Runat="server"
					CssClass="rightslist"
					Enabled="False">
			
					<asp:ListItem></asp:ListItem>
					<asp:ListItem></asp:ListItem>
					<asp:ListItem></asp:ListItem>
					
				</asp:DropDownList>
				
				<asp:Button 
					ID="MemberRightsButton" 
					Runat="server" 
					CssClass="actionbuttons" 
					OnClick="ChangeMemberRights"
					Enabled="False" />
					
				<asp:Button 
					ID="MemberDeleteButton" 
					Runat="server" 
					CssClass="actionbuttons" 
					OnClick="DeleteiFolderMembers"
					Enabled="False" />
					
				<asp:Button 
					ID="MemberOwnerButton" 
					Runat="server" 
					CssClass="actionbuttons" 
					OnClick="ChangeOwner"
					Enabled="False" />
					
				<asp:Button 
					ID="MemberAddButton" 
					Runat="server" 
					CssClass="actionbuttons" 
					OnClick="AddiFolderMembers" />
					
			</div>

		</div>

		<ifolder:Policy ID="Policy" Runat="server" />

		<div class="footer">
		</div>

	</div>

</form>

</body>

</html>
