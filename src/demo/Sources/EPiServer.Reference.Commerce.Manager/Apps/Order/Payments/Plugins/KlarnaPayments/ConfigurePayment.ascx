<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ConfigurePayment.ascx.cs" Inherits="Klarna.Payments.CommerceManager.Apps.Order.Payments.Plugins.KlarnaPayments.ConfigurePayment" %>

<h2>Service setting</h2>

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
            <td class="FormLabelCell">Is production:</td>
            <td class="FormFieldCell">
                <asp:CheckBox runat="server" ID="IsProductionCheckBox" />
            </td>
        </tr>
    </tbody>
</table>