<%@ Page language="c#" Codebehind="Users.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderWeb.Admin.Users" %>
<%@ Register TagPrefix="iFolder" TagName="TopNavigation" Src="TopNavigation.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="MemberSearch" Src="MemberSearch.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="PageFooter" Src="PageFooter.ascx" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
	
<head>

	<meta http-equiv="Content-Type" content="text/html; charset=iso-8859-1">
	<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">
	
	<title><%= GetString( "TITLE" ) %></title>
	
	<style type="text/css">
		@import url(iFolderAdmin.css);
		@import url(Users.css);
	</style>
	
</head>

<body id="users" runat="server">

<form runat="server">

	<div class="container">
	
		<iFolder:TopNavigation ID="TopNav" Runat="server" />
	
		<div class="nav">
		
			<h3><%= GetString( "IFOLDERUSERS" ) %></h3>
				
			<iFolder:MemberSearch ID="MemberSearch" Runat="server" />
					
			<div class="accountsnav">
	
				<asp:datagrid 
					ID="Accounts" 
					Runat="server" 
					AutoGenerateColumns="False" 
					CssClass="accounts"
					CellPadding="0" 
					CellSpacing="0" 
					PageSize="15" 
					GridLines="None" 
					ShowHeader="True"
					HeaderStyle-CssClass="accountsheader" 
					AlternatingItemStyle-CssClass="accountslistaltitem"
					ItemStyle-CssClass="accountslistitem">
					
					<Columns>
					
						<asp:TemplateColumn ItemStyle-CssClass="accountsitem1">
							<ItemTemplate>
								<asp:Image 
									ID="UserImage" 
									Runat="server" 
									ImageUrl='<%# GetUserImage( DataBinder.Eval( Container.DataItem, "AdminField" ) ) %>'
									Visible='<%# DataBinder.Eval( Container.DataItem, "VisibleField" ) %>' />
							</ItemTemplate>
						</asp:TemplateColumn>

						<asp:HyperLinkColumn 
							ItemStyle-CssClass="accountsitem2" 
							DataTextField="NameField"
							DataNavigateUrlField="IDField" 
							DataNavigateUrlFormatString="UserDetails.aspx?id={0}"
							Target="_top"/>
							
						<asp:BoundColumn ItemStyle-CssClass="accountsitem3" DataField="FullNameField"/>
						
						<asp:BoundColumn ItemStyle-CssClass="accountsitem4" DataField="StatusField"/>
						
					</Columns>
					
				</asp:datagrid>
						
				<ifolder:PageFooter ID="AccountsFooter" Runat="server"/>
				
				<asp:Button 
					ID="CreateButton" 
					Runat="server" 
					CssClass="ifolderbuttons" 
					OnClick="OnCreateButton_Click"
					Visible="False" />

			</div>
					
		</div>

		<div class="footer">
		</div>
				
	</div>

</form>

</body>

</html>
