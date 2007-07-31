<%@ Register TagPrefix="iFolder" TagName="TopNavigation" Src="TopNavigation.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="ListFooter" Src="ListFooter.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="Footer" Src="Footer.ascx" %>
<%@ Page language="c#" Codebehind="ServerDetails.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderWeb.Admin.ServerDetails" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" > 
<html>
	
<head>

	<meta http-equiv="Content-Type" content="text/html; charset=iso-8859-1">
	<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">

	<title><%= GetString( "TITLE" ) %></title>
		
	<style type="text/css">
		@import url(css/iFolderAdmin.css);
		@import url(css/ServerDetails.css);
		@import url(css/DataPath.css);
	</style>

	<script language="javascript">

		function EnableSystemButtons()
		{
			document.getElementById( "SaveButton" ).disabled = false;
			document.getElementById( "CancelButton" ).disabled = false;
		}
	
	</script>


</head>
	
<body id="server" runat="server">
	
<form runat="server" ID="Form1">

	<div class="container">
			
		<iFolder:TopNavigation ID="TopNav" Runat="server" />
		
		<div class="leftnav">
		
			<div class="detailnav">
			
				<div class="pagetitle">
				
					<%= GetString( "SERVERDETAILS" ) %>
					
				</div>
				
				<table class="detailinfo">
				
					<tr>
						<th>
							<%= GetString( "NAMETAG" ) %>
						</th>
						
						<td>
							<asp:Literal ID="Name" Runat="server" />
						</td>
					</tr>
					
					<tr>
						<th>
							<%= GetString( "TYPETAG" ) %>
						</th>
						
						<td>
							<asp:Literal ID="Type" Runat="server" />
						</td>
					</tr>

					<tr>
						<th>
							<%= GetString( "DNSNAMETAG" ) %>
						</th>

						<td>
							<asp:Literal ID="DnsName" Runat="server" />
						</td>
					</tr>
					
					<tr>
						<th>
							<%= GetString( "PUBLICURI" ) %>
						</th>
						
						<td>
							<asp:Literal ID="PublicIP" Runat="server" />
						</td>
					</tr>
					
					<tr>
						<th>
							<%= GetString( "PRIVATEURI" ) %>
						</th>
						
						<td>
							<asp:Literal ID="PrivateIP" Runat="server" />
						</td>
					</tr>
					
				</table>

			</div>

			<div class="reportnav">
			
				<div class="pagetitle">
				
					<%= GetString( "SERVERREPORTS" ) %>
						
				</div>
					
				<asp:DropDownList 
					ID="ReportList" 
					Runat="server" 
					AutoPostBack="True" />
				
				<asp:Button 
					ID="ViewReportButton" 
					Runat="server"
					CssClass="ifolderbuttons"
				        OnClick="ViewReportFile" />
			
			</div>
				
		</div>
		
		<div class="rightnav">
		
			<div class="detailnav">
			
				<div class="pagetitle">
				
					<%= GetString( "SERVERSTATUS" ) %>
					
				</div>
				
				<table class="detailinfo">
				
					<tr>
						<th>
							<%= GetString( "STATUSTAG" ) %>
						</th>
						
						<td>
							<asp:Literal ID="Status" Runat="server" />
						</td>
					</tr>
					
					<tr>
						<th>
							<%= GetString( "USERSTAG" ) %>
						</th>
						
						<td>
							<asp:Literal ID="UserCount" Runat="server" />
						</td>
					</tr>

					<tr>
						<th>
							<%= GetString( "IFOLDERSTAG" ) %>
						</th>

						<td>
							<asp:Literal ID="iFolderCount" Runat="server" />
						</td>
					</tr>
