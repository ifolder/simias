<%@ Page Language="C#" Codebehind="Share.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderApp.Web.SharePage" %>
<%@ Register TagPrefix="iFolder" TagName="Header" Src="Header.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="Message" Src="Message.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="Pagging" Src="Pagging.ascx" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01//EN" "http://www.w3.org/TR/html4/strict.dtd">
<html>

<head>
	<meta http-equiv="Content-Type" content="text/html; charset=iso-8859-1">
	
	<title><%= GetString("TITLE") %></title>
	
	<link rel="SHORTCUT ICON" href="images/ifolder.ico">

	<style type="text/css">
		@import url(css/ifolder.css);
	</style>

	<script type="text/javascript">

		function SubmitKeyDown(e, b)
		{
			var result = true;
			
			if ((e.which && e.which == 13) || (e.keyCode && e.keyCode == 13))
			{
				document.getElementById(b).click();
				result = false;
			} 
			
			return result;
		}
	
		function SetFocus()
		{
			var name = document.getElementById("NewiFolderName");
			
			if (name && (name.value.length == 0))
			{
				name.focus();
			}
			else
			{
				document.getElementById("SearchPattern").select();
			}
		}
		
		// on load
		window.onload = SetFocus;
	
	</script>

</head>

<body>

<form runat="server">

	<div id="container">
	
		<iFolder:Header runat="server" />
	
		<div id="context">
			<asp:HyperLink ID="HomeButton" NavigateUrl="iFolders.aspx" runat="server" />
			/
			<span id="ShareContext" runat="server">
				<asp:HyperLink ID="iFolderButton" runat="server" />
				/
				<%= GetString("SHARE") %>
			</span>
			<span id="CreateContext" runat="server">
				<%= GetString("IFOLDER") %>
			</span>
		</div>
		
		<div id="main">
		
			<iFolder:Message id="MessageBox" runat="server" />
	
			<div id="CreateSection" class="section" runat="server">

				<div class="title"><%= GetString("NEWIFOLDER") %></div>
				
				<div class="content">
					
					<table class="columns">
						<tr>
							<td>
								<%= GetString("NAME") %>
							</td>
							<td>				
								<%= GetString("DESCRIPTION") %>
							</td>
						</tr>
						<tr>
							<td>
								<asp:TextBox ID="NewiFolderName" onkeydown="return SubmitKeyDown(event, 'CreateButton');" runat="server" />
							</td>
							<td rowspan="4">				
								<asp:TextBox ID="NewiFolderDescription" TextMode="MultiLine" Rows="4" onkeydown="return SubmitKeyDown(event, 'CreateButton');" runat="server" />
							</td>
						</tr>
						<tr>
							<td><%= GetString("OWNER") %></td>
						</tr>
						<tr>
							<td class="highlight"><%= Session["UserFullName"] %></td>
						</tr>
					</table>
				
				</div>
							
			</div>
			
			<div id="share" class="section">
				
				<div class="title"><%= GetString("SHARE") %></div>
					
				<div class="content">
					
					<div class="search">
						<asp:DropDownList ID="SearchPropertyList" runat="server" />
						<asp:TextBox ID="SearchPattern" CssClass="searchPattern" runat="server" onkeydown="return SubmitKeyDown(event, 'SearchButton');" />
						<asp:Button ID="SearchButton" CssClass="hide" runat="server" />
					</div>
					
					<table class="columns"><tr><td>
					
						<div id="share-users">
				
							<div class="sub-title"><%= GetString("AVAILABLEUSERS") %></div>
							
							<asp:DataGrid
								ID="UserData"
								GridLines="none"
								AutoGenerateColumns="false"
								ShowHeader="false"
								CssClass="entries"
								runat="server">
								
								<columns>
									<asp:TemplateColumn ItemStyle-CssClass="icon">
										<itemtemplate>
											<asp:LinkButton CommandName="Add" CommandArgument='<%# DataBinder.Eval(Container.DataItem, "UserID") %>' Visible='<%# (bool)DataBinder.Eval(Container.DataItem, "Enabled") %>' runat="server">
												<asp:Image ImageUrl='images/16/list-add.png' runat="server" />
											</asp:LinkButton>
											<div visible='<%# !(bool)DataBinder.Eval(Container.DataItem, "Enabled") %>'  runat="server">
												<asp:Image ImageUrl='images/16/list-add-disabled.png' runat="server" />
											</div>
										</itemtemplate>
									</asp:TemplateColumn>
									<asp:TemplateColumn ItemStyle-CssClass="name">
										<itemtemplate>
											<asp:LinkButton CommandName="Add" CommandArgument='<%# DataBinder.Eval(Container.DataItem, "UserID") %>' Visible='<%# (bool)DataBinder.Eval(Container.DataItem, "Enabled") %>' runat="server">
												<%# DataBinder.Eval(Container.DataItem, "FullName") %>
											</asp:LinkButton>
											<div visible='<%# !(bool)DataBinder.Eval(Container.DataItem, "Enabled") %>'  runat="server">
												<%# DataBinder.Eval(Container.DataItem, "FullName") %>
											</div>
										</itemtemplate>
									</asp:TemplateColumn>
									<asp:TemplateColumn ItemStyle-CssClass="name">
										<itemtemplate>
											&nbsp;
										</itemtemplate>
									</asp:TemplateColumn>
								</columns>
							</asp:DataGrid>
								
							<iFolder:Pagging id="UserPagging" runat="server" />
			
						</div>
						
					</td><td>
					
						<div id="share-members">
				
							<div class="sub-title"><%= GetString("SHAREWITH") %></div>
							
							<asp:DataGrid
								ID="MemberData"
								GridLines="none"
								AutoGenerateColumns="false"
								ShowHeader="false"
								CssClass="entries"
								runat="server">
								
								<columns>
									<asp:TemplateColumn ItemStyle-CssClass="icon">
										<itemtemplate>
											<asp:LinkButton CommandName="Remove" CommandArgument='<%# DataBinder.Eval(Container.DataItem, "UserID") %>' Visible='<%# (bool)DataBinder.Eval(Container.DataItem, "Enabled") %>' runat="server">
												<asp:Image ImageUrl='images/16/list-remove.png' runat="server" />
											</asp:LinkButton>
											<div visible='<%# !(bool)DataBinder.Eval(Container.DataItem, "Enabled") %>'  runat="server">
												<asp:Image ImageUrl='images/16/list-remove-disabled.png' runat="server" />
											</div>
										</itemtemplate>
									</asp:TemplateColumn>
									<asp:TemplateColumn ItemStyle-CssClass="name">
										<itemtemplate>
											<asp:LinkButton CommandName="Remove" CommandArgument='<%# DataBinder.Eval(Container.DataItem, "UserID") %>' Visible='<%# (bool)DataBinder.Eval(Container.DataItem, "Enabled") %>' runat="server">
												<%# DataBinder.Eval(Container.DataItem, "FullName") %>
											</asp:LinkButton>
											<div visible='<%# !(bool)DataBinder.Eval(Container.DataItem, "Enabled") %>'  runat="server">
												<%# DataBinder.Eval(Container.DataItem, "FullName") %>
											</div>
										</itemtemplate>
									</asp:TemplateColumn>
									<asp:TemplateColumn ItemStyle-CssClass="name">
										<itemtemplate>
											&nbsp;
										</itemtemplate>
									</asp:TemplateColumn>
								</columns>
							</asp:DataGrid>
								
							<iFolder:Pagging id="MemberPagging" runat="server" />
			
						</div>
						
					</td></tr></table>
				
				</div>
								
			</div>
			
			<div class="buttons">
				<asp:Button ID="CreateButton" runat="server" />
				<asp:Button ID="ShareButton" runat="server" />
				<asp:Button ID="CancelButton" runat="server" />
			</div>
			
		</div>
		
	</div>
	
</form>

</body>

</html>
