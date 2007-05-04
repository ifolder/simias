<%@ Page language="c#" Codebehind="OwnerSelect.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderWeb.Admin.OwnerSelect" %>
<%@ Register TagPrefix="iFolder" TagName="MemberSearch" Src="MemberSearch.ascx" %>
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
		@import url(css/OwnerSelect.css);
	</style>
	
</head>

<body id="ifolders" runat="server">

<form runat="server" ID="Form1">

	<div class="container">
			
		<iFolder:TopNavigation ID="TopNav" Runat="server" />
		
		<div class="leftnav" >
		
			<div class="headertitle">
				
				<asp:Label ID="HeaderTitle" Runat="server" />
				
			</div>
	
			<iFolder:MemberSearch ID="MemberSearch" Runat="server" />
			
			<div class="memberselectnav">

				<table class="memberlistheader">
			
					<tr>
						<td>
							<%= GetString( "IFOLDERUSERS" ) %>
						</td>
					</tr>
			
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
					AlternatingItemStyle-CssClass="memberlistaltitem"
					SelectedItemStyle-CssClass="memberselecteditem">
				
					<Columns>
					
						<asp:BoundColumn DataField="IDField" Visible="False" />
						
						<asp:TemplateColumn ItemStyle-CssClass="memberitem1">
							<ItemTemplate>
								<asp:Image 
									ID="MemberUserImage" 
									Runat="server" 
									ImageUrl="images/ifolder_user.gif" 
									Visible='<%# ( bool )DataBinder.Eval( Container.DataItem, "VisibleField" ) %>'/>
							</ItemTemplate>
						</asp:TemplateColumn>
					
						<asp:TemplateColumn ItemStyle-CssClass="memberitem2">
							<ItemTemplate>
								<asp:LinkButton 
									ID="MemberName" 
									Runat="server" 
									OnClick="OnMemberName_Click"
									Text='<%# DataBinder.Eval( Container.DataItem, "NameField" ) %>'
									Visible='<%# ( bool )DataBinder.Eval( Container.DataItem, "VisibleField" ) %>'/>
							</ItemTemplate>
						</asp:TemplateColumn>
						
						<asp:BoundColumn DataField="FullNameField" ItemStyle-CssClass="memberitem3" />
					
					</Columns>
				
				</asp:datagrid>
			
				<ifolder:ListFooter ID="MemberListFooter" Runat="server" />

				<div align="center" class="okcancelnav">
				
					<asp:Button
						ID="BackButton"
						Runat="server"
						CssClass="ifolderbuttons"
						OnClick="BackButton_Clicked" />					
				
					<asp:Button 
						ID="NextButton" 
						Runat="server" 
						CssClass="ifolderbuttons" 
						Enabled="False" 
						OnClick="NextButton_Clicked" />
					
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
