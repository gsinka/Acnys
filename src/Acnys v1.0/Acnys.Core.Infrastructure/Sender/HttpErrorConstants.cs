namespace Acnys.Core.Infrastructure.Sender
{
    public static class HttpErrorConstants
    {
        public const string HttpHeaderKey = "x-error-type";
        
        public const string ValidationError = "validation";
        public const string BusinessError = "business";
    }
}