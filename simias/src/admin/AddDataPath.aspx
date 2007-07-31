<%@ Register TagPrefix="iFolder" TagName="Footer" Src="Footer.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="TopNavigation" Src="TopNavigation.ascx" %>
<%@ Page language="c#" Codebehind="AddDataPath.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderWeb.Admin.AddDataPath"%>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>

<head>
                <meta http-equiv="Content-Type" content="text/html; charset=iso-8859-1">
                <meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">

                <title><%= GetString( "TITLE" ) %></title>

                <style type="text/css">
                        @import url( css/iFolderAdmin.css );
			@import url( css/DataPath.css );

                </style>

</head>

<body id="datapaths" runat="server">

<form runat="server">

        <div class="container">

                <iFolder:TopNavigation ID="TopNav" Runat="server" />

		<div class="pathnav">

                        <div class="detailnav">

	      		    	<div class="pagetitle">
                      
                			<%= GetString( "ADDDATASTORE" ) %>
				</div>
			
				<table class="datapathinfo">

                        		<tr>
                                		 <th>
   	                                	    	  <%= GetString( "NAME:" ) %>
	                                         </th>
	
	                	                <td>
        		                                 <asp:TextBox
					                     ID="DataPathName"
                        	        	             Runat="server"
                        		        	     CssClass="edittext" />
		                                </td>
        	                        </tr>

                	                <tr>
                        	           	 <th>
							<%= GetString( "FULLPATH:") %>
                                        	 </th>

	                                         <td>
        	                                        <asp:TextBox
                	                                    ID="FullPath"
                        	                            Runat="server"
                                	                    CssClass="edittext" />
                                        	 </td>
	                                </tr>
				</table>
				<div align="center" class="okcancelnav">
					<asp:Button
			                        ID="CancelButton"
                			        Runat="server"
                        			CssClass="ifolderbuttons"
	                        		OnClick="OnCancelButton_Click"
	        	                	Visible="True" />
					<asp:Button
						ID="AddDataPathButton"
						Runat="server"
                                                CssClass="ifolderbuttons"
                                                OnClick="OnAddDataPathButton_Click"
                                                Visible="True" />

				</div>	
			</div>
			
		</div>
	</div>
</form>
</body>
</html>


