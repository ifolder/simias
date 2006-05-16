<%@ Control Language="c#" Codebehind="iFolderActions.ascx.cs" AutoEventWireup="false" Inherits="Novell.iFolderApp.Web.iFolderActionsControl"%>

<!-- actions -->

<div id="actions" class="group">
	
	<script type="text/javascript">

		function ConfirmiFolderRemove(f)
		{
			return confirm("<%= GetString("IFOLDER.CONFIRMREMOVE") %>");
		}
	
		function ConfirmiFolderDelete(f)
		{
			return confirm("<%= GetString("IFOLDER.CONFIRMDELETE") %>");
		}
	
	</script>

	<div class="box">
	
		<div class="title"><%= GetString("ACTIONS") %></div>
	
			<div id="Remove" class="link" runat="server">
				<asp:LinkButton ID="RemoveButton" runat="server" />
			</div>
			
			<div id="Delete" class="link" runat="server">
				<asp:LinkButton ID="DeleteButton" runat="server" />
			</div>
	
	</div>
		
</div>
