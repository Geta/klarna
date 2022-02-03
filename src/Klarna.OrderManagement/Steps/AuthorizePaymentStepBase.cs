using System.Threading.Tasks;
using EPiServer.Commerce.Order;
using Klarna.Common.Extensions;
using Klarna.Common.Models;
using Mediachase.Commerce;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Orders.Managers;

namespace Klarna.OrderManagement.Steps
{
    public abstract class AuthorizePaymentStepBase : PaymentStep
    {
        protected AuthorizePaymentStepBase(IPayment payment, MarketId marketId, IKlarnaOrderServiceFactory klarnaOrderServiceFactory)
            : base(payment, marketId, klarnaOrderServiceFactory)
        {
        }

        public override async Task<PaymentStepResult> Process(IPayment payment, IOrderForm orderForm, IOrderGroup orderGroup, IShipment shipment)
        {
            if (payment.TransactionType != TransactionType.Authorization.ToString())
            {
                var paymentStepResult = await Successor.Process(payment, orderForm, orderGroup, shipment).ConfigureAwait(false);
                paymentStepResult.Status = Successor != null && paymentStepResult.Status;
                
                return paymentStepResult;
            }

            if (ShouldProcessFraudUpdate(payment, orderGroup))
            {
                return ProcessFraudUpdate(payment, orderGroup);
            }

            return await ProcessAuthorization(payment, orderGroup).ConfigureAwait(false);
        }

        private static bool ShouldProcessFraudUpdate(IPayment payment, IOrderGroup orderGroup)
        {
            if (!(orderGroup is IPurchaseOrder)) return false;

            var isPending = payment.Status == PaymentStatus.Pending.ToString();
            var isFraudStopped = orderGroup.OrderStatus != OrderStatus.Completed
                                 && payment.HasFraudStatus(NotificationFraudStatus.FRAUD_RISK_STOPPED);
            return isPending || isFraudStopped;
        }

        private PaymentStepResult ProcessFraudUpdate(IPayment payment, IOrderGroup orderGroup)
        {
            var paymentStepResult = new PaymentStepResult();

            if (payment.HasFraudStatus(NotificationFraudStatus.FRAUD_RISK_ACCEPTED))
            {
                payment.Status = PaymentStatus.Processed.ToString();
                OrderStatusManager.ReleaseHoldOnOrder((PurchaseOrder)orderGroup);
                AddNoteAndSaveChanges(orderGroup, payment.TransactionType, "Klarna fraud risk accepted");
                paymentStepResult.Status = true;
                return paymentStepResult;
            }

            if (payment.HasFraudStatus(NotificationFraudStatus.FRAUD_RISK_REJECTED))
            {
                payment.Status = PaymentStatus.Failed.ToString();
                OrderStatusManager.HoldOrder((PurchaseOrder)orderGroup);
                AddNoteAndSaveChanges(orderGroup, payment.TransactionType, "Klarna fraud risk rejected");
                paymentStepResult.Status = true;
                return paymentStepResult;
            }

            if (payment.HasFraudStatus(NotificationFraudStatus.FRAUD_RISK_STOPPED))
            {
                payment.Status = PaymentStatus.Failed.ToString();
                OrderStatusManager.HoldOrder((PurchaseOrder)orderGroup);
                AddNoteAndSaveChanges(orderGroup, payment.TransactionType, "Klarna fraud risk stopped");
                paymentStepResult.Status = true;
                return paymentStepResult;
            }

            paymentStepResult.Message = $"Can't process authorization. Unknown fraud notification: {payment.Properties[Common.Constants.FraudStatusPaymentField]} or no fraud notifications received so far.";
            
            return paymentStepResult;
        }

        public abstract Task<PaymentStepResult> ProcessAuthorization(IPayment payment, IOrderGroup orderGroup);
    }
}
