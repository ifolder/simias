<%@ Page language="c#" Codebehind="SystemInfo.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderWeb.Admin.SystemInfo" %>
<%@ Register TagPrefix="iFolder" TagName="TopNavigation" Src="TopNavigation.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="ListFooter" Src="ListFooter.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="Policy" Src="Policy.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="Footer" Src="Footer.ascx" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
	
<head>
	
	<meta http-equiv="Content-Type" content="text/html; charset=iso-8859-1">
	<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">
	
	<title><%= GetString( "TITLE" ) %></title>
	
	<style type="text/css">
		@import url(css/iFolderAdmin.css);
		@import url(css/SystemInfo.css);
	</style>
	
	<script language="javascript">

		function EnableSystemButtons()
		{
			document.getElementById( "SaveButton" ).disabled = false;
			document.getElementById( "CancelButton" ).disabled = false;
		}
		function alertuser()
		{
			if (document.getElementById( "Policy_SecurityState_encryption").checked == true && document.getElementById( "Policy_SecurityState_encryption").disabled == false)
			return confirm("<%= GetString("CONFIRMENCRYPTION") %>");
		}

	</script>
	
</head>

<body id="system" runat="server">
	
<form runat="server">
			
	<div class="container">
			
		<iFolder:TopNavigation ID="TopNav" Runat="server" />

		<div class="leftnav">
		
			<div class="detailnav">
			
				<div class="pagetitle">
				
					<%= GetString( "SYSTEMSETTINGS" ) %>
					
				</div>
				
				<table class="detailinfo">
				
					<tr>
						<th>
							<%= GetString( "NAMETAG" ) %>
						</th>
						
						<td>
							<asp:TextBox 
								ID="Name" 
								Runat="server" 
								CssClass="edittext"
								onkeypress="EnableSystemButtons()" />
						</td>
					</tr>
					
					<tr>
						<th>
							<%= GetString( "DESCRIPTIONTAG" ) %>
						</th>
						
						<td>
							<textarea 
								id="Description" 
								runat="server" 
								class="edittext" 
								Rows="2" 
								wrap="soft"
								onkeypress="EnableSystemButtons()"></textarea>
						</td>
					</tr>
				
					<tr>
						<th>
							<%= GetString( "SSLTAG" ) %>
						</th>

                                                <td>
							<asp:DropDownList ID="SSLValue" runat="server" 
								AutoPostBack="True"
								OnSelectedIndexChanged="EnableSaveButtons" />
                                                </td>
                                        </tr>

					<tr>
						<th>
							<%= GetString( "TOTALUSERSTAG" ) %>
						</th>
						
						<td>
							<asp:Literal ID="NumberOfUsers" Runat="server" />
						</td>
					</tr>
					
					<tr>
						<th>
							<%= GetString( "TOTALIFOLDERSTAG" ) %>
						</th>
						
						<td>
							<asp:Literal ID="NumberOfiFolders" Runat="server" />
						</td>
					</tr>

					<tr>
						<th>
							<%= GetString( "FULLNAMEDISPLAYORDER" ) %>
						</th>
						
						<td>
							<asp:RadioButtonList
								ID="FullNameSetting"	
								Runat="server"
								AutoPostBack="True"
								RepeatDirection="Horizontal"
								OnSelectedIndexChanged="EnableSaveButtons"> 
								<asp:ListItem></asp:ListItem>
								<asp:ListItem></asp:ListItem>
							</asp:RadioButtonList>
						</td>
					</tr>
					
				</table>

				<asp:Button 
					ID="CancelButton" 
					Runat="server" 
					CssClass="ifolderbuttons"
					Enabled="False"
					OnClick="OnCancelButton_Click" />
					
				<asp:Button 
					ID="SaveButton" 
					Runat="server" 
					CssClass="ifoldersavebutton"
					Enabled="False"
					OnClick="OnSaveButton_Click" />

				<asp:Button 
					ID="ReprovisionStatusButton" 
					Runat="server" 
					CssClass="reprovisionbuttons"
					Enabled="true"
					OnClick="OnReprovisionStatusButton_Click" />
					
			</div>
			
			<div class="adminlistnav">
		
				<div class="pagetitle">
				
					<%= GetString( "IFOLDERADMINS" ) %>
					
				</div>
				
				<table class="adminlistheader" cellpadding="0" cellspacing="0" border="0">
			
					<tr>
						<td class="checkboxcolumn">
							<asp:CheckBox 
								ID="AllAdminsCheckBox" 
								Runat="server" 
								OnCheckedChanged="OnAllAdminsChecked" 
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
					</tr>
			
				</table>
				
				<asp:datagrid 
					id="AdminList" 
					runat="server" 
					AutoGenerateColumns="False" 
					PageSize="9" 
					CellPadding="0"
					CellSpacing="0" 
					GridLines="None" 
					ShowHeader="False" 
					CssClass="adminlist" 
					AlternatingItemStyle-CssClass="adminlistaltitem" 
					ItemStyle-CssClass="adminlistitem">
					
					<Columns>
					
						<asp:BoundColumn DataField="IDField" Visible="False" />
						
						<asp:TemplateColumn ItemStyle-CssClass="adminitem1">
							<ItemTemplate>
								<asp:CheckBox 
									ID="AdminListCheckBox" 
									Runat="server" 
									OnCheckedChanged="OnAdminChecked" 
									AutoPostBack="True" 
									Visible='<%# DataBinder.Eval( Container.DataItem, "VisibleField" ) %>' 
									Checked='<%# GetAdminCheckedState( DataBinder.Eval( Container.DataItem, "IDField" ) ) %>'
									Enabled='<%# GetAdminEnabledState( DataBinder.Eval( Container.DataItem, "IDField" ) ) %>' />
							</ItemTemplate>
						</asp:TemplateColumn>
						
						<asp:TemplateColumn ItemStyle-CssClass="adminitem2">
							<ItemTemplate>
								<asp:Image 
									ID="AdminListImage" 
									Runat="server" 
									ImageUrl="images/ifolder_admin.gif"
									Visible='<%# DataBinder.Eval( Container.DataItem, "VisibleField" ) %>'/>
							</ItemTemplate>
						</asp:TemplateColumn>
						
						<asp:HyperLinkColumn 
							ItemStyle-CssClass="adminitem3" 
							DataTextField="NameField" 
							DataNavigateUrlField="IDField"
							DataNavigateUrlFormatString="UserDetails.aspx?id={0}" 
							Target="_top" />
							
						<asp:BoundColumn ItemStyle-CssClass="adminitem4" DataField="FullNameField" />
						
					</Columns>
					
				</asp:datagrid>
				
				<ifolder:ListFooter ID="AdminListFooter" Runat="server" />
				
				<asp:Button 
					ID="DeleteButton" 
					Runat="server" 
					CssClass="deleteadminbutton" 
					Enabled="False"
					OnClick="OnDeleteButton_Click" />
				
				<asp:Button 
					ID="AddButton" 
					Runat="server" 
					CssClass="addadminbutton"
					OnClick="OnAddButton_Click" />
					
			</div>
			
		</div>
		
		<ifolder:Policy ID="Policy" Runat="server" />		
		
	</div>
		
<!-- 	<ifolder:Footer id="footer" runat="server" /> -->
				
</form>

</body>

</html>