<!--					
					<tr>
						<th>
							<%= GetString( "LOGGEDONUSERSTAG" ) %>
						</th>
						
						<td>
							<asp:Literal ID="LoggedOnUsersCount" Runat="server" />
						</td>
					</tr>
					
					<tr>
						<th>
							<%= GetString( "SESSIONSTAG" ) %>
						</th>
						
						<td>
							<asp:Literal ID="SessionCount" Runat="server" />
						</td>
					</tr>
					
					<tr>
						<th>
							<%= GetString( "DISKSPACEUSEDTAG" ) %>
						</th>
						
						<td>
							<asp:Literal ID="DiskSpaceUsed" Runat="server" />
						</td>
					</tr>
					
					<tr>
						<th>
							<%= GetString( "DISKSPACEAVAILABLETAG" ) %>
						</th>
						
						<td>
							<asp:Literal ID="DiskSpaceAvailable" Runat="server" />
						</td>
					</tr>
-->					
					<tr>
						<th>
							<%= GetString( "LDAPSTATUSTAG" ) %>
						</th>
						
						<td>
							<asp:Literal ID="LdapStatus" Runat="server" />
						</td>
					</tr>
<!--					
					<tr>

						<th>
							<%= GetString( "MAXCONNECTIONSTAG" ) %>
						</th>
						
						<td>
							<asp:Literal ID="MaxConnectionCount" Runat="server" />
						</td>
					</tr>
-->					
				</table>

			</div>
			
		</div>
		
		<div class="lognav">
		
			<div class="pagetitle">
			
				<%= GetString( "SERVERLOGS" ) %>
				
			</div>
			
			<asp:Label 
				ID="LogLabel" 
				Visible = "False"
				Runat="server" />

			<asp:DropDownList 
				ID="LogList" 
				Runat="server" 
				AutoPostBack="True" />
			
			<asp:Button 
				ID="ViewLogButton" 
				Runat="server" 
				CssClass="ifolderbuttons" 
				OnClick="ViewLogFile" />


				<asp:Label 
					ID="LogLevelLabel" 
					Runat="server" />
					
				<asp:DropDownList 
					ID="LogLevelList" 
					Runat="server" 
					AutoPostBack="True" />
				
				<asp:Button 
					ID="LogLevelButton" 
					Runat="server" 
					CssClass="ifolderbuttons" 
					OnClick="LogLevelButtonClicked"
					Enabled="True" />
		</div>
		<div class="lognav">
		
			<div class="pagetitle">
			
