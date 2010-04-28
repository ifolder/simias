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

		function EnableServerDetailsButtons()
		{
			document.getElementById( "SaveServerDetailsButton" ).disabled = false;
			document.getElementById( "CancelServerDetailsButton" ).disabled = false;
		}

		function EnableLdapDetailsButtons()
		{
			document.getElementById( "SaveLdapDetailsButton" ).disabled = false;
			document.getElementById( "CancelLdapDetailsButton" ).disabled = false;
		}

		function ConfirmDeletion()
		{		
			return alert("<%= GetString("CONFIRMDELETION") %>");
		}

        function ConfirmChangeMaster()
        {
        	return alert("<%= GetString("CONFIRMCHANGEMASTER") %>");
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

							<asp:LinkButton
								ID="ChangeMasterButton" 
								Runat="server" 
								CssClass="changemasterbuttons"
								Enabled="True"
							/>
							<asp:LinkButton
								ID="RepairServerButton" 
								Runat="server" 
								CssClass="changemasterbuttons"
								Enabled="False"
							/>
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
							 <asp:TextBox
								ID="PublicIP"
								Runat="server"
								Width=250
								onkeypress="EnableServerDetailsButtons()" />

						</td>
					</tr>
					
					<tr>
						<th>
							<%= GetString( "PRIVATEURI" ) %>
						</th>
						
						<td>
							 <asp:TextBox
								ID="PrivateIP"
								Runat="server"
								Width=250
								onkeypress="EnableServerDetailsButtons()" />
						</td>
					</tr>

					<tr>
                                                <th>
							<asp:Literal ID="MasterUri" Runat="server" />
                                                </th>

                                                <td>
                                                         <asp:TextBox
                                                                ID="MasterIP"
                                                                Runat="server"
                                                                Width=250
                                                                onkeypress="EnableServerDetailsButtons()" 
								Visible="false" />
                                                </td>

                                        </tr>

					<tr>
						<td>
							<asp:Button
								ID="CancelServerDetailsButton"
								Runat="server"
								CssClass="ifolderbuttons"
								OnClick="OnCancelServerDetailsButton_Click"
								Enabled="False"
								/>
						</td>
					
						<td>
							<asp:Button
								ID="SaveServerDetailsButton"
								Runat="server"
								CssClass="ifoldersavebutton"
								Enabled="False"
								OnClick="OnSaveServerDetailsButton_Click"
								/>
						</td>
					</tr>
					
				</table>

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
			
			<asp:LinkButton
				ID="ViewLogButton" 
				Runat="server" 
				CssClass="ifolderbuttons" />


				<asp:Label 
					ID="LogLevelLabel" 
					font-size = 12
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
		<div class="leftldapnav">
		
			<div class="pagetitle">
			
 				<%= GetString( "LDAPDETAILS" ) %>
				
			</div>
				<table class="detailinfo">

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
                                                        <%= GetString( "LDAPSTATUSTAG" ) %>
                                                </th>

                                                <td>
                                                        <asp:Literal ID="LdapStatus" Runat="server" />
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
								onkeypress="EnableLdapDetailsButtons()"
								CssClass="syncnowtextbox"
								/>

							<%= GetString( "MINUTES" ) %>

							<asp:LinkButton
								ID="SyncNowButton" 
								Runat="server" 
								CssClass="ifolderbuttons"
								Enabled="True"
								/>
						</th>
					</tr>

					<tr>
						<th>
							<%= GetString( "LDAPMEMBERDELETE" ) %>
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
								onkeypress="EnableLdapDetailsButtons()"
								CssClass="syncnowtextbox"
								/>

							<%= GetString( "MINUTES" ) %>
						</td>
					</tr>

					<tr>
						<td>
							<asp:Button
								ID="CancelLdapDetailsButton"
								Runat="server"
								CssClass="ifolderbuttons"
								Enabled="False"
								OnClick="OnCancelLdapDetailsButton_Click"
								/>
						</td>
					
						<td>
							<asp:Button
								ID="SaveLdapDetailsButton"
								Runat="server"
								CssClass="ifoldersavebutton"
								Enabled="False"
								OnClick="OnSaveLdapDetailsButton_Click"
								/>
						</td>
					</tr>
				</table>
		</div>		
	
		<div class="rightldapnav">

			<div class="pagetitle">
				 &nbsp;
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
                                                                Enabled="false" />
                                                </td>
                                        </tr>
					
					<tr>
                                                <th>
                                                        <%= GetString( "LDAPSSL" ) %>
                                                </th>

                                                <td>
                                                        <asp:Literal
                                                                          ID="LdapSsl"
                                                                          Runat="server"
                                                                           />
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
                                                                Enabled="false"
                                                                />
                                                </td>
                                        </tr>

                                        <tr>
                                                <th>
                                                        <%= GetString( "LDAPPROXYUSERPWD" ) %>
                                                </th>

                                                <td>
                                                        <asp:TextBox
                                                                ID="LdapProxyUserPwd"
                                                                TextMode="Password"
                                                                Enabled="false"
                                                                Runat="server"
                                                                />
                                                </td>
                                        </tr>
					
					<tr>
						<th>
							<%= GetString( "LDAPCONTEXTTAG" ) %>
						</th>
						
						<td>
							<textarea
								ID="LdapSearchContext"
                                                                runat="server"
                                                                class="edittext"
								readonly="true"
								wrap="soft"
								rows="2"
								cols="25"
                                                                ></textarea>
						</td>
					</tr>

					<tr> </tr><tr></tr><tr></tr><tr></tr>
					<tr> 
						<td>
					
							<asp:Button 
								    ID="LdapEditButton" 
								    Runat="server" 
								    CssClass="ldapeditifolderbuttons"
								    Enabled="False"
								    OnClick="OnLdapEditButton_Click"		    
								    />
						</td>
					</tr>
	
			</table>

		</div>

		
		<div class = "lognav" >

                        <div class="pagetitle">

                               <%= GetString("DATASTORE") %>

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
						<asp:BoundColumn DataField="NameField" Visible="False" />
						<asp:TemplateColumn ItemStyle-CssClass="datapathitem2">
                                                        <ItemTemplate>
                                                                <asp:Label Text='<%# GetFriendlyString(DataBinder.Eval(Container.DataItem, "NameField"),20) %>' ToolTip='<%# DataBinder.Eval(Container.DataItem, "NameField") %>' runat="server"/>
                                                        </ItemTemplate>

                                                </asp:TemplateColumn>
						<asp:TemplateColumn ItemStyle-CssClass="datapathitem3">
                                                        <ItemTemplate>
                                                                <asp:Label Text='<%# GetFriendlyString(DataBinder.Eval(Container.DataItem, "FullPathField"),48) %>' ToolTip='<%# DataBinder.Eval(Container.DataItem, "FullPathField") %>' runat="server"/>
                                                        </ItemTemplate>

                                                </asp:TemplateColumn>
                	                        <asp:BoundColumn ItemStyle-CssClass="datapathitem4" DataField="FreeSpaceField"/>
						<asp:BoundColumn ItemStyle-CssClass="datapathitem5" DataField="StatusField"/>
	
        	                        </Columns>
	
        	                        </asp:datagrid>
                                <ifolder:ListFooter ID="DataPathsFooter" Runat="server"/>
			</div>

                        <div class = "nav">
				<asp:Button
                                        ID="DeleteButton"
                                        Runat="server"
                                        CssClass="ifolderbuttons"
                                        Enabled="False"
                                        OnClick="OnDeleteButton_Click" />

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
