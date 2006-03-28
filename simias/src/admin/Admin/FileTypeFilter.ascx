<%@ Control Language="c#" AutoEventWireup="false" Codebehind="FileTypeFilter.ascx.cs" Inherits="Novell.iFolderWeb.Admin.FileTypeFilter" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>

<div id="filetypenav">

	<div class="policytitle"><%= GetString( "FILETYPEFILTER" ) %></div>
	
	<div id="nonsystempolicy" class="policydetails">
	
		<asp:DataGrid 
			ID="FileTypeList" 
			Runat="server" 
			CssClass="filetypetable" 
			GridLines="Both"
			AllowPaging="true" 
			AllowSorting="False" 
			AutoGenerateColumns="False" 
			ShowHeader="True" 
			PageSize="5"
			PagerStyle-Mode="NextPrev" 
			PagerStyle-Position="Bottom" 
			PagerStyle-CssClass="filetypetablepages">
			
			<Columns>
			
				<asp:BoundColumn DataField="FileRegExField" Visible="False" />
				
				<asp:TemplateColumn 
					HeaderStyle-CssClass="filetypetableheader" 
					ItemStyle-HorizontalAlign="Center">
					
					<ItemTemplate>
						<asp:CheckBox 
							ID="FileTypeCheckBox" 
							Runat="server" 
							AutoPostBack="True"
							OnCheckedChanged="FileTypeCheckChanged" 
							Checked='<%# DataBinder.Eval( Container.DataItem, "EnabledField" ) %>' />
					</ItemTemplate>
					
				</asp:TemplateColumn>
				
				<asp:BoundColumn 
					DataField="FileNameField" 
					ReadOnly="True" 
					HeaderStyle-CssClass="filetypetableheader"
					ItemStyle-CssClass="filetypetableitem" />
					
			</Columns>
			
		</asp:DataGrid>
		
	</div>
	
	<div id="systempolicy" class="policydetails">
	
		<asp:DataGrid 
			ID="SystemFileTypeList" 
			Runat="server" 
			CssClass="filetypetable" 
			GridLines="Both"
			AllowPaging="true" 
			AllowSorting="False" 
			AutoGenerateColumns="False" 
			ShowHeader="True" 
			PageSize="5"
			PagerStyle-Mode="NextPrev" 
			PagerStyle-Position="Bottom" 
			PagerStyle-CssClass="filetypetablepages">
			
			<Columns>
			
				<asp:BoundColumn DataField="FileRegExField" Visible="False" />
				
				<asp:BoundColumn 
					DataField="FileNameField" 
					ReadOnly="True" 
					HeaderStyle-CssClass="filetypetableheader"
					ItemStyle-CssClass="filetypetableitem" />
					
			</Columns>
			
		</asp:DataGrid>
		
	</div>
	
</div>
