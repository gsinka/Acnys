using System.Collections.Generic;
using System.Text;
using Acnys.Core.Eventing.Abstractions;
using Newtonsoft.Json;
using Xunit;

namespace Acnys.Core.RabbitMQ.Tests
{
    public class EventPublisherContextBuilderTests
    {
        [Theory]
        [MemberData(nameof(Arguments))]
        public void Default_context_builder_tests(IEvent @event, Dictionary<string, object> args, string expectedRoutingKey, bool expectedMandatory)
        {
            var (routingKey, mandatory, properties, body) = EventPublisher.DefaultContextBuilder(@event, args);

            Assert.Equal(expectedRoutingKey, routingKey);
            Assert.Equal(expectedMandatory, mandatory);

            var expectedBody = @event != null ? Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event)) : null;
            Assert.Equal(expectedBody, body);
        }

        public static IEnumerable<object[]> Arguments()
        {
            return new List<object[]>()
            {
                // args, expected routing key, expected mandatory flag

                new object[] {
                    null, 
                    null, string.Empty, false },

                new object[] {
                    new TestEvent("test"),
                    new Dictionary<string, object>()
                    {
                        { EventPublisher.RoutingKey, string.Empty },
                    }, string.Empty, false },

                new object[] {
                    new TestEvent("test"),
                    new Dictionary<string, object>()
                    {
                        { EventPublisher.RoutingKey, "test" },
                    }, "test", false },

                new object[] {
                    new TestEvent("test"),
                    new Dictionary<string, object>()
                    {
                        { EventPublisher.Mandatory, false }
                    }, string.Empty, false },

                new object[] {
                    new TestEvent("test"),
                    new Dictionary<string, object>()
                    {
                        { EventPublisher.Mandatory, true }
                    }, string.Empty, true },
            };
        }

    }
}
