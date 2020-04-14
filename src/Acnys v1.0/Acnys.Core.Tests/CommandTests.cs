using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Xunit;

namespace Acnys.Core.Tests
{
    public class CommandTests
    {
        [Fact]
        public void RequestId_from_constructor()
        {
            var requestId = Guid.NewGuid();
            var evnt = new TestCommand(requestId);

            Assert.Equal(requestId, evnt.RequestId);
        }

        [Fact]
        public void RequestId_created_in_case_of_default_constructor()
        {
            var evnt = new TestCommand();
            Assert.NotEqual(Guid.Empty, evnt.RequestId);
        }
        
        [Fact]
        [SuppressMessage("ReSharper", "EqualExpressionComparison")]
        public void Commands_are_equal_if_command_ids_are_equal_or_references_equal()
        {
            var requestId = Guid.NewGuid();
            Assert.Equal(new TestCommand(requestId), new TestCommand(requestId));
            
            Assert.True(Equals(new TestCommand(requestId), new TestCommand(requestId)));
            Assert.True(new TestCommand(requestId) == new TestCommand(requestId));
            Assert.True(new TestCommand() != new TestCommand());
            Assert.True((TestCommand)null == (TestCommand)null);
            Assert.False((TestCommand)null == new TestCommand());
            Assert.False(new TestCommand() == null);

            var testEvent = new TestCommand();
            var testEvent2 = testEvent;
            Assert.True(testEvent.Equals(testEvent2));
        }

        [Fact]
        public void Command_compared_not_to_command_gives_false()
        {
            Assert.False(new TestCommand().Equals(null));
        }

        [Fact]
        public void Command_hash_tests()
        {
            var requestId = Guid.NewGuid();
            Assert.Equal(new TestCommand(requestId).GetHashCode(), new TestCommand(requestId).GetHashCode());
            Assert.NotEqual(new TestCommand().GetHashCode(), new TestCommand().GetHashCode());

        }

        [Fact]
        public void Command_with_constructor_serialized_and_deserialized_are_equal()
        {
            var command = new TestCommand();
            var commandJson = JsonConvert.SerializeObject(command);
            var deserialized = JsonConvert.DeserializeObject(commandJson, typeof(TestCommand));

            Assert.Equal(deserialized, command);
        }

        [Fact]
        public void Command_wo_constructor_serialized_and_deserialized_are_equal()
        {
            var command = new TestCommandWithoutConstructor();
            var commandJson = JsonConvert.SerializeObject(command);
            var deserialized = JsonConvert.DeserializeObject<TestCommandWithoutConstructor>(commandJson, new JsonSerializerSettings()
            {
                
            });

            Assert.Equal(deserialized, command);
        }

        public class TestCommand : Command
        {
            public TestCommand(Guid? requestId = null) : base(requestId ?? Guid.NewGuid()) { }
        }

        public class TestCommandWithoutConstructor : Command
        {
            public TestCommandWithoutConstructor(Guid? requestId = null) : base(requestId ?? Guid.NewGuid())
            {
            }
        }
    }
}