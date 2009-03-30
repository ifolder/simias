<%@ Page language="c#" Codebehind="ProvisionUsers.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderWeb.Admin.ProvisionUsers" %>
<%@ Register TagPrefix="iFolder" TagName="TopNavigation" Src="TopNavigation.ascx" %>
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
		@import url(css/MemberSelect.css);
	</style>
	
</head>

<body id="ifolders" runat="server">

<form runat="server">

	<div class="container">
			
		<iFolder:TopNavigation ID="TopNav" Runat="server" />
		
		<div class="leftnav" >
		
			<div class="headertitle">
				
				<asp:Label ID="HeaderTitle" Runat="server" />
				
				<asp:Label ID="SubHeaderTitle" Runat="server" />
				
			</div>
	
			<div class="memberselectnav">
				
				<asp:DropDownList
					ID="SelectServerList"
					Width=400
					OnSelectedIndexChanged="OnSelectServerList_Changed"
					AutoPostBack="True"
					Runat="server"
					CssClass="searchlist" />
				
				<table class="memberlistheader">
					<tr>
						<td>
							<%= GetString( "IFOLDERUSERS" ) %>
						<td>
					<tr>
				</table>
			
				<asp:datagrid 
					id="MemberList" 
					runat="server" 
					AutoGenerateColumns="False" 
					CssClass="memberlist"
					CellPadding="0" 
					CellSpacing="0"
					ShowHeader="False" 
					PageSize="15" 
					GridLines="None" 
					ItemStyle-CssClass="memberlistitem"
					AlternatingItemStyle-CssClass="memberlistaltitem">
				
					<Columns>

						<asp:BoundColumn DataField="IDField" Visible="False" />
						
						<asp:TemplateColumn ItemStyle-CssClass="memberitem2">
							<ItemTemplate>
								<asp:Image 
									ID="UserImage" 
									Runat="server" 
									ImageUrl='<%# GetUserImage( DataBinder.Eval( Container.DataItem , "ProvisionedField") ) %>'
									Visible='<%# ( bool )DataBinder.Eval( Container.DataItem, "VisibleField" ) %>'/>
							</ItemTemplate>
						</asp:TemplateColumn>
					
						<asp:BoundColumn DataField="NameField" />
					
						<asp:BoundColumn DataField="FullNameField" />
					
					</Columns>
				
				</asp:datagrid>
			
				<ifolder:ListFooter ID="MemberListFooter" Runat="server" />

				<div align="center" class="okcancelnav">
				
					<asp:Button 
						ID="OkButton" 
						Runat="server" 
						CssClass="provision_reprovision_buttons" 
						OnClick="OkButton_Clicked" />
					
					<asp:Button 
						ID="CancelButton" 
						Runat="server" 
						CssClass="ifolderbuttons" 
						OnClick="CancelButton_Clicked" />
					
				</div>

			</div>
			
		</div>						
		
	</div>

	
</form>
		
</body>
	
</html>
