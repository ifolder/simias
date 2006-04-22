<%@ Register TagPrefix="iFolder" TagName="Footer" Src="Footer.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="ListFooter" Src="ListFooter.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="TopNavigation" Src="TopNavigation.ascx" %>
<%@ Page language="c#" Codebehind="CreateUser.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderWeb.Admin.CreateUser" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
	
<head>
		<meta http-equiv="Content-Type" content="text/html; charset=iso-8859-1">
		<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">

		<title><%= GetString( "TITLE" ) %></title>
		
		<style type="text/css">
			@import url( css/iFolderAdmin.css ); 
			@import url( css/CreateUser.css ); 
		</style>
		
</head>

<body id="users" runat="server">

<script type="text/javascript">

	var addToFull = true;
	var firstLength = 0;
	
	function addFirstToFull(b)
	{
		if ( addToFull && ( b.value.length > 0 ) )
		{
			var full = document.getElementById( "FullName" );
			if ( full.value.length == 0 )
			{
				full.value = b.value;
			}
			else
			{
				var s = full.value.slice( firstLength, full.value.length );
				full.value = b.value + s;
			}
			
			firstLength = b.value.length;
		}
		
		return true;
	}
	
	function addLastToFull(b)
	{
		if ( addToFull && ( b.value.length > 0 ) )
		{
			var full = document.getElementById( "FullName" );
			if ( full.value.length == 0 )
			{
				full.value = b.value;
			}*
			else
			{
				var s = full.value.slice( 0, firstLength + 1 );
				full.value = s + " " + b.value;
			}
		}
	}
	
	function fullNameChanged(b)
	{
		addToFull = b.value.length == 0;
	}

</script>

<form runat="server">

	<div class="container">

		<iFolder:TopNavigation ID="TopNav" Runat="server" />

		<div class="leftnav">

			<div class="detailnav">

				<div class="pagetitle">
				
					<%= GetString( "CREATEUSER" ) %>
					
				</div>

				<table class="detailinfo">

					<tr>
						<th>
							<%= GetString( "USERNAMETAG" ) %>
						</th>

						<td>
							<asp:TextBox 
								ID="UserName" 
								Runat="server" 
								CssClass="edittext" />
						</td>
					</tr>

					<tr>
						<th>
							<%= GetString( "FIRSTNAMETAG" ) %>
						</th>

						<td>
							<asp:TextBox 
								ID="FirstName" 
								Runat="server" 
								CssClass="edittext" 
								onblur="return addFirstToFull(this)" />
						</td>
					</tr>
					
					<tr>
						<th>
							<%= GetString( "LASTNAMETAG" ) %>
						</th>
					
						<td>
							<asp:TextBox 
								ID="LastName" 
								Runat="server" 
								CssClass="edittext"
								onblur="return addLastToFull(this)" />
						</td>
					</tr>
					
					<tr>
						<th>
							<%= GetString( "FULLNAMETAG" ) %>
						</th>
					
						<td>
							<asp:TextBox 
								ID="FullName" 
								Runat="server" 
								CssClass="edittext"
								onchange="return fullNameChanged(this)" />
						</td>
					</tr>

					<tr>
						<th>
							<%= GetString( "PASSWORDTAG" ) %>
						</th>

						<td>
							<asp:TextBox 
								ID="Password" 
								Runat="server" 
								CssClass="edittext" 
								TextMode="Password" />
						</td>
					</tr>

					<tr>
						<th>
							<%= GetString( "RETYPEPASSWORDTAG" ) %>
						</th>

						<td>
							<asp:TextBox 
								ID="RetypedPassword" 
								Runat="server" 
								CssClass="edittext" 
								TextMode="Password" />
						</td>
					</tr>

				</table>

				<div align="center">
				
					<asp:Button 
						ID="CreateButton" 
						Runat="server" 
						CssClass="ifolderbuttons" 
						OnClick="OnCreateButton_Click" />

					<asp:Button 
						ID="CancelButton" 
						Runat="server" 
						CssClass="ifolderbuttons" 
						OnClick="OnCancelButton_Click" />
						
				</div>
				
			</div>

		</div>

	</div>

	<ifolder:Footer id="footer" runat="server" />

</form>

</body>

</html>
