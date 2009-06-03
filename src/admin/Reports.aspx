<%@ Page language="c#" Codebehind="Reports.aspx.cs" AutoEventWireup="false" Inherits="Novell.iFolderWeb.Admin.Reports" %>
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
		@import url(css/Reports.css);
	</style>

</head>

<body id="reports" runat="server">
	
<form runat="server" ID="Form1">

	<div class="container">
			
		<iFolder:TopNavigation ID="TopNav" Runat="server" />
		
		<div class="leftnav">

<!-- 			<div class="detailnav"> -->

				<div class="pagetitle">
				
					<%= GetString( "CONFIGUREREPORTING" ) %>
					
				</div>
				
				<asp:CheckBox 
					ID="EnableReporting" 
					Runat="server" 
					AutoPostBack="True"
					OnCheckedChanged="OnEnableReporting_Changed" />
					
				<asp:Label ID="EnableReportingLabel" Runat="server" />
				
				<table class="when" >
				
					<tr>
						<th colspan="2">
						
							<%= GetString( "FREQUENCY" ) %>
							
						</th>
					</tr>
			
					<tr>
						<td>
						
							<asp:RadioButtonList 
								ID="FrequencyList" 
								Runat="server" 
								AutoPostBack="True" 
								RepeatDirection="Vertical"
								OnSelectedIndexChanged="OnFrequencyList_Changed">
								
								<asp:ListItem></asp:ListItem>
								<asp:ListItem></asp:ListItem>
								<asp:ListItem></asp:ListItem>
								
							</asp:RadioButtonList>
							
						</td>	
					</tr>
					
				</table>
				
				<table class="when">
				
					<tr>
						<th colspan="2">
						
							<%= GetString( "TIME" ) %>
							
						</th>
					</tr>
					
					<tr>
					
						<td>&nbsp;</td>
						<td>&nbsp;</td>
						
					</tr>
					
					<tr>
						<td class="whenlabel">
						
							<asp:Label ID="DayLabel" Runat="server" />
							
						</td>
						
						<td>
						
							<asp:DropDownList
								ID="DayOfMonthList"
								Runat="server"
								AutoPostBack="True"
								OnSelectedIndexChanged="OnDayOfMonthList_Changed"
								Visible="False" />
								
							<asp:DropDownList
								ID="DayOfWeekList"
								Runat="server"
								AutoPostBack="True"
								OnSelectedIndexChanged="OnDayOfWeekList_Changed"
								Visible="False" />
								
						</td>
					</tr>
					
					<tr>
						<td class="whenlabel">
						
							<%= GetString( "ATTAG" ) %>
							
						</td>
						
						<td>
						
							<asp:DropDownList 
								ID="TimeOfDayList" 
								Runat="server" 
								AutoPostBack="True" 
								OnSelectedIndexChanged="OnTimeOfDayList_Changed" />
								
						</td>
					</tr>
				
				</table>
				
				<table class="format">
				
					<tr>
						<th colspan="2">
						
							<%= GetString( "SAVETO" ) %>
							
						</th>
					</tr>
					
					<tr>
						<td>

							<asp:RadioButtonList
								ID="ReportLocation" 
								Runat="server" 
								AutoPostBack="True" 
								RepeatDirection="Vertical"
								OnSelectedIndexChanged="OnReportLocation_Changed">
								
								<asp:ListItem></asp:ListItem>
								<asp:ListItem></asp:ListItem>
								
							</asp:RadioButtonList>
							
						</td>
					</tr>
					
					<tr>
						<td>
						
							<%= GetString( "FORMATTAG" ) %>
							
							<asp:DropDownList
								ID="FormatList"
								Runat="server"
								AutoPostBack="True"
								OnSelectedIndexChanged="OnFormatList_Changed" />
								
						</td>
					</tr>
					
				</table>
				
				<div class="summary">
				
					<asp:Label ID="Summary" Runat="server" />
				
				</div>
				
				<div class="reportbuttons">
				
					<asp:Button 
						ID="SaveReportConfig" 
						Runat="server" 
						CssClass="ifoldersavebutton"
						Enabled="False"
						OnClick="OnSaveReport_Click" />
						
					<asp:Button 
						ID="CancelReportConfig" 
						Runat="server" 
						CssClass="ifolderbuttons"
						Enabled="False"
						OnClick="OnCancelReport_Click" />
					
				</div>
				
<!--			</div> -->
			
		</div>

	</div>
	
<!-- 	<ifolder:Footer id="footer" runat="server" /> -->
				
</form>
	
</body>

</html>
