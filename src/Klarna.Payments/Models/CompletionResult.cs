namespace Klarna.Payments.Models
{
    public class CompletionResult
    {
        public string RedirectUrl { get; }
        public bool IsRedirect => !string.IsNullOrEmpty(RedirectUrl);

        public static CompletionResult Empty = new CompletionResult(null);

        public CompletionResult(string redirectUrl)
        {
            RedirectUrl = redirectUrl;
        }

        public static CompletionResult WithRedirectUrl(string redirectUrl)
        {
            return new CompletionResult(redirectUrl);
        }
    }
}