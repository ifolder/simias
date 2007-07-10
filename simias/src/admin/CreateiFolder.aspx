<%@ Page language="c#" Codebehind="CreateiFolder.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderWeb.Admin.CreateiFolder" %>
<%@ Register TagPrefix="iFolder" TagName="TopNavigation" Src="TopNavigation.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="Footer" Src="Footer.ascx" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>

<head>

	<meta http-equiv="Content-Type" content="text/html; charset=iso-8859-1">
	<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">
	
	<title><%= GetString( "TITLE" ) %></title>
	
	<style type="text/css">
		@import url(css/iFolderAdmin.css);
		@import url(css/CreateiFolder.css);
	</style>
	
	<script language="javascript">

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

		function EnableNextButton()
		{
			document.getElementById( "NextButton" ).disabled = false;
		}

	</script>

</head>

<body id="ifolders" runat="server">

<form runat="server" ID="Form1">

	<div class="container">
			
		<iFolder:TopNavigation ID="TopNav" Runat="server" />
				
		<div id="CreateiFolderDiv" runat="server">
	
			<div class="pagetitle">
				
				<%= GetString( "CREATENEWIFOLDER" ) %>
				
			</div>
		
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
							onkeypress="EnableNextButton()" />
					</td>
				</tr>
			
				<tr>
					<td>
						<asp:Label ID="DescriptionLabel" Runat="server" />
					</td>
				
					<td class="tablecolumn">
						<asp:TextBox 
							ID="Description" 
							TextMode="MultiLine" 
							Rows="3"
							onkeydown="return SubmitKeyDown(event, 'NextButton');"
							Runat="server" />
					</td>
				</tr>
			
			</table>
			
				<div align="center" class="okcancelnav">
				
					<asp:Button ID="NextButton" Runat="server" CssClass="ifolderbuttons" Enabled="False" OnClick="NextButton_Clicked" />
					
					<asp:Button ID="CancelButton" Runat="server" CssClass="ifolderbuttons" OnClick="CancelButton_Clicked" />
					
				</div>
			
		
		</div>

	</div>
		
	
</form>
		
</body>
	
</html>
