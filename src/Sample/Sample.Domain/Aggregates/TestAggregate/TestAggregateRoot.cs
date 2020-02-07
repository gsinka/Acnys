using System;
using System.Collections.Generic;
using Acnys.Core.Aggregates;
using Acnys.Core.Eventing;

namespace Sample.Domain.Aggregates.TestAggregate
{
    public class TestAggregateRoot : IEventSourcedAggregateRoot
    {
        public Guid AggregateId { get; }
        public long AggregateVersion { get; }

        public TestAggregateRoot(Guid id)
        {
            AggregateId = id;
            AggregateVersion = 1;
        }

        public void TestFunction(string data)
        {
        }

        public IList<IEvent> GetUncommittedEvents()
        {
            throw new NotImplementedException();
        }

        public void ClearUncommittedEvents()
        {
            throw new NotImplementedException();
        }
    }
}
