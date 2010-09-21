<%@ Page language="c#" Codebehind="userMove.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderWeb.Admin.UserMove" %>
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

<body id="system" runat="server">

<form runat="server">

	<div class="container">
	
		<iFolder:TopNavigation ID="TopNav" Runat="server" />
	
		<div class="nav">
		
			<div class="pagetitle">
			
				<%= GetString( "REPROVISIONLIST" ) %>
				
			</div>
                                <asp:Button
                                        ID="RefreshButton"
                                        Runat="server"
                                        CssClass="refreshbuttons"
                                        Enabled="true"
                                        OnClick="OnRefreshButton_Click" />
					
			<div class="accountsnav">
	
				<table class="accountslistheader" cellpadding="0" cellspacing="0" border="0">
			
					<tr>
						<td class="typecolumn">
							<%= GetString( "TYPE" ) %>
						</td> 
						
						<td class="regularcolumn">
							<%= GetString( "USERNAME" ) %>
						</td>
						
						<td class="regularcolumn">
							<%= GetString( "CURRENTHOME" ) %>
						</td>
						<td class="regularcolumn">
							<%= GetString( "NEWHOME" ) %>
						</td>
						<td class="regularcolumn">
						    <%= GetString( "COMPLETED" ) %>
						</td>
						
						<td class="regularcolumn">
							<%= GetString( "REPROVISOINSTATE" ) %>
						</td>

						<td class="regularcolumn">
							 &nbsp;  
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
					
						<asp:TemplateColumn ItemStyle-CssClass="accountsitem2">
							<ItemTemplate>
								<asp:Image 
									ID="UserImage" 
									Runat="server"
									ImageUrl='<%# GetUserImage( DataBinder.Eval( Container.DataItem, "AdminField" ), DataBinder.Eval( Container.DataItem , "ProvisionedField") ) %>' 
									Visible='<%# DataBinder.Eval( Container.DataItem, "VisibleField" ) %>' />
							</ItemTemplate>
						</asp:TemplateColumn>   

						<asp:HyperLinkColumn 
							ItemStyle-CssClass="accountsitem3leftoff" 
							DataTextField="NameField"
							DataNavigateUrlField="IDField" 
							DataNavigateUrlFormatString="userMoveDetails.aspx?id={0}"
							Target="_top"/>
						<asp:BoundColumn ItemStyle-CssClass="accountsitem3" DataField="CurrentHomeField"/>
						<asp:BoundColumn ItemStyle-CssClass="accountsitem3" DataField="NewHomeField"/>
						<asp:BoundColumn ItemStyle-CssClass="accountsitem3" DataField="PercentageStatusField"/>
						<asp:BoundColumn ItemStyle-CssClass="accountsitem3" DataField="StatusField"/>
						<asp:TemplateColumn ItemStyle-CssClass="accountsitem3">
							<ItemTemplate>
								<asp:LinkButton Text='<%# GetString( "DELETE" ) %>' id="LinkButton1" OnClick="OnDeleteClicked" Visible='<%# DataBinder.Eval(Container.DataItem,"DeleteField") %>' runat="server" />
							</ItemTemplate>
						</asp:TemplateColumn>		
					</Columns>
					
				</asp:datagrid>
						
				<ifolder:ListFooter ID="AccountsFooter" Runat="server"/>
				
		<%--		<asp:Button 
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
					ID="ProvisionButton" 
					Runat="server" 
					CssClass="provisionbuttons" 
					OnClick="OnProvisionButton_Click"
					Enabled="False" />
					
				<asp:Button 
					ID="SaveButton" 
					Runat="server" 
					CssClass="provisionbuttons" 
					OnClick="OnSaveButton_Click"
					Enabled="False" />

				<asp:Button 
					ID="CreateButton" 
					Runat="server" 
					CssClass="ifolderbuttons" 
					OnClick="OnCreateButton_Click"
					Visible="False" />  --%>

			</div>
					
		</div>

	</div>
 <%-- moved to new location in header 
	<ifolder:Footer Runat="server" />  --%>
				
</form>

</body>

</html>
