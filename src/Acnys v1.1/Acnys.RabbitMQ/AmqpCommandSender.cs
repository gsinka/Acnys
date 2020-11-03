using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Request.Abstractions;
using Acnys.Request.Rabbit;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Acnys.RabbitMQ
{
    /// <summary>
    /// AMQP command sender
    /// </summary>
    public class AmqpCommandSender : ISendCommand
    {
        private readonly ILogger<AmqpCommandSender> _log;
        private readonly IModel _channel;
        private const string RabbitPublishOptionsKey = "x-rabbit-publish-options";
        private const string RabbitBasicPropertiesKey = "x-rabbit-basic-properties";
        
        public AmqpCommandSender(ILogger<AmqpCommandSender> log, IModel channel)
        {
            _log = log;
            _channel = channel;

            _log.LogTrace("AMQP command sender {senderId} initiated", GetHashCode());
        }

        public Task SendAsync<T>(T command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default) where T : ICommand
        {
            _log.BeginScope("Sending command on AMQP");

            var rabbitPublishOptions = (RabbitPublishOptions)(arguments?[RabbitPublishOptionsKey] ?? new ArgumentException("Rabbit publish options are missing", RabbitPublishOptionsKey));
            _log.LogDebug("Publish options extracted from arguments (publishOptionsKey): {@publishOptions}", RabbitPublishOptionsKey, rabbitPublishOptions);
            
            var rabbitBasicProperties = ((IBasicProperties)arguments?[RabbitBasicPropertiesKey]);

            if (rabbitBasicProperties == null)
            {
                rabbitBasicProperties = _channel.CreateBasicProperties();
                _log.LogDebug("Basic properties not found in arguments (publishOptionsKey), default created: {@publishOptions}", RabbitPublishOptionsKey, rabbitPublishOptions);
            }
            else
            {
                _log.LogDebug("Basic properties extracted from arguments (publishOptionsKey): {@publishOptions}", RabbitPublishOptionsKey, rabbitPublishOptions);
            }

            var bodyJson = JsonConvert.SerializeObject(command, Formatting.None, new JsonSerializerSettings() { });
            _log.LogDebug("Command {commandType} serialized for publish: {messageBody}", command.GetType().Name, bodyJson);
            
            _log.LogTrace("Publishing command on channel {channelNumber}", _channel.ChannelNumber);
            _channel.BasicPublish(rabbitPublishOptions.Exchange, rabbitPublishOptions.RoutingKey, rabbitPublishOptions.Mandatory, rabbitBasicProperties, Encoding.UTF8.GetBytes(bodyJson));
            _log.LogTrace("Command published on channel {channelNumber}", _channel.ChannelNumber);

            

            return Task.CompletedTask;
        }
    }
}