using Acnys.Core.Domain;
using Api;

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
