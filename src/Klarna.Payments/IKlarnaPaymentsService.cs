using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EPiServer.Commerce.Order;
using Klarna.Common;
using Klarna.Payments.Models;

namespace Klarna.Payments
{
    public interface IKlarnaPaymentsService : IKlarnaService
    {
        Task<bool> CreateOrUpdateSession(ICart cart, SessionSettings settings);
        Task<Session> GetSession(ICart cart);
        CompletionResult Complete(IPurchaseOrder purchaseOrder);
        Task<CreateOrderResponse> CreateOrder(string authorizationToken, ICart cart);
        bool AllowSharingOfPersonalInformation(ICart cart);
        PersonalInformationSession GetPersonalInformationSession(ICart cart, IDictionary<string, object> dic = null);
    }
}