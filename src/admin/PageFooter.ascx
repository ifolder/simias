<%@ Control Language="c#" AutoEventWireup="false" Codebehind="PageFooter.ascx.cs" Inherits="Novell.iFolderWeb.Admin.PageFooter" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>

<table class="footertable" cellpadding="0" cellspacing="0" border="0">

	<tr>
		<td class="leftfooter">
			<asp:ImageButton 
				ID="PageFirstButton" 
				Runat="server" 
				Visible="False" 
				ImageUrl="images/go-first.png"/>
				
			<asp:ImageButton 
				ID="PagePreviousButton" 
				Runat="server" 
				Visible="False" 
				ImageUrl="images/go-previous.png"/>
				
			<asp:Image 
				ID="PageFirstDisabledButton" 
				Runat="server" 
				Visible="True" 
				ImageUrl="images/go-first-gray.png"/>
				
			<asp:Image 
				ID="PagePreviousDisabledButton" 
				Runat="server" 
				Visible="True" 
				ImageUrl="images/go-previous-gray.png"/>
		</td>
		
		<td class="centerfooter">
			<asp:Label ID="PageText" Runat="server"/>
		</td>
		
		<td class="rightfooter">
			<asp:ImageButton 
				ID="PageNextButton" 
				Runat="server" 
				Visible="True" 
				ImageUrl="images/go-next.png"/>
				
			<asp:ImageButton 
				ID="PageLastButton" 
				Runat="server" 
				Visible="True" 
				ImageUrl="images/go-last.png"/>
				
			<asp:Image 
				ID="PageNextDisabledButton" 
				Runat="server" 
				Visible="False" 
				ImageUrl="images/go-next-gray.png"/>
				
			<asp:Image 
				ID="PageLastDisabledButton" 
				Runat="server" 
				Visible="False" 
				ImageUrl="images/go-last-gray.png"/>
		</td>
	</tr>
	
</table>
