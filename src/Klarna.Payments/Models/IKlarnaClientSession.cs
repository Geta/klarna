namespace Klarna.Payments.Models
{
    public interface IKlarnaClientSession 
    {
        string ClientToken { get; set; }
    }
}