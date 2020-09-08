using System;
using System.Linq;
using EPiServer.Commerce.Order;
using EPiServer.ServiceLocation;
using Klarna.Common;
using Mediachase.Commerce.Orders.Managers;
using Mediachase.Web.Console.Common;
using Newtonsoft.Json;

namespace Klarna.OrderManagement.CommerceManager.KlarnaSummary
{
    public partial class KlarnaPaymentControl : System.Web.UI.UserControl
    {
        public Guid EntityId
        {
            get
            {
                var empty = Guid.Empty;
                if (base.Request["ObjectId"] != null)
                {
                    empty = new Guid(Request["ObjectId"]);
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
        private Injected<KlarnaOrderServiceFactory> _klarnaOrderServiceFactory;

        protected void Page_Load(object sender, EventArgs e)
        {
            var purchaseOrder = _orderRepository.Service.Load<IPurchaseOrder>(OrderGroupId);
            var firstPayment = purchaseOrder?.GetFirstForm().Payments.FirstOrDefault();
            if (firstPayment == null) return;

            var paymentMethod = PaymentManager.GetPaymentMethod(firstPayment.PaymentMethodId);

            if (paymentMethod?.PaymentMethod.FirstOrDefault() != null
                && paymentMethod.PaymentMethod.First().SystemKeyword.Contains("Klarna"))
            {
                var klarnaOrderService = _klarnaOrderServiceFactory.Service.Create(paymentMethod, purchaseOrder.MarketId);

                var orderId = purchaseOrder.Properties[Constants.KlarnaOrderIdField]?.ToString();

                try
                {
                    var orderData = AsyncHelper.RunSync(() => klarnaOrderService.GetOrder(orderId));

                    OrderIdLabel.Text = orderData.OrderId;
                    KlarnaReferenceLabel.Text = orderData.KlarnaReference;
                    MerchantReference1Label.Text = orderData.MerchantReference1;
                    MerchantReference2Label.Text = orderData.MerchantReference2;
                    ExpiresAtLabel.Text = orderData.ExpiresAt;
                    StatusLabel.Text = orderData.Status.ToString();
                    OrderAmountLabel.Text = GetAmount(orderData.OrderAmount);
                    CapturedAmountLabel.Text = GetAmount(orderData.CapturedAmount);
                    RefundedAmountLabel.Text = GetAmount(orderData.RefundedAmount);

                    preLabel.InnerText = JsonConvert.SerializeObject(orderData, Formatting.Indented);
                }
                catch (Exception)
                {
                    OrderInfoErrorPanel.Visible = true;
                    OrderInfoPanel.Visible = false;
                }
            }
            else
            {
                Visible = false;
            }
        }

        private string GetAmount(int? amount)
        {
            return amount.HasValue ? ((decimal)amount.Value / 100).ToString("#.##") : string.Empty;
        }
    }
}