using Acnys.Core.Abstractions;

namespace Acnys.Core.ValueObjects
{
    public partial class ErrorCode : Enumeration
    {
        public string Message { get; private set; }

        public static ErrorCode Unknown = new ErrorCode(0, "Unknown", "Unknown error occured");
        public static ErrorCode NotFound = new ErrorCode(404, "Not found", "The requested resource not found");
        public static ErrorCode Unauthorized = new ErrorCode(401, "Unauthorized", "Authentication failed");
        public static ErrorCode Forbidden = new ErrorCode(403, "Forbidden", "Necessary permissions are missing to access resource");
        public static ErrorCode NotAllowed = new ErrorCode(405, "Not allowed", "Request method is not supported for the requested resource");
        public static ErrorCode Validation = new ErrorCode(1000, "Validation", "Request validation failed");
        public static ErrorCode Business = new ErrorCode(3000, "Business", "Unknown business exception occured");

        public ErrorCode(int id, string name, string message) : base(id, name)
        {
            Message = message;
        }

        public override string ToString() => $"{Id}-{Name}";
    }
}
