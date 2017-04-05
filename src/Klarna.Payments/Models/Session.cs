using Newtonsoft.Json;

namespace Klarna.Payments.Models
{
    public class Session
    {
        [JsonProperty("design")]
        public string Design { get; set; }
        [JsonProperty("purchase_country")]
        public string PurchaseCountry { get; set; }
        [JsonProperty("purchase_currency")]
        public string PurchaseCurrency { get; set; }
        [JsonProperty("locale")]
        public string Locale { get; set; }
        [JsonProperty("billing_address")]
        public Address BillingAddress { get; set; }
        [JsonProperty("shipping_address")]
        public Address ShippingAddress { get; set; }
        [JsonProperty("order_amount")]
        public int OrderAmount { get; set; }
        [JsonProperty("order_tax_amount")]
        public int OrderTaxAmount { get; set; }
        [JsonProperty("order_lines")]
        public OrderLine[] OrderLines { get; set; }
        [JsonProperty("customer")]
        public Customer Customer { get; set; }
        [JsonProperty("merchant_urls")]
        public MerchantUrl MerchantUrl { get; set; }
        [JsonProperty("merchant_reference1")]
        public string MerchantReference1 { get; set; }
        [JsonProperty("merchant_reference2")]
        public string MerchantReference2 { get; set; }
        [JsonProperty("merchant_data")]
        public string MerchantData { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }
        [JsonProperty("options")]
        public Options Options { get; set; }
    }

    public class Options
    {
        [JsonProperty("color_button")]
        public string ColorButton { get; set; }

        [JsonProperty("color_button_text")]
        public string ColorButtonText { get; set; }
        [JsonProperty("color_checkbox")]
        public string ColorCheckbox { get; set; }
        [JsonProperty("color_checkbox_checkmark")]
        public string ColorCheckboxCheckmark { get; set; }
        [JsonProperty("color_header")]
        public string ColorHeader { get; set; }
        [JsonProperty("color_link")]
        public string ColorLink { get; set; }
        [JsonProperty("color_border")]
        public string ColorBorder { get; set; }
        [JsonProperty("color_border_selected")]
        public string ColorBorderSelected { get; set; }
        [JsonProperty("color_text")]
        public string ColorText { get; set; }
        [JsonProperty("color_details")]
        public string ColorDetails { get; set; }
        [JsonProperty("color_text_secondary")]
        public string ColorTextSecondary { get; set; }
        [JsonProperty("radius_border")]
        public string RadiusBorder { get; set; }
    }

    public class Attachment
    {
        [JsonProperty("content_type")]
        public string ContentType { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }
    }

    public class MerchantUrl
    {
        [JsonProperty("status_update")]
        public string StatusUpdate { get; set; }

        [JsonProperty("confirmation")]
        public string Confirmation { get; set; }
        [JsonProperty("notification")]
        public string Notification { get; set; }
        [JsonProperty("push")]
        public string Push { get; set; }
    }

    public class Customer
    {
        [JsonProperty("date_of_birth")]
        public string DateOfBirth { get; set; }
        [JsonProperty("gender")]
        public string Gender { get; set; }
        [JsonProperty("last_four_ssn")]
        public string LastFourSsn{ get; set; }
    }

    public class Address
    {
        [JsonProperty("given_name")]
        public string GivenName { get; set; }
        [JsonProperty("family_name")]
        public string FamilyName { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("street_address")]
        public string StreetAddress { get; set; }
        [JsonProperty("street_address2")]
        public string StreetAddress2 { get; set; }
        [JsonProperty("postal_code")]
        public string PostalCode { get; set; }
        [JsonProperty("city")]
        public string City { get; set; }
        [JsonProperty("region")]
        public string Region { get; set; }
        [JsonProperty("phone")]
        public string Phone { get; set; }
        [JsonProperty("country")]
        public string Country { get; set; }
    }

    public class OrderLine
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("reference")]
        public string Reference { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("quantity")]
        public int Quantity { get; set; }
        [JsonProperty("unit_price")]
        public int UnitPrice { get; set; }
        [JsonProperty("tax_rate")]
        public int TaxRate { get; set; }
        [JsonProperty("total_amount")]
        public int TotalAmount { get; set; }
        [JsonProperty("total_discount_amount")]
        public int TotalDiscountAmount { get; set; }
        [JsonProperty("total_tax_amount")]
        public int TotalTaxAmount { get; set; }
        [JsonProperty("product_url")]
        public string ProductUrl { get; set; }
        [JsonProperty("image_url")]
        public string ProductImageUrl { get; set; }
    }

}
