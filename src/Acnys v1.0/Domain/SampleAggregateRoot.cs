using Acnys.Core.Domain;
using Api.Events;

namespace Domain
{
    public class SampleAggregateRoot : AggregateRoot
    {
        public void DoSomething()
        {
            RaiseEvent(new SampleEvent());
        }
    }
}
