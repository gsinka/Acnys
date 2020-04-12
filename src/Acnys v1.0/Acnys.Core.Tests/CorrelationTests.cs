using System;
using System.Collections.Generic;
using Acnys.Core.Helper;
using Acnys.Core.ValueObjects;
using Xunit;

// ReSharper disable InconsistentNaming

namespace Acnys.Core.Tests
{
    public class CorrelationTests
    {
        [Fact]
        public void Add_correlationId_to_arguments()
        {
            var arguments = new Dictionary<string, object>();
            var correlationId = Guid.NewGuid();

            arguments.UseCorrelationId(correlationId);
            Assert.Contains(arguments,pair => pair.Key == RequestConstants.CorrelationId && pair.Value.Equals(correlationId));
        }

        [Fact]
        public void Add_null_correlationId_to_arguments()
        {
            var arguments = new Dictionary<string, object>();
            arguments.UseCorrelationId(null);
            Assert.Empty(arguments);
        }

        [Fact]
        public void Add_correlationId_to_null_arguments_throws_exception()
        {
            Assert.Throws<ArgumentNullException>(() => ((IDictionary<string, object>)null).UseCorrelationId(Guid.NewGuid()));
        }

        [Fact]
        public void Get_correlationId_from_null_arguments_gives_null()
        {
            Assert.Null(((IDictionary<string, object>)null).CorrelationId());
        }

        [Fact]
        public void Get_guid_correlationId_from_arguments()
        {
            var arguments = new Dictionary<string, object>();
            var correlationId = Guid.NewGuid();
            arguments.Add(RequestConstants.CorrelationId, correlationId);

            Assert.Equal(correlationId, arguments.CorrelationId());
        }

        [Fact]
        public void Get_string_correlationId_from_arguments()
        {
            var arguments = new Dictionary<string, object>();
            var correlationId = Guid.NewGuid();
            arguments.Add(RequestConstants.CorrelationId, correlationId.ToString());

            Assert.Equal(correlationId, arguments.CorrelationId());
        }

        [Fact]
        public void Get_invalid_correlationId_from_arguments_gives_null()
        {
            var arguments = new Dictionary<string, object> {{RequestConstants.CorrelationId, "invalid_guid"}};
            Assert.Null(arguments.CorrelationId());
        }
    }
}
