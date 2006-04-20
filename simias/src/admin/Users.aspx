<%@ Page language="c#" Codebehind="Users.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderWeb.Admin.Users" %>
<%@ Register TagPrefix="iFolder" TagName="TopNavigation" Src="TopNavigation.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="MemberSearch" Src="MemberSearch.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="ListFooter" Src="ListFooter.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="Footer" Src="Footer.ascx" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
	
<head>

	<meta http-equiv="Content-Type" content="text/html; charset=iso-8859-1">
	<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">
	
	<title><%= GetString( "TITLE" ) %></title>
	
	<style type="text/css">
		@import url(css/iFolderAdmin.css);
		@import url(css/Users.css);
	</style>
	
</head>

<body id="users" runat="server">

<form runat="server">

	<div class="container">
	
		<iFolder:TopNavigation ID="TopNav" Runat="server" />
	
		<div class="nav">
		
			<div class="pagetitle">
			
				<%= GetString( "IFOLDERUSERS" ) %>
				
			</div>
							
			<iFolder:MemberSearch ID="MemberSearch" Runat="server" />
					
			<div class="accountsnav">
	
				<table class="accountslistheader" cellpadding="0" cellspacing="0" border="0">
			
					<tr>
						<td class="checkboxcolumn">
							<asp:CheckBox 
								ID="AllUsersCheckBox" 
								Runat="server" 
								OnCheckedChanged="OnAllUsersChecked" 
								AutoPostBack="True"	/>
						</td>
					
						<td class="typecolumn">
							<%= GetString( "TYPE" ) %>
						</td>
						
						<td class="usernamecolumn">
							<%= GetString( "USERNAME" ) %>
						</td>
						
						<td class="fullnamecolumn">
							<%= GetString( "FULLNAME" ) %>
						</td>
						
						<td class="statuscolumn">
							<%= GetString( "ENABLED" ) %>
						</td>
					</tr>
			
				</table>
				
				<asp:datagrid 
					ID="Accounts" 
					Runat="server" 
					AutoGenerateColumns="False" 
					CssClass="accountslist"
					CellPadding="0" 
					CellSpacing="0" 
					PageSize="15" 
					GridLines="None" 
					ShowHeader="False"
					AlternatingItemStyle-CssClass="accountslistaltitem"
					ItemStyle-CssClass="accountslistitem">
					
					<Columns>
					
						<asp:BoundColumn DataField="IDField" Visible="False" />
					
						<asp:BoundColumn DataField="DisabledField" Visible="False" />
						
						<asp:TemplateColumn ItemStyle-CssClass="accountsitem1">
							<ItemTemplate>
								<asp:CheckBox 
									ID="AccountsListCheckBox" 
									Runat="server" 
									OnCheckedChanged="OnUserChecked" 
									AutoPostBack="True" 
									Visible='<%# DataBinder.Eval( Container.DataItem, "VisibleField" ) %>' 
									Checked='<%# GetMemberCheckedState( DataBinder.Eval( Container.DataItem, "IDField" ) ) %>'
									Enabled='<%# IsUserEnabled( DataBinder.Eval( Container.DataItem, "IDField" ) ) %>' />
							</ItemTemplate>
						</asp:TemplateColumn>
						
						<asp:TemplateColumn ItemStyle-CssClass="accountsitem2">
							<ItemTemplate>
								<asp:Image 
									ID="UserImage" 
									Runat="server" 
									ImageUrl='<%# GetUserImage( DataBinder.Eval( Container.DataItem, "AdminField" ) ) %>'
									Visible='<%# DataBinder.Eval( Container.DataItem, "VisibleField" ) %>' />
							</ItemTemplate>
						</asp:TemplateColumn>

						<asp:HyperLinkColumn 
							ItemStyle-CssClass="accountsitem3" 
							DataTextField="NameField"
							DataNavigateUrlField="IDField" 
							DataNavigateUrlFormatString="UserDetails.aspx?id={0}"
							Target="_top"/>
							
						<asp:BoundColumn ItemStyle-CssClass="accountsitem4" DataField="FullNameField"/>
						
						<asp:BoundColumn ItemStyle-CssClass="accountsitem5" DataField="StatusField"/>
						
					</Columns>
					
				</asp:datagrid>
						
				<ifolder:ListFooter ID="AccountsFooter" Runat="server"/>
				
				<asp:Button 
					ID="DeleteButton" 
					Runat="server" 
					CssClass="ifolderbuttons" 
					OnClick="OnDeleteButton_Click" 
					Enabled="False"
					Visible="False" />
					
				<asp:Button 
					ID="DisableButton" 
					Runat="server" 
					CssClass="ifolderbuttons" 
					OnClick="OnDisableButton_Click"
					Enabled="False" />
					
				<asp:Button 
					ID="EnableButton" 
					Runat="server" 
					CssClass="ifolderbuttons" 
					OnClick="OnEnableButton_Click"
					Enabled="False" />
					
				<asp:Button 
					ID="CreateButton" 
					Runat="server" 
					CssClass="ifolderbuttons" 
					OnClick="OnCreateButton_Click"
					Visible="False" />

			</div>
					
		</div>

	</div>

	<ifolder:Footer Runat="server" />
				
</form>

</body>

</html>
