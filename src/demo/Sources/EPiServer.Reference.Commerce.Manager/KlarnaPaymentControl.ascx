<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="KlarnaPaymentControl.ascx.cs" Inherits="EPiServer.Reference.Commerce.Manager.KlarnaPaymentControl" %>
<%@ Register TagPrefix="mc2" Namespace="Mediachase.Commerce.Manager.Apps.Common.Design"
	Assembly="Mediachase.ConsoleManager" %>
<%@ Register src="~/Apps/Core/Controls/ButtonsHolder.ascx" tagname="ButtonsHolder" tagprefix="uc1" %>

<table cellpadding="0" cellspacing="0" width="100%">
	<tr>
		<td style="padding: 5px;width:300px;" valign="top">
            <mc2:BlockHeaderLight HeaderCssClass="ibn-toolbar-light" ID="bhl" runat="server" Title="Klarna order information"></mc2:BlockHeaderLight>
	        <table class="orderform-blockheaderlight-datatable" style="background-color: transparent;">
		        <tbody>
                    <tr>
				        <td class="custom-label-cell"><span class="cell-label">Order id:</span></td>
                        <td class="custom-value-cell" style="background-color: transparent;">
                            <asp:Label runat="server" ID="OrderIdLabel" CssClass="cell-label"></asp:Label>
					    </td>
			        </tr>
                    <tr>
				        <td class="custom-label-cell"><span class="cell-label">Klarna reference:</span></td>
                        <td class="custom-value-cell" style="background-color: transparent;">
                            <asp:Label runat="server" ID="KlarnaReferenceLabel" CssClass="cell-label"></asp:Label>
					    </td>
			        </tr>
                    <tr>
				        <td class="custom-label-cell"><span class="cell-label">Merchant reference 1:</span></td>
                        <td class="custom-value-cell" style="background-color: transparent;">
                            <asp:Label runat="server" ID="MerchantReference1Label" CssClass="cell-label"></asp:Label>
					    </td>
			        </tr>
                    <tr>
				        <td class="custom-label-cell"><span class="cell-label">Merchant reference 2:</span></td>
                        <td class="custom-value-cell" style="background-color: transparent;">
                            <asp:Label runat="server" ID="MerchantReference2Label" CssClass="cell-label"></asp:Label>
					    </td>
			        </tr>
                    <tr>
				        <td class="custom-label-cell"><span class="cell-label">Expires at:</span></td>
                        <td class="custom-value-cell" style="background-color: transparent;">
                            <asp:Label runat="server" ID="ExpiresAtLabel" CssClass="cell-label"></asp:Label>
					    </td>
			        </tr>
                    <tr>
				        <td class="custom-label-cell"><span class="cell-label">Status:</span></td>
                        <td class="custom-value-cell" style="background-color: transparent;">
                            <asp:Label runat="server" ID="StatusLabel" CssClass="cell-label"></asp:Label>
					    </td>
			        </tr>
                    <tr>
				        <td class="custom-label-cell"><span class="cell-label">Order amount:</span></td>
                        <td class="custom-value-cell" style="background-color: transparent;">
                            <asp:Label runat="server" ID="OrderAmountLabel" CssClass="cell-label"></asp:Label>
					    </td>
			        </tr>
                    <tr>
				        <td class="custom-label-cell"><span class="cell-label">Captured amount:</span></td>
                        <td class="custom-value-cell" style="background-color: transparent;">
                            <asp:Label runat="server" ID="CapturedAmountLabel" CssClass="cell-label"></asp:Label>
					    </td>
			        </tr>
                     <tr>
				        <td class="custom-label-cell"><span class="cell-label">Refunded amount:</span></td>
                        <td class="custom-value-cell" style="background-color: transparent;">
                            <asp:Label runat="server" ID="RefundedAmountLabel" CssClass="cell-label"></asp:Label>
					    </td>
			        </tr>

                    <tr>
                        <td colspan="2">
                            <a id="showOrderInformationLink" href="#">Show all order information</a>
                            <pre runat="server" id="preLabel" style="display: none" ClientIDMode="Static"></pre>
                        </td>
                    </tr>
		        </tbody>
	        </table>
        </td>
    </tr>
</table>

<script>
    $(document).ready(function () {
        var preLabel = $("#preLabel");
        var showOrderInformationLink = $("#showOrderInformationLink");

        $(showOrderInformationLink).click(function () {
            if ($(preLabel).css("display") == "none") {
                $(preLabel).css("display", "block");
                $(showOrderInformationLink).text("Hide all order information");
            } else {
                $(preLabel).css("display", "none");
                $(showOrderInformationLink).text("Show all order information");
            }
        });
    });
</script>

<style>
    #showOrderInformationLink { text-decoration: none;}
    #showOrderInformationLink:hover { text-decoration: underline;}
    #preLabel { padding: 10px; }
    span.cell-label { color: rgb(29, 29, 29); }
    td.custom-label-cell { font-weight: bold; text-align: left; background-color: transparent;vertical-align: top;width: 130px; }
    td.custom-value-cell { background-color: transparent; vertical-align: top}
</style>