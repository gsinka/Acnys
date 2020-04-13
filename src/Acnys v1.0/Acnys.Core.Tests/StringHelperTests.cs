using System;
using System.Collections.Generic;
using System.Text;
using Acnys.Core.Helper;
using Xunit;

namespace Acnys.Core.Tests
{
    public class StringHelperTests
    {
        [Fact]
        public void Add_random_postfix_with_defaults()
        {
            var text = "test".AddRandomPostfix();
            Assert.Equal(15, text.Length);
            Assert.StartsWith("test-", text);
        }

        [Fact]
        public void Add_random_prefix_with_defaults()
        {
            var text = "test".AddRandomPrefix();
            Assert.Equal(15, text.Length);
            Assert.EndsWith("-test", text);
        }
    }
}
