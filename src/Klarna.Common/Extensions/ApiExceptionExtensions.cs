using Klarna.Common.Models;

namespace Klarna.Common.Extensions
{
    public static class ApiExceptionExtensions
    {
        /// <summary>
        /// Returns a compact "{CorrelationId} {ErrorCode} {messages joined by ", "}" representation of the
        /// Klarna error, falling back to the underlying exception message when no Klarna error model is attached.
        /// Never throws — formatting failures are swallowed in favour of returning the exception's full string
        /// representation, so the original exception is never masked by a formatting bug.
        /// </summary>
        public static string GetFormattedErrorMessage(this ApiException apiException)
        {
            try
            {
                if (apiException == null)
                {
                    return string.Empty;
                }
                var errorMessage = apiException.ErrorMessage;
                if (errorMessage == null)
                {
                    return apiException.Message ?? string.Empty;
                }
                var messages = JoinMessages(errorMessage.ErrorMessages, ", ");
                return $"{errorMessage.CorrelationId} {errorMessage.ErrorCode} {messages}";
            }
            catch
            {
                return apiException?.ToString() ?? string.Empty;
            }
        }

        /// <summary>
        /// Returns a labeled log-line "Error Code: '...'; CorrelationId: '...'; Messages: '...'" used by
        /// checkout-order logging, falling back to the underlying exception message when no Klarna error model
        /// is attached. Never throws — see <see cref="GetFormattedErrorMessage"/>.
        /// </summary>
        public static string GetVerboseLogMessage(this ApiException apiException)
        {
            try
            {
                if (apiException == null)
                {
                    return string.Empty;
                }
                var errorMessage = apiException.ErrorMessage;
                if (errorMessage == null)
                {
                    return apiException.Message ?? string.Empty;
                }
                var messages = JoinMessages(errorMessage.ErrorMessages, " ");
                return $"Error Code: '{errorMessage.ErrorCode}'; CorrelationId: '{errorMessage.CorrelationId}'; Messages: '{messages}'";
            }
            catch
            {
                return apiException?.ToString() ?? string.Empty;
            }
        }

        private static string JoinMessages(string[] messages, string separator)
        {
            return messages != null ? string.Join(separator, messages) : string.Empty;
        }
    }
}
