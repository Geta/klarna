using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Commerce.Order;
using Mediachase.Commerce.Orders;
using Mediachase.Commerce.Plugins.Payment;
using EPiServer.Logging;
using Klarna.OrderManagement.Steps;
using Klarna.Payments.Steps;

namespace Klarna.Payments
{
    public class KlarnaPaymentGateway : ISplitPaymentGateway, ISplitPaymentPlugin, IPaymentPlugin
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(KlarnaPaymentGateway));
        private IDictionary<string, string> _configData;
        private IOrderForm _orderForm;
        private IOrderGroup _orderGroup;
        private IShipment _shipment;

        /// <summary>
        /// Returns the configuration data associated with a gateway.
        /// Sets the configuration gateway data. This data typically includes
        /// information like gateway URL, account info and so on.
        /// </summary>
        /// <value>The settings.</value>
        public virtual IDictionary<string, string> Settings
        {
            get
            {
                return this._configData;
            }
            set
            {
                this._configData = value;
            }
        }

        public bool ProcessPayment(Payment payment, ref string message)
        {
            return ProcessPayment(payment as IPayment, ref message);
        }

        public bool ProcessPayment(IPayment payment, ref string message)
        {
            Logger.Error("KlarnaPaymentGateway called as IPaymentGateway instead of ISplitPaymentGateway");

            if (_orderGroup == null)
            {
                _orderGroup = (payment as Payment)?.Parent.Parent;
            }
            if (_orderForm == null)
            {
                _orderForm = (payment as Payment)?.Parent ?? _orderGroup?.Forms.FirstOrDefault(form => form.Payments.Contains(payment));
            }

            _shipment = _orderForm.Shipments.FirstOrDefault();

            return ProcessPayment(payment, _shipment, ref message);
        }

        public bool ProcessPayment(Payment payment, Shipment shipment, ref string message)
        {
            return ProcessPayment(payment as IPayment, shipment, ref message);
        }

        public bool ProcessPayment(IPayment payment, IShipment shipment, ref string message)
        {
            _shipment = shipment;

            if (_orderGroup == null)
            {
                _orderGroup = (payment as Payment)?.Parent.Parent;
            }
            if (_orderForm == null)
            {
                _orderForm = (payment as Payment)?.Parent ?? _orderGroup?.Forms.FirstOrDefault(form => form.Payments.Contains(payment));
            }

            try
            {
                Logger.Debug("Klarna checkout gateway. Processing Payment ....");

                var authorizePaymentStep = new AuthorizePaymentStep(payment, _orderGroup.Market.MarketId);
                var cancelPaymentStep = new CancelPaymentStep(payment, _orderGroup.Market.MarketId);
                var capturePaymentStep = new CapturePaymentStep(payment, _orderGroup.Market.MarketId);
                var creditPaymentStep = new CreditPaymentStep(payment, _orderGroup.Market.MarketId);
                var releaseRemainingPaymentStep = new ReleaseRemainingPaymentStep(payment, _orderGroup.Market.MarketId);

                authorizePaymentStep.SetSuccessor(cancelPaymentStep);
                cancelPaymentStep.SetSuccessor(capturePaymentStep);
                capturePaymentStep.SetSuccessor(creditPaymentStep);
                creditPaymentStep.SetSuccessor(releaseRemainingPaymentStep);

                return authorizePaymentStep.Process(payment, _orderForm, _orderGroup, _shipment, ref message);
            }
            catch (Exception ex)
            {
                Logger.Error("Process payment failed with error: " + ex.Message, ex);
                message = ex.Message;
                throw;
            }
        }
    }
}
