namespace EPiServer.Reference.Commerce.Site.Infrastructure.Attributes
{
    public class LocalizedEmailAttribute : LocalizedRegularExpressionAttribute
    {
        public LocalizedEmailAttribute(string name)
            : base(@"^[_a-z0-9-]+(\.[_a-z0-9-]+)*(\+[_a-z0-9-]+)?@[a-z0-9-]+(\.[a-z0-9-]+)*$", name)
        {
        }
    }
}