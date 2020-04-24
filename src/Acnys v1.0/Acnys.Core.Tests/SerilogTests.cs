using System;
using System.Collections.Generic;
using System.Text;
using Acnys.Core.Infrastructure.Serilog;
using Autofac;
using Xunit;

namespace Acnys.Core.Tests
{
    public class SerilogTests
    {
        [Fact]
        public void Registration_on_null_builder()
        {
            ContainerBuilder builder = null;
            Assert.Throws<ArgumentNullException>(() => builder.RegisterLogger());
        }
    }
}
