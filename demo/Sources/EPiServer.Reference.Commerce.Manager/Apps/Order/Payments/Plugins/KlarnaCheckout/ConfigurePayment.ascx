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
    
<h2>Klarna widget setting</h2>

<table class="DataForm">
    <tbody>
        <tr>
            <td class="FormLabelCell">Color button:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="txtColorButton" />
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">Color button text:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="txtColorButtonText" />
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">Color checkbox:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="txtColorCheckbox" />
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">Color header:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="txtColorHeader" />
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">Color link:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="txtColorLink" />
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">Radius border:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="txtRadiusBorder" />
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">Color checkbox checkmark:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="txtColorCheckboxCheckmark" />
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">Allow separate shipping address:</td>
            <td class="FormFieldCell">
                <asp:CheckBox runat="server" ID="allowSeparateShippingAddressCheckBox" />
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">Date of birth mandatory:</td>
            <td class="FormFieldCell">
                <asp:CheckBox runat="server" ID="dateOfBirthMandatoryCheckBox" />
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">Shipping details:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="txtShippingDetails" />
            </td>
        </tr>
         <tr>
            <td class="FormLabelCell">Title mandatory:</td>
            <td class="FormFieldCell">
                <asp:CheckBox runat="server" ID="titleMandatoryCheckBox" />
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">Show subtotal detail:</td>
            <td class="FormFieldCell">
                <asp:CheckBox runat="server" ID="showSubtotalDetailCheckBox" />
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">Require validate callback success:</td>
            <td class="FormFieldCell">
                <asp:CheckBox runat="server" ID="requireValidateCallbackSuccessCheckBox" />
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">Additional checkbox text:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="additionalCheckboxTextTextBox" />
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">Additional checkbox default checked:</td>
            <td class="FormFieldCell">
                <asp:CheckBox runat="server" ID="additionalCheckboxDefaultCheckedCheckBox" />
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">Additional checkbox required:</td>
            <td class="FormFieldCell">
                <asp:CheckBox runat="server" ID="additionalCheckboxRequiredCheckBox" />
            </td>
        </tr>
    </tbody>
</table>
    
<h2>Other setting</h2>

<table class="DataForm">
    <tbody>
         <tr>
            <td class="FormLabelCell">Confirmation URL:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="txtConfirmationUrl" />
                <asp:RequiredFieldValidator ID="requiredConfirmationUrl" runat="server" ControlToValidate="txtConfirmationUrl" ErrorMessage="Confirmation URL is required." />
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">Terms URL:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="txtTermsUrl" />
                <asp:RequiredFieldValidator ID="requiredTermsUrl" runat="server" ControlToValidate="txtTermsUrl" ErrorMessage="Terms URL is required." />
            </td>
        </tr>
         <tr>
            <td class="FormLabelCell">Checkout URL:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="txtCheckoutUrl" />
                <asp:RequiredFieldValidator ID="requiredCheckoutUrl" runat="server" ControlToValidate="txtCheckoutUrl" ErrorMessage="Checkout URL is required." />
            </td>
        </tr>
         <tr>
            <td class="FormLabelCell">Push URL:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="txtPushUrl" />
                <asp:RequiredFieldValidator ID="requiredPushUrl" runat="server" ControlToValidate="txtPushUrl" ErrorMessage="Push URL is required." />
            </td>
        </tr>
         <tr>
            <td class="FormLabelCell">Notification URL:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="txtNotificationUrl" />
                <asp:RequiredFieldValidator ID="requiredNotificationUrl" runat="server" ControlToValidate="txtNotificationUrl" ErrorMessage="Notification URL is required." />
            </td>
        </tr>
    </tbody>
</table>
</div>