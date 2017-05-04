<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="KlarnaPaymentControl.ascx.cs" Inherits="Klarna.OrderManagement.CommerceManager.KlarnaSummary.KlarnaPaymentControl" %>

<table cellpadding="0" cellspacing="0" width="100%">
	<tr>
		<td style="padding: 5px;width:300px;" valign="top">
            <table cellspacing="0" cellpadding="0" style="border-width: 0px; width: 100%; border-collapse: collapse; background-color: transparent;">
					<tbody><tr>
						<td valign="bottom" style="background-color: transparent;"><img src="/Apps/MetaDataBase/images/leftCorner.GIF" width="11" height="20"></td><td style="background-color: transparent;"><table cellpadding="0" cellspacing="0" border="0" class="ibn-toolbar-light" style="background-color: transparent;"><tbody><tr><td nowrap="nowrap" style="padding-right: 5px; padding-left: 5px; font-weight: bold; background-color: transparent;">Klarna order information</td></tr></tbody></table></td><td background="/Apps/MetaDataBase/images/linehz.GIF" width="100%" style="background-repeat: repeat-x; background-position-y: 100%; background-color: transparent;"></td><td valign="bottom" style="background-color: transparent;"><img src="/Apps/MetaDataBase/images/rightCorner.GIF" width="11" height="20"></td>
					</tr>
				</tbody>
            </table>
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