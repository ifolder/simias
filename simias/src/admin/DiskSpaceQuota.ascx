<%@ Control Language="c#" AutoEventWireup="false" Codebehind="DiskSpaceQuota.ascx.cs" Inherits="Novell.iFolderWeb.Admin.DiskSpaceQuota" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>

<div id="quotanav">
	<div class="policytitle"><%= GetString( "DISKQUOTA" ) %></div>
	<div class="policydetails">
		<asp:CheckBox ID="QuotaEnabled" Runat="server" AutoPostBack="True" />
		<div class="policylimit"><%= GetString( "LIMITTAG" ) %></div>
		<asp:TextBox ID="QuotaLimit" Runat="server" CssClass="policytextbox" AutoPostBack="true" />
		<div class="policyunits"><%= GetString( "MB" ) %></div>
		<table id="QuotaTable" runat="server" class="PolicyTable">
			<tr>
				<th>
					<%= GetString( "USEDTAG" ) %>
				</th>
				<td><asp:Literal ID="QuotaUsed" Runat="server" /></td>
			</tr>
			<tr>
				<th>
					<%= GetString( "AVAILABLETAG" ) %>
				</th>
				<td><asp:Literal ID="QuotaAvailable" Runat="server" /></td>
			</tr>
			<tr>
				<th>
					<asp:Literal ID="QuotaEffectiveHeader" Runat="server" />
				</th>
				<td>
					<asp:Literal ID="QuotaEffective" Runat="server" />
				</td>
			</tr>
		</table>
	</div>
</div>

