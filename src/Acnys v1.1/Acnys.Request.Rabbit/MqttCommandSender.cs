using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Request.Abstractions;

namespace Acnys.Request.Rabbit
{
    public class MqttCommandSender : ISendCommand
    {
        private readonly Func<ISendCommand> _next;

        public MqttCommandSender()
        {
        }

        public async Task SendAsync<T>(T command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default) where T : ICommand
        {
        }
    }
}