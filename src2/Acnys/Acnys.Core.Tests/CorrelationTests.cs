using System;
using System.Collections.Generic;
using Acnys.Core.Abstractions.Extensions;
using Xunit;

namespace Acnys.Core.Tests
{
    public class CorrelationTests
    {
        [Fact]
        public void Empty_dictionary_returns_null()
        {
            var args = new Dictionary<string, object>();

            Assert.Null(args.CorrelationId());
            Assert.Null(args.CausationId());

            IDictionary<string, object> nullArgs = null;
            Assert.Null(nullArgs.CorrelationId());
        }

        [Fact]
        public void Null_dictionary_returns_null()
        {
            IDictionary<string, object> args = null;

            Assert.Null(args.CorrelationId());
            Assert.Null(args.CausationId());
        }

        [Fact]
        public void CorrelationId_can_be_set_and_get_from_dictionary()
        {
            var correlationId = Guid.NewGuid();
            var args = new Dictionary<string, object>().UseCorrelationId(correlationId);

            Assert.NotNull(args.CorrelationId());
            Assert.Equal(correlationId, args.CorrelationId());
        }

        [Fact]
        public void CausationId_can_be_set_and_get_from_dictionary()
        {
            var causationId = Guid.NewGuid();
            var args = new Dictionary<string, object>().UseCausationId(causationId);

            Assert.NotNull(args.CausationId());
            Assert.Equal(causationId, args.CausationId());
        }

    }
}
