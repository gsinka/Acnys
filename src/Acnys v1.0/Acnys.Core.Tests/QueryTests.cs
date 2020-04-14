using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Acnys.Core.Tests
{
    public class QueryTests
    {
        [Fact]
        public void RequestId_from_constructor()
        {
            var requestId = Guid.NewGuid();
            var evnt = new TestQuery(requestId);

            Assert.Equal(requestId, evnt.RequestId);
        }

        [Fact]
        public void RequestId_created_in_case_of_default_constructor()
        {
            var evnt = new TestQuery();
            Assert.NotEqual(Guid.Empty, evnt.RequestId);
        }

        [Fact]
        [SuppressMessage("ReSharper", "EqualExpressionComparison")]
        public void Events_are_equal_if_event_ids_are_equal_or_references_equal()
        {
            var requestId = Guid.NewGuid();
            Assert.Equal(new TestQuery(requestId), new TestQuery(requestId));
            
            Assert.True(Equals(new TestQuery(requestId), new TestQuery(requestId)));

            var testEvent = new TestQuery();
            var testEvent2 = testEvent;
            Assert.True(testEvent.Equals(testEvent2));
        }

        [Fact]
        public void Event_compared_not_to_event_gives_false()
        {
            Assert.False(new TestQuery().Equals(null));
        }

        [Fact]
        public void Event_hash_tests()
        {
            var requestId = Guid.NewGuid();
            Assert.Equal(new TestQuery(requestId).GetHashCode(), new TestQuery(requestId).GetHashCode());
            Assert.NotEqual(new TestQuery().GetHashCode(), new TestQuery().GetHashCode());

        }

        private class TestQuery : Query<object>
        {
            public TestQuery(Guid? requestId = null) : base(requestId ?? Guid.NewGuid())
            { }
            public TestQuery(Guid requestId) : base(requestId) { }
        }
    }
}