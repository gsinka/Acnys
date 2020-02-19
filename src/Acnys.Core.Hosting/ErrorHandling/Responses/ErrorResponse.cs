using System.Collections.Generic;

namespace Acnys.Core.Hosting.ErrorHandling.Responses
{
    internal class ErrorResponse : ValueObject
    {
        public string Code { get; }
        public string Message { get; }

        public ErrorResponse(string code, string message)
        {
            Code = code;
            Message = message;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Code;
            yield return Message;
        }
    }
}
