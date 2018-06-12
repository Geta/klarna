using EPiServer.Commerce.Order;
using Klarna.Common.Extensions;
using Klarna.Payments.Models;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;

namespace Klarna.OrderManagement.Steps
{
    public abstract class AuthorizePaymentStepBase : PaymentStep
    {
        protected AuthorizePaymentStepBase(IPayment payment, MarketId marketId, KlarnaOrderServiceFactory klarnaOrderServiceFactory)
            : base(payment, marketId, klarnaOrderServiceFactory)
        {
        }

        public override bool Process(IPayment payment, IOrderForm orderForm, IOrderGroup orderGroup, IShipment shipment, ref string message)
        {
            if (payment.TransactionType != TransactionType.Authorization.ToString())
            {
                return Successor != null && Successor.Process(payment, orderForm, orderGroup, shipment, ref message);
            }

            if (ShouldProcessFraudUpdate(payment, orderGroup))
            {
                return ProcessFraudUpdate(payment, orderGroup, ref message);
            }

            return ProcessAuthorization(payment, orderGroup, ref message);
        }

        private static bool ShouldProcessFraudUpdate(IPayment payment, IOrderGroup orderGroup)
        {
            return payment.Status == PaymentStatus.Pending.ToString() && orderGroup is IPurchaseOrder;
        }

        private bool ProcessFraudUpdate(IPayment payment, IOrderGroup orderGroup, ref string message)
        {
            if (payment.HasFraudStatus(NotificationFraudStatus.FRAUD_RISK_ACCEPTED))
            {
                payment.Status = PaymentStatus.Processed.ToString();

                OrderStatusManager.ReleaseHoldOnOrder((PurchaseOrder)orderGroup);

                AddNoteAndSaveChanges(orderGroup, payment.TransactionType, "Klarna fraud risk accepted");

                return true;
            }

            if (payment.HasFraudStatus(NotificationFraudStatus.FRAUD_RISK_REJECTED))
            {
                payment.Status = PaymentStatus.Failed.ToString();

                AddNoteAndSaveChanges(orderGroup, payment.TransactionType, "Klarna fraud risk rejected");

                return false;
            }

            if (payment.HasFraudStatus(NotificationFraudStatus.FRAUD_RISK_STOPPED))
            {
                payment.Status = PaymentStatus.Failed.ToString();

                AddNoteAndSaveChanges(orderGroup, payment.TransactionType, "Klarna fraud risk stopped");

                return false;
            }

            message = $"Can't process authorization. Unknown fraud notitication: {payment.Properties[Common.Constants.FraudStatusPaymentField]} or no fraud notifications received so far.";
            return false;
        }

        public abstract bool ProcessAuthorization(IPayment payment, IOrderGroup orderGroup, ref string message);
    }
}
