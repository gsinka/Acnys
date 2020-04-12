using System;
using System.Collections.Generic;
using System.Text;
using Acnys.Core.Services;
using Xunit;

namespace Acnys.Core.Tests
{
    public class ClockTests
    {
        [Fact]
        public void Test_ComputerClock()
        {
            var clock = new ComputerClock();

            var difference = (clock.UtcNow - DateTime.UtcNow).TotalMilliseconds;
            Assert.InRange(difference, -1, 1);
        }
    }
}
