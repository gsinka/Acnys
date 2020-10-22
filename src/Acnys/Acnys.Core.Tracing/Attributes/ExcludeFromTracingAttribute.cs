using System;

namespace Acnys.Core.Tracing.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class ExcludeFromTracingAttribute : Attribute { }
}
