namespace Klarna.Common
{
    public static class Constants
    {
        public const string KlarnaSystemKeyword = "Klarna";

        public const string OrderNamespace = "Mediachase.Commerce.Orders";
        public const string PurchaseOrderClass = "PurchaseOrder";
        public const string OtherPaymentClass = "OtherPayment";
        public const string CartClass = "ShoppingCart";

        // Payment method property fields
        public const string KlarnaSerializedMarketOptions = "KlarnaSerializedMarketOptions";

        // Purchase order meta fields
        public const string KlarnaOrderIdField = "KlarnaOrderId";

        // Other payment meta fields
        public const string FraudStatusPaymentField = "FraudStatusPayment";

        /// <summary>
        /// TODO The current version of .NET SDK library for Klarna Services
        /// </summary>
        public const string Version = "3.1.12";

        /// <summary>
        /// The API for the European live environment
        /// </summary>
        public const string ProdUrlEurope = "https://api.klarna.com/";

        /// <summary>
        /// The API for the U.S. live environment
        /// </summary>
        public const string ProdUrlNorthAmerica = "https://api-na.klarna.com/";

        /// <summary>
        /// The API for the Oceania live environment
        /// </summary>
        public const string ProdUrlOceania = "https://api-oc.klarna.com/";

        /// <summary>
        /// The API for the European testing environment
        /// </summary>
        public const string TestUrlEurope = "https://api.playground.klarna.com/";

        /// <summary>
        /// The API for the U.S. testing environment
        /// </summary>
        public const string TestUrlNorthAmerica = "https://api-na.playground.klarna.com/";

        /// <summary>
        /// The API for the Oceania testing environment
        /// </summary>
        public const string TestUrlOceania = "https://api-oc.playground.klarna.com/";
    }
}
