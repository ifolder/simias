<%@ Control Language="c#" AutoEventWireup="false" Codebehind="Policy.ascx.cs" Inherits="Novell.iFolderWeb.Admin.Policy" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>
<%@ Register TagPrefix="iFolder" TagName="iFolderEnabled" Src="iFolderEnabled.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="iFolderLimit" Src="iFolderLimit.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="AccountEnabled" Src="AccountEnabled.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="SyncInterval" Src="SyncInterval.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="FileTypeFilter" Src="FileTypeFilter.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="FileSizeFilter" Src="FileSizeFilter.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="DiskSpaceQuota" Src="DiskSpaceQuota.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="SecurityState" Src="SecurityState.ascx" %>
<%@ Register TagPrefix="iFolder" TagName="Sharing" Src="Sharing.ascx" %>

<div id="policynav">

	<div class="pagetitle">
	
		<%= GetString( "POLICIES" ) %>
		
	</div>
	
	<ifolder:AccountEnabled ID="AccountEnabled" Runat="server" />
	
	<ifolder:iFolderEnabled ID="iFolderEnabled" Runat="server" />

	<ifolder:iFolderLimit ID="iFolderLimit" Runat="server" />
	
	<ifolder:DiskSpaceQuota ID="DiskQuota" Runat="server" />
	
	<ifolder:FileSizeFilter ID="FileSize" Runat="server" />
	
	<ifolder:FileTypeFilter ID="FileType" Runat="server" />
	
	<ifolder:SyncInterval ID="SyncInterval" Runat="server" />
	
	<ifolder:SecurityState ID="SecurityState" Runat="server" />

	<ifolder:Sharing ID="Sharing" Runat="server" />

	<table>
	
		<tr>
			<td>
				<asp:Button ID="PolicyApplyButton" Runat="server" CssClass="ifoldersavebutton" />
			</td>
			
			<td>
				<asp:Button ID="PolicyCancelButton" Runat="server" CssClass="ifolderbuttons" />
			</td>
		</tr>
		
	</table>
	
</div>
