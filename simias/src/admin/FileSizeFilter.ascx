<%@ Control Language="c#" AutoEventWireup="false" Codebehind="FileSizeFilter.ascx.cs" Inherits="Novell.iFolderWeb.Admin.FileSizeFilter" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>

<script language="javascript">

	function EnableButtons()
	{
		var saveButton = document.getElementById( "Policy_PolicyApplyButton" );
		if ( saveButton != null )
		{
			saveButton.disabled = false;
		}
		
		var cancelButton = document.getElementById( "Policy_PolicyCancelButton" );
		if ( cancelButton != null )
		{
			cancelButton.disabled = false;
		}
	}

</script>

<div id="filesizenav">

	<asp:Label ID="Title" Runat="server" CssClass="policytitle" />
	
	<div class="policydetails">
				
		<table class="policytable">

			<tr>
				<td class="policycheckbox">
					<asp:CheckBox ID="Enabled" Runat="server" AutoPostBack="True" CssClass="policycheckbox" />
				</td>
				
				<td class="policytabletag">
					<asp:Label ID="LimitTag" Runat="server" />
				</td>
				
				<td class="policytablevalue">
					<asp:TextBox 
						ID="LimitValue" 
						Runat="server" 
						CssClass="policytextbox" 
						onkeypress="EnableButtons()" />
				</td>
				
				<td>
					<%= GetString( "MB" ) %>
				</td>
			</tr>		
		
			<tr>
				<td>
				</td>
				
				<td class="policytabletag">
					<asp:Label ID="EffectiveTag" Runat="server" />
				</td>
				
				<td class="policytablevalue">
					<asp:Label ID="EffectiveValue" Runat="server" />
				</td>
				
				<td>
					<asp:Label ID="EffectiveUnits" Runat="server" />
				</td>
			</tr>
		
		</table>
		
	</div>
	
</div>

