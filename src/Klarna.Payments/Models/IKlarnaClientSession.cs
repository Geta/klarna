namespace Klarna.Payments.Models
{
    public interface IKlarnaClientSession 
    {
        string SessionId { get; set; }
        string ClientToken { get; set; }
    }
}