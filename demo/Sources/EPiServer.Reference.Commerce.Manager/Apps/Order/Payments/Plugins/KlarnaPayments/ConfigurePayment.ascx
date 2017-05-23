<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ConfigurePayment.ascx.cs" Inherits="Klarna.Payments.CommerceManager.Apps.Order.Payments.Plugins.KlarnaPayments.ConfigurePayment" %>
<%@ Register TagPrefix="mc" Namespace="Mediachase.BusinessFoundation" Assembly="Mediachase.BusinessFoundation, Version=10.4.3.0, Culture=neutral, PublicKeyToken=41d2e7a615ba286c" %>

 <asp:UpdatePanel UpdateMode="Conditional" ID="ConfigureUpdatePanelContentPanel" runat="server" RenderMode="Inline" ChildrenAsTriggers="true">
        <ContentTemplate>
        <style>
            .karnapayment-parameters table.DataForm tbody tr td.FormLabelCell { width: 200px; }
            .karnapayment-parameters h2 { margin-top: 20px }
        </style>

        <div class="karnapayment-parameters">
            
            <h2>Market</h2>
    
            <table class="DataForm">
                <tbody>
                     <tr>
                        <td class="FormLabelCell">Select a market:</td>
                        <td class="FormFieldCell">
                            <asp:DropDownList runat="server" ID="marketDropDownList" OnSelectedIndexChanged="marketDropDownList_OnSelectedIndexChanged" AutoPostBack="True" />
                        </td>
                    </tr>
                </tbody>
            </table>
    
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
                    <td class="FormLabelCell">Klarna logo url:</td>
                    <td class="FormFieldCell">
                        <asp:TextBox runat="server" ID="txtKlarnaLogoUrl" />
                    </td>
                </tr>
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
                    <td class="FormLabelCell">Notification URL:</td>
                    <td class="FormFieldCell">
                        <asp:TextBox runat="server" ID="txtNotificationUrl" />
                        <asp:RequiredFieldValidator ID="requiredNotificationUrl" runat="server" ControlToValidate="txtNotificationUrl" ErrorMessage="Notification URL is required." />
                    </td>
                </tr>
                <tr>
                    <td class="FormLabelCell">Send product and image URL:</td>
                    <td class="FormFieldCell">
                        <asp:CheckBox runat="server" ID="SendProductAndImageUrlCheckBox" />
                    </td>
                </tr>
                 <tr>
                    <td class="FormLabelCell">Use attachments:</td>
                    <td class="FormFieldCell">
                        <asp:CheckBox runat="server" ID="UseAttachmentsCheckBox" />
                    </td>
                </tr>
                <tr>
                    <td class="FormLabelCell">Pre-assesment:</td>
                    <td class="FormFieldCell">
                        <asp:CheckBox runat="server" ID="PreAssesmentCheckBox" />
                    </td>
                </tr>
            </tbody>
        </table>
        </div>
    </ContentTemplate>
 </asp:UpdatePanel>