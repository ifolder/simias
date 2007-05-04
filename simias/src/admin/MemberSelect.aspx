<%@ Page language="c#" Codebehind="MemberSelect.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderWeb.Admin.MemberSelect" %>
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
	
			<iFolder:MemberSearch ID="MemberSearch" Runat="server" />
			
			<div class="memberselectnav">

				<table class="memberlistheader">
			
					<tr>
						<td class="checkboxcolumn">									
							<asp:CheckBox 
								ID="AllMembersCheckBox" 
								Runat="server" 
								OnCheckedChanged="AllMembersChecked" 
								AutoPostBack="True" />
						</td>
					
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
					AlternatingItemStyle-CssClass="memberlistaltitem">
				
					<Columns>

						<asp:BoundColumn DataField="IDField" Visible="False" />
						
						<asp:TemplateColumn ItemStyle-CssClass="memberitem1">
							<ItemTemplate>
								<asp:CheckBox 
									ID="MemberItemCheckBox" 
									Runat="server" 
									OnCheckedChanged="MemberChecked" 
									AutoPostBack="true" 
									Visible='<%# ( bool )DataBinder.Eval( Container.DataItem, "VisibleField" ) %>' 
									Enabled='<%# ( bool )DataBinder.Eval( Container.DataItem, "EnabledField" ) %>' 
									Checked='<%# GetMemberCheckedState( DataBinder.Eval( Container.DataItem, "IDField" ) ) %>'/>
							</ItemTemplate>
						</asp:TemplateColumn>
					
						<asp:TemplateColumn ItemStyle-CssClass="memberitem2">
							<ItemTemplate>
								<asp:Image 
									ID="MemberUserImage" 
									Runat="server" 
									ImageUrl="images/ifolder_user.gif" 
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
						ID="BackButton"
						Runat="server"
						CssClass="ifolderbuttons"
						OnClick="BackButton_Clicked" />
				
					<asp:Button 
						ID="OkButton" 
						Runat="server" 
						CssClass="ifolderbuttons" 
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
