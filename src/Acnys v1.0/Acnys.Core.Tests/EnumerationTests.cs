using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Acnys.Core.Tests
{
    public class EnumerationTests
    {
        [Fact]
        public void Check_enum_values()
        {
            Assert.Equal(1, TestEnumeration.Item1.Id);
            Assert.Equal("1", TestEnumeration.Item1.Name);
            Assert.Equal("1", TestEnumeration.Item1.ToString());

            Assert.Equal(2, TestEnumeration.Item2.Id);
            Assert.Equal("2", TestEnumeration.Item2.Name);

            var items = Enumeration.GetAll<TestEnumeration>();
            Assert.Equal(2, items.Count());

            Assert.True(TestEnumeration.Item1.Equals(TestEnumeration.Item1));
            Assert.False(TestEnumeration.Item1.Equals("no an enum"));
            Assert.False(TestEnumeration.Item1.Equals(OtherTestEnumeration.Item1));
            
            Assert.Equal(TestEnumeration.Item1.GetHashCode(), TestEnumeration.Item1.GetHashCode());

            Assert.Equal(0, TestEnumeration.Item1.CompareTo(OtherTestEnumeration.Item1));

        }

        private class TestEnumeration : Enumeration
        {
            public static TestEnumeration Item1 = new TestEnumeration(1, "1");
            public static TestEnumeration Item2 = new TestEnumeration(2, "2");

            private TestEnumeration(int id, string name) : base(id, name)  { }
        }
        private class OtherTestEnumeration : Enumeration
        {
            public static OtherTestEnumeration Item1 = new OtherTestEnumeration(1, "1");
            public static OtherTestEnumeration Item2 = new OtherTestEnumeration(3, "3");

            private OtherTestEnumeration(int id, string name) : base(id, name)  { }
        }
    }
}