<!-- 				<%= GetString( "SERVERLOGS" ) %> -->
				LDAP Details
				
			</div>
				<table class="detailinfo">
					
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
							<%= GetString( "LDAPUPSINCE" ) %>
						</th>
						
						<td>
							<asp:Literal ID="LdapUpSince" Runat="server" />
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
									  AutoPostBack="True" />
						</td>
					</tr>

				
					<tr>
						<th>
							<%= GetString( "LDAPPROXYUSER" ) %>
						</th>
						
						<td>
							<asp:Literal ID="LdapProxyUser" Runat="server" />
						</td>
					</tr>

					<tr>
						<th>
							<%= GetString( "LDAPCYCLES" ) %>
						</th>
						
						<td>
							<asp:Literal ID="LdapCycles" Runat="server" />
						</td>
					</tr>

					<tr>
						<th>
							<%= GetString( "IDENTITYSYNCTAG" ) %>
						</th>

						<th>						
							<asp:TextBox 
								ID="IDSyncInterval" 
								Runat="server" 
								CssClass="syncnowtextbox"
								onkeypress="EnableSystemButtons()" />

							<%= GetString( "MINUTES" ) %>

							<asp:Button 
								ID="SyncNowButton" 
								Runat="server" 
								CssClass="syncnowbutton"
								OnClick="OnSyncNowButton_Click"			
								Enabled="True"
								/>
						</th>
					</tr>

					<tr>
						<th>
							<%= GetString( "LDAPMEMBERDELETEGRACEINT" ) %>
						</th>
						
						<td>
							<asp:TextBox 
								ID="LdapDeleteGraceInterval" 
								Runat="server" 
								CssClass="syncnowtextbox"
								onkeypress="EnableSystemButtons()"/>

							<%= GetString( "MINUTES" ) %>
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
								CssClass="syncnowtextbox"
								onkeypress="EnableSystemButtons()"/>
						</td>
					</tr>

					<tr> 
						<td>
							<asp:Button	
									ID="CancelButton" 
									Runat="server"	
									CssClass="ifolderbuttons"
									Enabled="False"
									/>
					
							<asp:Button 
								    ID="SaveButton" 
								    Runat="server" 
								    CssClass="ifolderbuttons"
								    Enabled="False"
								    OnClick="OnSaveButton_Click"		    
								    />
						</td>
					</tr>
				</table>
		</div>		
		
		<div class = "lognav" >

                        <div class="pagetitle">

                               <%= GetString("DATASTORE") %>

                        </div>
                </div>

                <div class="datapathsnav">
			
	                <table class="datapathlistheader" cellpadding="0" cellspacing="0" border="0">

                              <tr>
        		              <td class="checkboxcolumn">
                        		     <asp:CheckBox
                                        	  ID="AllDataPathCheckBox"
                                                  Runat="server"
                                                  OnCheckedChanged="OnAllDataPathChecked"
                                                  AutoPostBack="True"     />
                                      </td>
	
                                      <td class="namecolumn">
                                   	   <%= GetString( "NAME" ) %>
                                      </td>

                                      <td class="fullpathcolumn">
                                            <%= GetString( "FULLPATH" ) %>
                                      </td>

                                      <td class="freespacecolumn">
                                            <%= GetString( "FREESPACE" ) %>
                                      </td>

				      <td class="statuscolumn">
					    <%= GetString( "ENABLED" ) %>	
				      </td>	
	                    </tr>

        		</table>
			<asp:datagrid
				id="DataPaths"
                                Runat="server"
                                AutoGenerateColumns="False"
                                CssClass="datapathlist"
                                CellPadding="0"
                                CellSpacing="0"
                                PageSize="5"
                                GridLines="None"
                                ShowHeader="False"
                                AlternatingItemStyle-CssClass="datapathlistaltitem"
                                ItemStyle-CssClass="datapathlistitem">

                                <Columns>
					<asp:BoundColumn DataField="DisabledField" Visible="False" />
					<asp:TemplateColumn ItemStyle-CssClass="datapathitem1">
						<ItemTemplate>
							<asp:CheckBox
                                               		   	ID="DataPathCheckBox"
		                                                Runat="server"
                                                  		AutoPostBack="True"  
								OnCheckedChanged="OnPathChecked"
								Visible='<%# DataBinder.Eval( Container.DataItem, "VisibleField" ) %>'
								Checked='<%# GetMemberCheckedState( DataBinder.Eval( Container.DataItem, "NameField" ) ) %>'
								Enabled='<%# IsPathEnabled( DataBinder.Eval( Container.DataItem, "NameField" ) ) %>' /> 
                                        	</ItemTemplate>
					</asp:TemplateColumn>
	                                <asp:BoundColumn ItemStyle-CssClass="datapathitem2" DataField="NameField"/>
        	                        <asp:BoundColumn ItemStyle-CssClass="datapathitem3" DataField="FullPathField"/>
                                        <asp:BoundColumn ItemStyle-CssClass="datapathitem4" DataField="FreeSpaceField"/>
					<asp:BoundColumn ItemStyle-CssClass="datapathitem5" DataField="StatusField"/>

                                </Columns>

                                </asp:datagrid>
                                <ifolder:ListFooter ID="DataPathsFooter" Runat="server"/>


                        <div class = "nav">
                                <asp:Button
                                        ID="DisableButton"
                                        Runat="server"
                                        CssClass="ifolderbuttons"
                                        Enabled="False"
                                        OnClick="OnDisableButton_Click" />

                                <asp:Button
                                        ID="EnableButton"
                                        Runat="server"
                                        CssClass="ifolderbuttons"
                                        Enabled="False"
                                        OnClick="OnEnableButton_Click" />

				<asp:Button
                                        ID="AddButton"
                                        Runat="server"
                                        CssClass="ifolderbuttons"
                                        OnClick="OnAddButton_Click" />
                        </div>
		</div>
        </div>

</form>

</body>

</html>
