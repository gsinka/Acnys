using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Acnys.Core.Tests
{
    public class EventTests
    {
        [Fact]
        public void EventId_from_constructor()
        {
            var eventId = Guid.NewGuid();
            var evnt = new TestEvent(eventId);

            Assert.Equal(eventId, evnt.EventId);
        }

        [Fact]
        public void EventId_created_in_case_of_default_constructor()
        {
            var evnt = new TestEvent();
            Assert.NotEqual(Guid.Empty, evnt.EventId);
        }

        [Fact]
        [SuppressMessage("ReSharper", "EqualExpressionComparison")]
        public void Events_are_equal_if_event_ids_are_equal_or_references_equal()
        {
            var eventId = Guid.NewGuid();
            Assert.Equal(new TestEvent(eventId), new TestEvent(eventId));
            
            Assert.True(Equals(new TestEvent(eventId), new TestEvent(eventId)));
            Assert.True(new TestEvent(eventId) == new TestEvent(eventId));
            Assert.True(new TestEvent() != new TestEvent());
            Assert.True((TestEvent)null == (TestEvent)null);
            Assert.False((TestEvent)null == new TestEvent());
            Assert.False(new TestEvent() == null);

            var testEvent = new TestEvent();
            var testEvent2 = testEvent;
            Assert.True(testEvent.Equals(testEvent2));
        }

        [Fact]
        public void Event_compared_not_to_event_gives_false()
        {
            Assert.False(new TestEvent().Equals(null));
        }

        [Fact]
        public void Event_hash_tests()
        {
            var eventId = Guid.NewGuid();
            Assert.Equal(new TestEvent(eventId).GetHashCode(), new TestEvent(eventId).GetHashCode());
            Assert.NotEqual(new TestEvent().GetHashCode(), new TestEvent().GetHashCode());

        }

        private class TestEvent : Event
        {
            public TestEvent() { }
            public TestEvent(Guid eventId) : base(eventId) { }
        }
    }
}
