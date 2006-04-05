<%@ Page language="c#" Codebehind="MemberSelect.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderWeb.Admin.MemberSelect" %>
<%@ Register TagPrefix="iFolder" TagName="MemberSearch" Src="MemberSearch.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="TopNavigation" Src="TopNavigation.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="PageFooter" Src="PageFooter.ascx" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>

<head>

	<meta http-equiv="Content-Type" content="text/html; charset=iso-8859-1">
	<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">
	
	<title><%= GetString( "TITLE" ) %></title>
	
	<style type="text/css">
		@import url(iFolderAdmin.css);
		@import url(MemberSelect.css);
	</style>
	
</head>

<body id="users" runat="server">

<form runat="server">

	<div class="container">
			
		<iFolder:TopNavigation ID="TopNav" Runat="server" />
				
		<div id="CreateiFolderDiv" runat="server">
	
			<h3><%= GetString( "CREATENEWIFOLDER" ) %></h3>
		
			<table>
		
				<tr>
					<td>
						<asp:Label ID="NameLabel" Runat="server" />
					</td>
				
					<td class="tablecolumn">
						<asp:TextBox 
							ID="Name" 
							Runat="server" 
							CssClass="edittext"
							AutoPostBack="True"
							OnTextChanged="OnNameChanged" />
					</td>
				</tr>
			
				<tr>
					<td>
						<asp:Label ID="DescriptionLabel" Runat="server" />
					</td>
				
					<td class="tablecolumn">
						<textarea 
							id="Description" 
							runat="server" 
							class="edittext" 
							rows="3" 
							wrap="soft">
						</textarea>
					</td>
				</tr>
			
			</table>
		
		</div>
				
		<h3 class="headertitle"><%= GetString( "SELECTUSERS" ) %></h3>
	
		<iFolder:MemberSearch ID="MemberSearch" Runat="server" />
	
		<div class="memberselectnav">
			<table class="members">
				<tr>
					<td>
						<!-- available member list -->
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
					
						<ifolder:PageFooter ID="MemberListFooter" Runat="server" />
					</td>
				
					<td class="memberbuttonnav">
						<asp:ImageButton 
							ID="AddButton" 
							Runat="server" 
							Visible="False" 
							ImageUrl="images/go-next-48x48.png"
							OnClick="AddMembers" />
						
						<asp:Image 
							ID="AddDisabledButton" 
							Runat="server" 
							Visible="True" 
							ImageUrl="images/go-next-gray-48x48.png" />
						
						<asp:ImageButton 
							ID="RemoveButton" 
							Runat="server" 
							Visible="False" 
							ImageUrl="images/go-previous-48x48.png"
							OnClick="RemoveMembers" />
						
						<asp:Image 
							ID="RemoveDisabledButton" 
							Runat="server" 
							Visible="True" 
							ImageUrl="images/go-previous-gray-48x48.png" />
					</td>
				
					<td>
						<!-- selected member list -->
						<table class="memberlistheader">
					
							<tr>
								<td class="checkboxcolumn">									
									<asp:CheckBox 
										ID="AllSelectedMembersCheckBox" 
										Runat="server" 
										OnCheckedChanged="AllSelectedMembersChecked" 
										AutoPostBack="True" />
								</td>
							
								<td>
									<%= GetString( "SELECTEDUSERS" ) %>
								</td>
							</tr>
					
						</table>
					
						<asp:datagrid 
							id="SelectedMemberList" 
							runat="server" 
							AutoGenerateColumns="False" 
							ShowHeader="False"
							CssClass="memberlist" 
							CellPadding="0" 
							CellSpacing="0" 
							PageSize="15" 
							GridLines="None" 
							ItemStyle-CssClass="memberlistitem"
							AlternatingItemStyle-CssClass="memberlistaltitem">
						
							<Columns>
						
								<asp:BoundColumn DataField="IDField" Visible="False" />
							
								<asp:TemplateColumn ItemStyle-CssClass="memberitem1">
									<ItemTemplate>
										<asp:CheckBox 
											ID="SelectedMemberItemCheckBox" 
											Runat="server" 
											OnCheckedChanged="SelectedMemberChecked" 
											AutoPostBack="true" 
											Visible='<%# DataBinder.Eval( Container.DataItem, "VisibleField" ) %>' 
											Enabled='<%# ( bool )DataBinder.Eval( Container.DataItem, "EnabledField" ) %>' 
											Checked='<%# GetSelectedMemberCheckedState( DataBinder.Eval( Container.DataItem, "IDField" ) ) %>'/>
									</ItemTemplate>
								</asp:TemplateColumn>
								
								<asp:TemplateColumn ItemStyle-CssClass="memberitem2">
									<ItemTemplate>
										<asp:Image 
											ID="SelectedMemberUserImage" 
											Runat="server" 
											Visible='<%# DataBinder.Eval( Container.DataItem, "VisibleField" ) %>' 
											ImageUrl="images/ifolder_user.gif"/>
									</ItemTemplate>
								</asp:TemplateColumn>
								
								<asp:BoundColumn DataField="NameField" ItemStyle-CssClass="memberitem3" />
									
								<asp:BoundColumn DataField="FullNameField" ItemStyle-CssClass="memberitem4" />
									
							</Columns>
							
						</asp:datagrid>
						
						<ifolder:PageFooter ID="SelectedMemberListFooter" Runat="server" />
					</td>
				</tr>
				
			</table>
			
		</div>
		
		<div class="okcancelnav">
		
			<asp:Button ID="OkButton" Runat="server" CssClass="ifolderbuttons" Enabled="False" OnClick="OkButton_Clicked" />
			
			<asp:Button ID="CancelButton" Runat="server" CssClass="ifolderbuttons" OnClick="CancelButton_Clicked" />
			
		</div>
		
		<div class="footer">
		</div>
		
	</div>
			
</form>
		
</body>
	
</html>