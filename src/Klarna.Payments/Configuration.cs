using System.Collections.Generic;

namespace Klarna.Payments
{
    public class Configuration
    {
        public IEnumerable<string> CustomerPreAssessmentCountries { get; set; }
        public bool IsProduction { get; set; }
        public bool SendProductAndImageUrlField { get; set; }
        public bool UseAttachments { get; set; }
    }
}
