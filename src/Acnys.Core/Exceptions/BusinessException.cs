using System;

namespace Acnys.Core.Exceptions
{
    public class BusinessException : Exception
    {
        public string ErrorCode { get; }

        public BusinessException(string errorCode, string message)
            : this(errorCode, message, null)
        {
        }

        public BusinessException(string errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
