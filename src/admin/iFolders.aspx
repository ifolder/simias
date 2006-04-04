<%@ Page language="c#" Codebehind="iFolders.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderWeb.Admin.iFolders" %>
<%@ Register TagPrefix="iFolder" TagName="PageFooter" Src="PageFooter.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="iFolderSearch" Src="iFolderSearch.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="TopNavigation" Src="TopNavigation.ascx" %>

<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
	
<head>

	<meta http-equiv="Content-Type" content="text/html; charset=iso-8859-1">
	<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">

	<title><%= GetString( "TITLE" ) %></title>
		
	<style type="text/css">
		@import url(iFolderAdmin.css);
		@import url(iFolders.css);
	</style>
		
</head>

<body id="ifolders" runat="server">

<form runat="server">

	<div class="container">
			
		<iFolder:TopNavigation ID="TopNav" Runat="server" />

		<div class="nav">
		
			<h3><%= GetString( "IFOLDERS" ) %></h3>
				
			<iFolder:iFolderSearch ID="iFolderSearch" Runat="server" />
					
			<div class="ifoldersnav">
	
				<asp:datagrid 
					id="iFolderList" 
					runat="server" 
					AutoGenerateColumns="False" 
					PageSize="11" 
					CellPadding="0"
					CellSpacing="0" 
					GridLines="None" 
					ShowHeader="True" 
					CssClass="ifolderlist" 
					HeaderStyle-CssClass="ifolderlistheader"
					AlternatingItemStyle-CssClass="ifolderlistaltitem" 
					ItemStyle-CssClass="ifolderlistitem">
					
					<Columns>
					
						<asp:BoundColumn DataField="IDField" Visible="False" />
						
						<asp:TemplateColumn ItemStyle-CssClass="ifolderitem1">
							<HeaderTemplate>
								<asp:CheckBox 
									ID="iFolderAllCheckBox" 
									Runat="server" 
									AutoPostBack="True" 
									OnCheckedChanged="OnAlliFoldersChecked" />
							</HeaderTemplate>
							
							<ItemTemplate>
								<asp:CheckBox 
									ID="iFolderListCheckBox" 
									Runat="server" 
									OnCheckedChanged="OniFolderChecked" 
									AutoPostBack="True" 
									Visible='<%# DataBinder.Eval( Container.DataItem, "VisibleField" ) %>' 
									Checked='<%# GetMemberCheckedState( DataBinder.Eval( Container.DataItem, "IDField" ) ) %>' />
							</ItemTemplate>
						</asp:TemplateColumn>
						
						<asp:TemplateColumn ItemStyle-CssClass="ifolderitem2">
							<ItemTemplate>
								<asp:Image 
									ID="iFolderListImage" 
									Runat="server" 
									ImageUrl='<%# GetiFolderImage( DataBinder.Eval( Container.DataItem, "DisabledField" ), DataBinder.Eval( Container.DataItem, "SharedField" ) ) %>' 
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
						
						<asp:BoundColumn ItemStyle-CssClass="ifolderitem5" DataField="SizeField" />
						
					</Columns>
					
				</asp:datagrid>
				
				<ifolder:PageFooter ID="iFolderListFooter" Runat="server" />
						
			</div>
					
		</div>
		
		<div class="footer">
		</div>
				
	</div>

</form>

</body>

</html>
