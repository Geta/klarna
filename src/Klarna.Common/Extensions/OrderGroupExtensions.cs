using System;
using EPiServer.Commerce.Order;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Customers;
using Mediachase.Commerce.Orders;

namespace Klarna.Common.Extensions
{
    public static class OrderGroupExtensions
    {
        private static readonly Injected<IOrderRepository> _orderRepository;
        private static readonly Injected<IOrderGroupFactory> _orderGroupFactory;

        public static void AddNote(this IOrderGroup orderGroup, string noteTitle, string noteMessage)
        {
            var note = _orderGroupFactory.Service.CreateOrderNote(orderGroup);
            note.CustomerId = CustomerContext.Current.CurrentContactId;
            note.Type = OrderNoteTypes.Custom.ToString();
            note.Title = noteTitle;
            note.Detail = noteMessage;
            note.Created = DateTime.UtcNow;
            orderGroup.Notes.Add(note);

            _orderRepository.Service.Save(orderGroup);
        }
    }
}
