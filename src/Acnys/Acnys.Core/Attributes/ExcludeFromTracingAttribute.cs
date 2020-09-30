using System;
using System.Collections.Generic;
using System.Text;

namespace Acnys.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class ExcludeFromTracingAttribute : Attribute { }
}
