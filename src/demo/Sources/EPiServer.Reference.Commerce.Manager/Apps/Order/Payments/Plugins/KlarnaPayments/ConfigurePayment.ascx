<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ConfigurePayment.ascx.cs" Inherits="Klarna.Payments.CommerceManager.Apps.Order.Payments.Plugins.KlarnaPayments.ConfigurePayment" %>

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
            <td class="FormLabelCell">Is production:</td>
            <td class="FormFieldCell">
                <asp:CheckBox runat="server" ID="IsProductionCheckBox" />
            </td>
        </tr>
    </tbody>
</table>

<h2>Klarna widget setting</h2>

<table class="DataForm">
    <tbody>
         <tr>
            <td class="FormLabelCell">Color details:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="txtColorDetails" />
            </td>
        </tr>
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
            <td class="FormLabelCell">Color checkbox checkmark:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="txtColorCheckboxCheckmark" />
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
            <td class="FormLabelCell">Color border:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="txtColorBorder" />
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">Color border selected:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="txtColorBorderSelected" />
            </td>
        </tr>
         <tr>
            <td class="FormLabelCell">Color text:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="txtColorText" />
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">Color text secondary:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="txtColorTextSecondary" />
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">Color radius border:</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="txtRadiusBorder" />
            </td>
        </tr>
    </tbody>
</table>