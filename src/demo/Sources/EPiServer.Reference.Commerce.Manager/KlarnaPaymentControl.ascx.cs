using System;
using System.Linq;
using EPiServer.Commerce.Order;
using EPiServer.Globalization;
using EPiServer.ServiceLocation;
using Klarna.OrderManagement;
using Klarna.Payments;
using Klarna.Payments.Extensions;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Web.Console.Common;
using Newtonsoft.Json;

namespace EPiServer.Reference.Commerce.Manager
{
    public partial class KlarnaPaymentControl : System.Web.UI.UserControl
    {
        public Guid EntityId
        {
            get
            {
                Guid empty = Guid.Empty;
                if (base.Request["ObjectId"] != null)
                {
                    empty = new Guid(base.Request["ObjectId"]);
                }
                return empty;
            }
        }

        protected int OrderGroupId
        {
            get
            {
                return ManagementHelper.GetIntFromQueryString("id");
            }
        }

        private Injected<IOrderRepository> _orderRepository;

        protected void Page_Load(object sender, EventArgs e)
        {
            var purchaseOrder = _orderRepository.Service.Load<IPurchaseOrder>(OrderGroupId);
            if (purchaseOrder != null)
            {
                var paymentMethod = PaymentManager.GetPaymentMethodBySystemName(Constants.KlarnaPaymentSystemKeyword, ContentLanguage.PreferredCulture.Name);
                if (paymentMethod != null)
                {

                    if (purchaseOrder.GetFirstForm().Payments.Any(x => x.PaymentMethodId == paymentMethod.PaymentMethod.FirstOrDefault()?.PaymentMethodId))
                    {
                        var username = paymentMethod.GetParameter(Constants.KlarnaUsernameField, string.Empty);
                        var password = paymentMethod.GetParameter(Constants.KlarnaPasswordField, string.Empty);
                        var apiUrl = paymentMethod.GetParameter(Constants.KlarnaApiUrlField, string.Empty);

                        var klarnaOrderService = new KlarnaOrderService(username, password, apiUrl);

                        var orderId = purchaseOrder.Properties[Constants.KlarnaOrderIdField]?.ToString();

                        var orderData = klarnaOrderService.GetOrder(orderId);

                        preLabel.InnerText = JsonConvert.SerializeObject(orderData, Formatting.Indented);
                    }
                    else
                    {
                        Visible = false;
                    }
                }
            }
        }
    }
}