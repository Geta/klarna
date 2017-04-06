namespace Klarna.Payments
{
    public class Configuration
    {
        public bool IsCustomerPreAssessmentEnabled { get; set; }
        public bool IsProduction { get; set; }
        public bool SendProductAndImageUrlField { get; set; }
        public bool UseAttachmentsField { get; set; }
    }
}
