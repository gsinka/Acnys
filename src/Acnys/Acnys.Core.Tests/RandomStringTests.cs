using Acnys.Core.Helper;
using Xunit;

namespace Acnys.Core.Tests
{
    public class RandomStringTests
    {
        [Fact]
        public void Random_postfix()
        {
            var str = "test";
            var pre = str.AddRandomPrefix();
            var post = str.AddRandomPostfix();

            Assert.StartsWith(str, post);
            Assert.Equal(15, pre.Length);
            
            Assert.EndsWith(str, pre);
            Assert.Equal(15, post.Length);

        }
    }
}
