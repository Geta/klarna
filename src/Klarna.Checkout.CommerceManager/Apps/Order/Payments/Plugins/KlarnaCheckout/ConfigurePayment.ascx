<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ConfigurePayment.ascx.cs" Inherits="Klarna.Checkout.CommerceManager.Apps.Order.Payments.Plugins.KlarnaCheckout.ConfigurePayment" %>
<%@ Register TagPrefix="mc" Namespace="Mediachase.BusinessFoundation" Assembly="Mediachase.BusinessFoundation, Version=10.4.3.0, Culture=neutral, PublicKeyToken=41d2e7a615ba286c" %>

<style>
    .karnapayment-parameters table.DataForm tbody tr td.FormLabelCell { width: 200px; }
    .karnapayment-parameters h2 { margin-top: 20px }
</style>

<div class="karnapayment-parameters">
    
<h2>Klarna connection setting</h2>


<table class="DataForm">
    <tbody>
         <tr>
            <td class="FormLabelCell">Username:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="txtUsername" />
                <asp:RequiredFieldValidator ID="requiredUsername" runat="server" ControlToValidate="txtUsername" ErrorMessage="Username is required." />
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">Password:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="txtPassword"  />
                <asp:RequiredFieldValidator ID="requiredPassword" runat="server" ControlToValidate="txtPassword" ErrorMessage="Password is required." />
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">ApiUrl:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="txtApiUrl"  />
                <asp:RequiredFieldValidator ID="requiredApiUrl" runat="server" ControlToValidate="txtApiUrl" ErrorMessage="Api URL is required." />
            </td>
        </tr>
    </tbody>
</table>
</div>