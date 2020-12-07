using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Commerce.Order;
using Mediachase.Commerce.Orders;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using Klarna.Common;
using Klarna.Common.Models;
using Klarna.OrderManagement;
using Klarna.OrderManagement.Steps;
using Klarna.Payments.Steps;

namespace Klarna.Payments
{
    public class KlarnaPaymentGateway : ISplitPaymentGateway, ISplitPaymentPlugin, IPaymentPlugin
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(KlarnaPaymentGateway));
        private IDictionary<string, string> _configData;
        private IOrderForm _orderForm;
        private IShipment _shipment;

        internal Injected<KlarnaOrderServiceFactory> InjectedKlarnaOrderServiceFactory { get; set; }
        private KlarnaOrderServiceFactory KlarnaOrderServiceFactory => InjectedKlarnaOrderServiceFactory.Service;

        internal Injected<IKlarnaPaymentsService> InjectedKlarnaPaymentsService { get; set; }
        private IKlarnaPaymentsService KlarnaPaymentsService => InjectedKlarnaPaymentsService.Service;

        public IOrderGroup OrderGroup { get; set; }

        /// <summary>
        /// Returns the configuration data associated with a gateway.
        /// Sets the configuration gateway data. This data typically includes
        /// information like gateway URL, account info and so on.
        /// </summary>
        /// <value>The settings.</value>
        public virtual IDictionary<string, string> Settings
        {
            get => _configData;
            set => _configData = value;
        }

        public PaymentProcessingResult ProcessPayment(IOrderGroup orderGroup, IPayment payment, IShipment shipment)
        {
            OrderGroup = orderGroup;
            _orderForm = orderGroup.GetFirstForm();
            var result = ProcessPayment(payment, shipment);
            var message = result.Message;
            return result.Status
                ? PaymentProcessingResult.CreateSuccessfulResult(message)
                : PaymentProcessingResult.CreateUnsuccessfulResult(message);
        }

        public PaymentProcessingResult ProcessPayment(IOrderGroup orderGroup, IPayment payment)
        {
            OrderGroup = orderGroup;
            _orderForm = orderGroup.GetFirstForm();
            var message = string.Empty;
            return ProcessPayment(payment, ref message)
                ? PaymentProcessingResult.CreateSuccessfulResult(message)
                : PaymentProcessingResult.CreateUnsuccessfulResult(message);
        }

        public bool ProcessPayment(Payment payment, ref string message)
        {
            return ProcessPayment(payment as IPayment, ref message);
        }

        public bool ProcessPayment(IPayment payment, ref string message)
        {
            if (_orderForm == null)
            {
                _orderForm = OrderGroup.GetFirstForm();
            }

            _shipment = _orderForm.Shipments.FirstOrDefault();

            var result = ProcessPayment(payment, _shipment);
            message = result.Message;

            return result.Status;
        }

        public bool ProcessPayment(Payment payment, Shipment shipment, ref string message)
        {
            var result = ProcessPayment(payment, shipment);
            message = result.Message;
            return result.Status;
        }

        public PaymentStepResult ProcessPayment(IPayment payment, IShipment shipment)
        {
            var paymentStepResult = new PaymentStepResult();
            _shipment = shipment;
            
            if (_orderForm == null)
            {
                _orderForm = (payment as Payment)?.Parent ?? OrderGroup?.Forms.FirstOrDefault(form => form.Payments.Contains(payment));
            }
            if (OrderGroup == null)
            {
                OrderGroup = (_orderForm as OrderForm)?.Parent;
            }

            if (OrderGroup == null)
            {
                paymentStepResult.Message = "OrderGroup is null";
                throw new Exception(paymentStepResult.Message);
            }

            try
            {
                Logger.Debug("Klarna Payment gateway. Processing Payment ....");

                var authorizePaymentStep = new AuthorizePaymentStep(payment, OrderGroup.MarketId, KlarnaOrderServiceFactory, KlarnaPaymentsService);
                var cancelPaymentStep = new CancelPaymentStep(payment, OrderGroup.MarketId, KlarnaOrderServiceFactory);
                var capturePaymentStep = new CapturePaymentStep(payment, OrderGroup.MarketId, KlarnaOrderServiceFactory);
                var creditPaymentStep = new CreditPaymentStep(payment, OrderGroup.MarketId, KlarnaOrderServiceFactory);
                var releaseRemainingPaymentStep = new ReleaseRemainingPaymentStep(payment, OrderGroup.MarketId, KlarnaOrderServiceFactory);

                authorizePaymentStep.SetSuccessor(cancelPaymentStep);
                cancelPaymentStep.SetSuccessor(capturePaymentStep);
                capturePaymentStep.SetSuccessor(creditPaymentStep);
                creditPaymentStep.SetSuccessor(releaseRemainingPaymentStep);

                return AsyncHelper.RunSync(() => authorizePaymentStep.Process(payment, _orderForm, OrderGroup, _shipment));
            }
            catch (Exception ex)
            {
                Logger.Error("Process payment failed with error: " + ex.Message, ex);
                
                paymentStepResult.Message = ex.Message;
                return paymentStepResult;
            }
        }
    }
}
