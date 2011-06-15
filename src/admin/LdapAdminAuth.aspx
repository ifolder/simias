<%@ Register TagPrefix="iFolder" TagName="Footer" Src="Footer.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="TopNavigation" Src="TopNavigation.ascx" %>
<%@ Page language="c#" Codebehind="LdapAdminAuth.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderWeb.Admin.LdapAdminAuth"%>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>

<head>
                <meta http-equiv="Content-Type" content="text/html; charset=iso-8859-1">
                <meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">

                <title><%= GetString( "TITLE" ) %></title>

                <style type="text/css">
                        @import url( css/iFolderAdmin.css );
			@import url(css/ServerDetails.css);
                </style>
		
		 <script language="javascript">

	                function EnableSystemButtons()
        	        {
                	        document.getElementById( "OkButton" ).disabled = false;
                        	document.getElementById( "CancelButton" ).disabled = false;
					}
		</script>

</head>

<body id="server" runat="server">

<form runat="server">

        <div class="container">

                <iFolder:TopNavigation ID="TopNav" Runat="server" />


<%--     		    	<div class="pagetitle">
               			<%= GetString( "EDITLDAPDETAILS" ) %>
			</div>
--%>			
			<div class="lognav">
				<table class="detailinfo">
					
					<tr>
						<th class="pagetitle" >
							<%= GetString( "ENTERLDAPDETAILS" ) %>
						</th>					
					</tr>
					<tr></tr> <tr></tr>

                        		<tr>
						<th>
							<%= GetString( "LDAPADMINNAME" ) %>
						</th>
	                	                <td>
        		                                 <asp:TextBox
					                     ID="LdapAdminName"
                        	        	             Runat="server"/>
		                                </td>
        	                        </tr>

                	                <tr>
						<th>
							<%= GetString( "LDAPADMINPWD") %>
						</th>
	                                         <td>
        	                                        <asp:TextBox
                	                                    ID="LdapAdminPwd"
                        	                            Runat="server"
							    TextMode="Password" />
                                        	 </td>
	                                </tr>
				</table>
			</div>
		
			<div class="lognav">

				<table class="detailinfo">	

					<tr>
						<th	class="pagetitle" >
							<%= GetString("EDITLDAPDETAILS") %>
						</th>
					</tr>
					<tr></tr> <tr></tr>

					<tr>
						<th>
							<%= GetString( "LDAPSERVER" ) %>
						</th>

						<td>
							<asp:TextBox
								ID="LdapServer"
								Runat="server"
								CssClass="syncnowtextbox"
								onkeypress="EnableSystemButtons()" />
						</td>
					</tr>

					<tr>
						<th>
							<%= GetString( "LDAPSSL" ) %>
						</th>

						<td>
							<asp:DropDownList
								ID="LdapSslList"
								Runat="server"
								AutoPostBack="false" />
						</td>
					</tr>
					<tr>
						<th>
							<%= GetString( "LDAPPROXYUSER" ) %>
						</th>

						<td>
							<asp:TextBox
								ID="LdapProxyUser"
								Runat="server"
								onkeypress="EnableSystemButtons()" />
						</td>
					</tr>

					<tr>
						<th>
						</th>
						<td>
							<%= GetString( "PASSWORDCHANGEWARNING" ) %>
						</td>
					</tr>

					<tr>
						<th>
							<%= GetString( "LDAPPROXYUSERPWD" ) %>
						</th>

						<td>
							<asp:TextBox
								ID="LdapProxyUserPwd"
								Runat="server"
							    	TextMode="Password"
							/>
						</td>
					</tr>

					<tr>
						<th>
							<%= GetString( "CONFIRMLDAPPROXYUSERPWD" ) %>
						</th>

						<td>
							<asp:TextBox
								ID="ConfirmLdapProxyUserPwd"
								Runat="server"
							    	TextMode="Password"
							/>
						</td>
					</tr>
					
					<tr>
						<th>
							<%= GetString( "LDAPCONTEXTTAG" ) %>
						</th>

						<td>
							<asp:TextBox
								ID="LdapSearchContext"
								Runat="server"
								CssClass="edittextbox"
								wrap="true"
								Textmode="Multiline"
								Rows="2"
								Columns="25"
								onkeypress="EnableSystemButtons()"/>
						</td>
					</tr>

                                        <tr>
                                                <td>
							<asp:Button
			                        		ID="CancelButton"
                			        		Runat="server"
                        					CssClass="ifolderbuttons"
	                        				OnClick="OnCancelButton_Click"
	        	                			Visible="True" />
                                                </td>

                                                <td>

							<asp:Button
								ID="OkButton"
								Runat="server"
                                                		CssClass="ifolderbuttons"
                                                		OnClick="OnOkButton_Click"
                                                		Visible="True" />
					</tr>
				</table>
			</div>
			
	</div>
</form>
</body>
</html>


