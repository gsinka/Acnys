using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Acnys.Core.Tests
{
    public class ValueObjectTests
    {
        [Fact]
        public void Equality_tests()
        {
            var valueObject = new TestValueObject("Value 1", "Value 2");
            var sameValueObject = new TestValueObject("Value 1", "Value 2");
            var differentValueObject = new TestValueObject("Other value 1", null);
            var differentValueObject2 = new TestValueObject(null, "Other value 2");
            
            Assert.Equal(valueObject, sameValueObject);
            Assert.True(valueObject == sameValueObject);
            Assert.False(valueObject != sameValueObject);

            Assert.NotEqual(valueObject, differentValueObject);
            Assert.NotEqual(valueObject, differentValueObject2);
            Assert.False(valueObject == differentValueObject);
            Assert.True(valueObject != differentValueObject);

            Assert.False(valueObject == null);
            Assert.False(valueObject.Equals(null));

            Assert.Equal(valueObject.GetHashCode(), sameValueObject.GetHashCode());
            Assert.NotEqual(valueObject.GetHashCode(), differentValueObject.GetHashCode());

            var copyObject = valueObject.GetCopy();
            Assert.Equal(valueObject, copyObject);
        }

        private class TestValueObject : ValueObject
        {
            public string Value1 { get; }
            public string Value2 { get; }

            public TestValueObject(string value1, string value2)
            {
                Value1 = value1;
                Value2 = value2;
            }

            protected override IEnumerable<object> GetAtomicValues()
            {
                return new object[] { Value1, Value2 };
            }
        }
    }
}
