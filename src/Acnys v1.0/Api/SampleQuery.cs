using System;
using Acnys.Core;

namespace Api
{
    public class SampleQuery : Query<SampleQuery.Result>
    {
        public SampleQuery(Guid? requestId = null) : base(requestId ?? Guid.NewGuid())
        {
        }

        public class Result
        {
        }
    }
}