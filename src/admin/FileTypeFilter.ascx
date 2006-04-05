<%@ Control Language="c#" AutoEventWireup="false" Codebehind="FileTypeFilter.ascx.cs" Inherits="Novell.iFolderWeb.Admin.FileTypeFilter" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>
<%@ Register TagPrefix="iFolder" TagName="PageFooter" Src="PageFooter.ascx" %>

<div id="filetypenav">

	<div class="policytitle"><%= GetString( "EXCLUDEDFILES" ) %></div>
	
	<div class="policydetails">
	
		<asp:TextBox 
			ID="NewFileTypeName" 
			Runat="server"
			CssClass="newfiletypename"
			Visible="False" />

		<asp:Button 
			ID="AddButton" 
			Runat="server" 
			CssClass="filetypeaddbutton" 
			OnClick="OnFileTypeAddClick"
			Visible="False" />
			
		<table class="filetypelistheader">
	
			<tr>
				<td class="checkboxcolumn">
					<asp:CheckBox 
						ID="AllFilesCheckBox" 
						Runat="server" 
						OnCheckedChanged="OnAllFilesChecked" 
						AutoPostBack="True" />
				</td>
			
				<td class="filenamecolumn">
					<%= GetString( "FILENAME" ) %>
				</td>
				
				<td class="enabledcolumn">
					<%= GetString( "STATUS" ) %>
				</td>
			</tr>
	
		</table>
		
		<asp:datagrid 
			id="FileTypeList" 
			runat="server" 
			AutoGenerateColumns="False" 
			CssClass="filetypelist"
			CellPadding="0" 
			CellSpacing="0"
			ShowHeader="False" 
			PageSize="5" 
			GridLines="None" 
			ItemStyle-CssClass="filetypelistitem"
			AlternatingItemStyle-CssClass="filetypelistaltitem">
			
			<Columns>
			
				<asp:BoundColumn DataField="FileRegExField" Visible="False" />
				
				<asp:TemplateColumn ItemStyle-CssClass="filetypeitem1" >
					
					<ItemTemplate>
						<asp:CheckBox 
							ID="FileTypeCheckBox" 
							Runat="server" 
							AutoPostBack="True"
							OnCheckedChanged="OnFileTypeCheckChanged" 
							Checked='<%# IsEntryChecked( DataBinder.Eval( Container.DataItem, "FileRegExField" ) ) %>'
							Visible='<%# ( bool )DataBinder.Eval( Container.DataItem, "VisibleField" ) %>' />
					</ItemTemplate>
					
				</asp:TemplateColumn>
				
				<asp:BoundColumn DataField="FileNameField" ItemStyle-CssClass="filetypeitem2" />
					
				<asp:BoundColumn DataField="EnabledField" ItemStyle-CssClass="filetypeitem3" />
					
			</Columns>
			
		</asp:DataGrid>
		
		<ifolder:PageFooter ID="FileTypeListFooter" Runat="server" />
		
		<asp:Button 
			ID="DeleteButton" 
			Runat="server" 
			CssClass="filetypecontrolbutton" 
			Enabled="False"
			OnClick="OnDeleteFileType"
			Visible="False" />
			
		<asp:Button
			ID="AllowButton"
			Runat="server"
			CssClass="filetypecontrolbutton"
			Enabled="False"
			OnClick="OnAllowFileType"
			Visible="False" />
			
		<asp:Button
			ID="DenyButton"
			Runat="server"
			CssClass="filetypecontrolbutton"
			Enabled="False"
			OnClick="OnDenyFileType"
			Visible="False" />
				
	</div>

</div>