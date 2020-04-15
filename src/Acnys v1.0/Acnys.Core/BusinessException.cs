using System;

namespace Acnys.Core
{
    public class BusinessException : Exception
    {
        public int ErrorCode { get; }
       

        public BusinessException(int errorCode, string message, Exception innerException = null)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
