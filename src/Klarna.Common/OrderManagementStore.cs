using System.Net.Http;
using System.Threading.Tasks;
using Klarna.Common.Helpers;
using Klarna.Common.Models;

namespace Klarna.Common
{
    /// <summary>
    /// The Order Management API is used for handling an order after the customer has completed the purchase.
    /// It is used for updating, capturing and refunding an order as well as to see the history of events that
    /// have affected this order.
    /// Endpoint for https://developers.klarna.com/api/#order-management-api-release-remaining-authorization
    /// </summary>
    public class OrderManagementStore : BaseStore
    {
        public OrderManagementStore(ApiSession apiSession, IJsonSerializer jsonSerializer) :
            base(apiSession, "ordermanagement/v1/orders", jsonSerializer)
        { }

        /// <summary>
        /// Releases remaining authorization
        /// <a href="https://developers.klarna.com/api/#order-management-api-release-remaining-authorization">
        ///     https://developers.klarna.com/api/#order-management-api-release-remaining-authorization
        /// </a>
        /// </summary>
        /// <param name="orderId">Id of order to release</param>
        /// <returns></returns>
        public Task ReleaseRemainingAuthorization(string orderId)
        {
            var url = ApiUrlHelper.GetApiUrlForController(ApiSession.ApiUrl, ApiControllerUri, $"{orderId}/release-remaining-authorization");
            return Post(url);
        }

        /// <summary>
        /// Extends authorization time
        /// <a href="https://developers.klarna.com/api/#order-management-api-extend-authorization-time">
        ///     https://developers.klarna.com/api/#order-management-api-extend-authorization-time
        /// </a>
        /// </summary>
        /// <param name="orderId">Id of order to extend</param>
        /// <returns></returns>
        public Task ExtendAuthorizationTime(string orderId)
        {
            var url = ApiUrlHelper.GetApiUrlForController(ApiSession.ApiUrl, ApiControllerUri, $"{orderId}/extend-authorization-time");
            return Post(url);
        }

        /// <summary>
        /// Updates customer addresses
        /// <a href="https://developers.klarna.com/api/#order-management-api-update-customer-addresses">
        ///     https://developers.klarna.com/api/#order-management-api-update-customer-addresses
        /// </a>
        /// </summary>
        /// <param name="orderId">Id of order to update</param>
        /// <param name="customerAddresses">The <see cref="UpdateCustomerAddresses"/>object</param>
        /// <returns></returns>
        public Task UpdateCustomerAddresses(string orderId, OrderManagementCustomerAddresses customerAddresses)
        {
            var url = ApiUrlHelper.GetApiUrlForController(ApiSession.ApiUrl, ApiControllerUri, $"{orderId}/customer-details");
            return Patch(url, customerAddresses);
        }

        /// <summary>
        /// Cancels order
        /// <a href="https://developers.klarna.com/api/#order-management-api-cancel-order">
        ///     https://developers.klarna.com/api/#order-management-api-cancel-order
        /// </a>
        /// </summary>
        /// <param name="orderId">Id of order to cancel</param>
        /// <returns></returns>
        public Task CancelOrder(string orderId)
        {
            var url = ApiUrlHelper.GetApiUrlForController(ApiSession.ApiUrl, ApiControllerUri, $"{orderId}/cancel");
            return Post(url);
        }

        /// <summary>
        /// Updates merchant references
        /// <a href="https://developers.klarna.com/api/#order-management-api-update-merchant-references">
        ///     https://developers.klarna.com/api/#order-management-api-update-merchant-references
        /// </a>
        /// </summary>
        /// <param name="orderId">Id of order to update</param>
        /// <param name="merchantReferences">The <see cref="OrderManagementMerchantReferences"/> object</param>
        /// <returns></returns>
        public Task UpdateMerchantReferences(string orderId, OrderManagementMerchantReferences merchantReferences)
        {
            var url = ApiUrlHelper.GetApiUrlForController(ApiSession.ApiUrl, ApiControllerUri, $"{orderId}/merchant-references");
            return Patch(url, merchantReferences);
        }

