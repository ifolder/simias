<%@ Control Language="c#" AutoEventWireup="false" Codebehind="iFolderLimit.ascx.cs" Inherits="Novell.iFolderWeb.Admin.iFolderLimit" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>

<script language="javascript">

	function EnableiFolderLimitButtons()
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

<div id="iFolderLimitNav" runat="server" >

	<asp:Label ID="Title" Runat="server" CssClass="policytitle" />
	
	<div class="policydetails">
	
		
		<table class="policytable">
		
			<tr>
				<td class="policycheckbox">
					<asp:CheckBox ID="Enabled" Runat="server" AutoPostBack="True" />
				</td>
				
				<td class="policytabletag">
					<asp:Label ID="LimitTag" Runat="server" />
				</td>

				<td class="policytablevalue">
					<asp:TextBox 
						ID="LimitValue" 
						Runat="server" 
						CssClass="sharepolicytextbox" 
						onkeypress="EnableiFolderLimitButtons()" />
				</td>

				
			</tr>
		
		</table>
		
	</div>
	
</div>

