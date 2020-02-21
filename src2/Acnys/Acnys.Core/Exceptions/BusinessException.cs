using System;

namespace Acnys.Core.Exceptions
{
    public class BusinessException : Exception
    {
        public int ErrorCode { get; }

        public BusinessException(int errorCode, string message)
            : this(errorCode, message, null)
        {
        }

        public BusinessException(int errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