        /// <summary>
        /// Acknowledges order
        /// <a href="https://developers.klarna.com/api/#order-management-api-acknowledge-order">
        ///     https://developers.klarna.com/api/#order-management-api-acknowledge-order
        /// </a>
        /// </summary>
        /// <param name="orderId">Id of order to acknowledge</param>
        /// <returns></returns>
        public Task AcknowledgeOrder(string orderId)
        {
            var url = ApiUrlHelper.GetApiUrlForController(ApiSession.ApiUrl, ApiControllerUri, $"{orderId}/acknowledge");
            return Post(url);
        }

        /// <summary>
        /// Gets order
        /// <a href="https://developers.klarna.com/api/#order-management-api-get-order">
        ///     https://developers.klarna.com/api/#order-management-api-get-order
        /// </a>
        /// </summary>
        /// <param name="orderId">Id of order to retrieve</param>
        /// <returns><see cref="OrderManagementOrder"/></returns>
        public async Task<OrderManagementOrder> GetOrder(string orderId)
        {
            var url = ApiUrlHelper.GetApiUrlForController(ApiSession.ApiUrl, ApiControllerUri, $"{orderId}");
            return await Get<OrderManagementOrder>(url).ConfigureAwait(false);
        }

        /// <summary>
        /// Triggers resend of customer communication
        /// <a href="https://developers.klarna.com/api/#order-management-api-trigger-resend-of-customer-communication">
        ///     https://developers.klarna.com/api/#order-management-api-trigger-resend-of-customer-communication
        /// </a>
        /// </summary>
        /// <param name="orderId">Id of order that contains the capture</param>
        /// <param name="captureId">Id of capture to resend</param>
        /// <returns></returns>
        public Task TriggerResendOfCustomerCommunication(string orderId, string captureId)
        {
            var url = ApiUrlHelper.GetApiUrlForController(ApiSession.ApiUrl, ApiControllerUri, $"{orderId}/captures/{captureId}/trigger-send-out");
            return Post(url);
        }

        /// <summary>
        /// Creates capture and follow the Location header to fetch the data
        /// <a href="https://developers.klarna.com/api/#order-management-api-create-capture">
        ///     https://developers.klarna.com/api/#order-management-api-create-capture
        /// </a>
        /// </summary>
        /// <param name="orderId">Id of order to create capture</param>
        /// <param name="capture">The <see cref="OrderManagementCapture"/> object</param>
        /// <returns>Object of <see cref="OrderManagementCapture"/> </returns>
        public async Task<OrderManagementCapture> CreateAndFetchCapture(string orderId, OrderManagementCreateCapture capture)
        {
            var url = ApiUrlHelper.GetApiUrlForController(ApiSession.ApiUrl, ApiControllerUri, $"{orderId}/captures");
            var response = new Ref<HttpResponseMessage>();

            await Post(url, capture, null, response).ConfigureAwait(false);

            var headers = response.Value.Headers;
            url = headers.Location.ToString();

            if (!string.IsNullOrEmpty(url))
            {
                return await Get<OrderManagementCapture>(url).ConfigureAwait(false);
            }

            return default(OrderManagementCapture);
        }

        /// <summary>
        /// Creates a refund and follow the Location header to fetch the data
        /// <a href="https://developers.klarna.com/api/#order-management-api-create-a-refund">
        ///     https://developers.klarna.com/api/#order-management-api-create-a-refund
        /// </a>
        /// </summary>
        /// <param name="orderId">Id of order to create a refund</param>
        /// <param name="refund">The <see cref="OrderManagementRefund"/> object</param>
        /// <returns>Object of <see cref="OrderManagementRefund"/> </returns>
        public async Task<OrderManagementRefund> CreateAndFetchRefund(string orderId, OrderManagementRefund refund)
        {
            var url = ApiUrlHelper.GetApiUrlForController(ApiSession.ApiUrl, ApiControllerUri, $"{orderId}/refunds");

            var response = new Ref<HttpResponseMessage>();
            await Post(url, refund, null, response).ConfigureAwait(false);

            var headers = response.Value.Headers;
            url = headers.Location.ToString();

            if (!string.IsNullOrEmpty(url))
            {
                return await Get<OrderManagementRefund>(url).ConfigureAwait(false);
            }

            return default(OrderManagementRefund);
        }
    }
}
