<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="KlarnaPaymentControl.ascx.cs" Inherits="EPiServer.Reference.Commerce.Manager.KlarnaPaymentControl" %>
<%@ Register TagPrefix="mc2" Namespace="Mediachase.Commerce.Manager.Apps.Common.Design"
	Assembly="Mediachase.ConsoleManager" %>
<%@ Register src="~/Apps/Core/Controls/ButtonsHolder.ascx" tagname="ButtonsHolder" tagprefix="uc1" %>

<table cellpadding="0" cellspacing="0" width="100%">
	<tr>
		<td style="padding: 5px;width:300px;" valign="top">
            <mc2:BlockHeaderLight HeaderCssClass="ibn-toolbar-light" ID="bhl" runat="server"
				            Title="Klarna order information"></mc2:BlockHeaderLight>
	        <table class="orderform-blockheaderlight-datatable" style="background-color: transparent;">
		        <tbody>
                    <tr>
				        <td valign="top" font-weight: bold; text-align: right; background-color: transparent;">
                            <pre runat="server" id="preLabel"></pre>
				        </td>
			        </tr>
		        </tbody>
	        </table>
        </td>
    </tr>
</table>