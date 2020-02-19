using NJsonSchema.Validation;
using System.Collections.Generic;

namespace Acnys.Core.Hosting.ErrorHandling.Responses
{
    internal class ValidationErrorResponse : ValueObject
    {
        public string ErrorCode { get; }
        public IEnumerable<ValidationError> ValidationErrors { get; }

        public ValidationErrorResponse(string errorCode, IEnumerable<ValidationError> validationErrors)
        {
            ErrorCode = errorCode;
            ValidationErrors = validationErrors;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return ErrorCode;
            yield return ValidationErrors;
        }
    }

    internal class ValidationError : ValueObject
    {
        public string ErrorCode { get; }
        public string Message { get; }
        public string Property { get; }

        public ValidationError(string errorCode, string message, string property)
        {
            ErrorCode = errorCode;
            Message = message;
            Property = property;
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return ErrorCode;
            yield return Message;
            yield return Property;
        }
    }
}
