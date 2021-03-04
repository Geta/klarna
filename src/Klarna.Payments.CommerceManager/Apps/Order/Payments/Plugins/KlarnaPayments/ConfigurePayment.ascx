<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ConfigurePayment.ascx.cs" Inherits="Klarna.Payments.CommerceManager.Apps.Order.Payments.Plugins.KlarnaPayments.ConfigurePayment" %>

 <asp:UpdatePanel UpdateMode="Conditional" ID="ConfigureUpdatePanelContentPanel" runat="server" RenderMode="Inline" ChildrenAsTriggers="true">
        <ContentTemplate>
        <style>
            .karnapayment-parameters table.DataForm tbody tr td.FormLabelCell { width: 170px; }
            .karnapayment-parameters h2 { margin-top: 20px }
            .karnapayment-parameters-url { width: 500px; }
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
                        <asp:TextBox runat="server" ID="txtApiUrl" CssClass="karnapayment-parameters-url" />
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
                        <asp:TextBox runat="server" ID="txtKlarnaLogoUrl" CssClass="karnapayment-parameters-url" />
                    </td>
                </tr>
                 <tr>
                    <td class="FormLabelCell">Color details:</td>
                    <td class="FormFieldCell">
                        <asp:TextBox runat="server" ID="txtColorDetails" />
                        <asp:RegularExpressionValidator  ControlToValidate="txtColorDetails" ID="regexColorDetails" ValidationExpression="^#([A-Fa-f0-9]{6})$" runat="server" ErrorMessage="Color details invalid color"></asp:RegularExpressionValidator>
                    </td>
                </tr>
                <tr>
                    <td class="FormLabelCell">Color button:</td>
                    <td class="FormFieldCell">
                        <asp:TextBox runat="server" ID="txtColorButton" />
                        <asp:RegularExpressionValidator  ControlToValidate="txtColorButton" ID="regexColorButton" ValidationExpression="^#([A-Fa-f0-9]{6})$" runat="server" ErrorMessage="Color invalid color"></asp:RegularExpressionValidator>
                    </td>
                </tr>
                <tr>
                    <td class="FormLabelCell">Color button text:</td>
                    <td class="FormFieldCell">
                        <asp:TextBox runat="server" ID="txtColorButtonText" />
                        <asp:RegularExpressionValidator  ControlToValidate="txtColorButtonText" ID="regexColorButtonText" ValidationExpression="^#([A-Fa-f0-9]{6})$" runat="server" ErrorMessage="Color checkbox invalid color"></asp:RegularExpressionValidator>
                    </td>
                </tr>
                <tr>
                    <td class="FormLabelCell">Color checkbox:</td>
                    <td class="FormFieldCell">
                        <asp:TextBox runat="server" ID="txtColorCheckbox" />
                        <asp:RegularExpressionValidator  ControlToValidate="txtColorCheckbox" ID="regexColorCheckbox" ValidationExpression="^#([A-Fa-f0-9]{6})$" runat="server" ErrorMessage="Color checkbox invalid color"></asp:RegularExpressionValidator>
                    </td>
                </tr>
                <tr>
                    <td class="FormLabelCell">Color checkbox checkmark:</td>
                    <td class="FormFieldCell">
                        <asp:TextBox runat="server" ID="txtColorCheckboxCheckmark" />
                        <asp:RegularExpressionValidator  ControlToValidate="txtColorCheckboxCheckmark" ID="regexColorCheckboxCheckmark" ValidationExpression="^#([A-Fa-f0-9]{6})$" runat="server" ErrorMessage="Color checkbox checkmark invalid color"></asp:RegularExpressionValidator>
                    </td>
                </tr>
                <tr>
                    <td class="FormLabelCell">Color header:</td>
                    <td class="FormFieldCell">
                        <asp:TextBox runat="server" ID="txtColorHeader" />
                        <asp:RegularExpressionValidator  ControlToValidate="txtColorHeader" ID="regexColorHeader" ValidationExpression="^#([A-Fa-f0-9]{6})$" runat="server" ErrorMessage="Color header invalid color"></asp:RegularExpressionValidator>
                    </td>
                </tr>
                <tr>
                    <td class="FormLabelCell">Color link:</td>
                    <td class="FormFieldCell">
                        <asp:TextBox runat="server" ID="txtColorLink" />
                        <asp:RegularExpressionValidator  ControlToValidate="txtColorLink" ID="regexColorLink" ValidationExpression="^#([A-Fa-f0-9]{6})$" runat="server" ErrorMessage="Color link invalid color"></asp:RegularExpressionValidator>
                    </td>
                </tr>
                <tr>
                    <td class="FormLabelCell">Color border:</td>
                    <td class="FormFieldCell">
                        <asp:TextBox runat="server" ID="txtColorBorder" />
                        <asp:RegularExpressionValidator  ControlToValidate="txtColorBorder" ID="regexColorBorder" ValidationExpression="^#([A-Fa-f0-9]{6})$" runat="server" ErrorMessage="Color border invalid color"></asp:RegularExpressionValidator>
                    </td>
                </tr>
                <tr>
                    <td class="FormLabelCell">Color border selected:</td>
                    <td class="FormFieldCell">
                        <asp:TextBox runat="server" ID="txtColorBorderSelected" />
                        <asp:RegularExpressionValidator  ControlToValidate="txtColorBorderSelected" ID="regexColorBorderSelected" ValidationExpression="^#([A-Fa-f0-9]{6})$" runat="server" ErrorMessage="Color border selected invalid color"></asp:RegularExpressionValidator>
                    </td>
                </tr>
                 <tr>
                    <td class="FormLabelCell">Color text:</td>
                    <td class="FormFieldCell">
                        <asp:TextBox runat="server" ID="txtColorText" />
                        <asp:RegularExpressionValidator  ControlToValidate="txtColorText" ID="regexColorText" ValidationExpression="^#([A-Fa-f0-9]{6})$" runat="server" ErrorMessage="Color text invalid color"></asp:RegularExpressionValidator>
                    </td>
                </tr>
                <tr>
                    <td class="FormLabelCell">Color text secondary:</td>
                    <td class="FormFieldCell">
                        <asp:TextBox runat="server" ID="txtColorTextSecondary" />
                        <asp:RegularExpressionValidator  ControlToValidate="txtColorTextSecondary" ID="regexColorTextSecondary" ValidationExpression="^#([A-Fa-f0-9]{6})$" runat="server" ErrorMessage="Color text secondary invalid color"></asp:RegularExpressionValidator>
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
                        <asp:TextBox runat="server" ID="txtConfirmationUrl" CssClass="karnapayment-parameters-url" />
                        <asp:RequiredFieldValidator ID="requiredConfirmationUrl" runat="server" ControlToValidate="txtConfirmationUrl" ErrorMessage="Confirmation URL is required." />
                    </td>
                </tr>
                <tr>
                    <td class="FormLabelCell">Notification URL:</td>
                    <td class="FormFieldCell">
                        <asp:TextBox runat="server" ID="txtNotificationUrl" CssClass="karnapayment-parameters-url" />
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
            <tr>
                <td class="FormLabelCell">Auto capturing:</td>
                <td class="FormFieldCell">
                    <asp:CheckBox runat="server" ID="AutoCaptureCheckBox" />
                </td>
            </tr>
            </tbody>
        </table>
        </div>
    </ContentTemplate>
 </asp:UpdatePanel>