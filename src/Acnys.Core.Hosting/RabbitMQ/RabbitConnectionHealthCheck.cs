using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;

namespace Acnys.Core.Hosting.RabbitMQ
{
    public class RabbitConnectionHealthCheck : IHealthCheck
    {
        private readonly IConnection _connection;

        public RabbitConnectionHealthCheck(IConnection connection)
        {
            _connection = connection;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.Run(() =>
            {
                var status = HealthStatus.Unhealthy;
                string statusText = null;
                Exception resultException = null;
                Dictionary<string, object> data = null;
                
                try
                {
                    status = HealthStatus.Healthy;
                    statusText = $"Connection {_connection.Endpoint} to RabbitMQ is healthy";
                    data = new Dictionary<string, object>();
                }
                catch (Exception exception)
                {
                    status = HealthStatus.Unhealthy;
                    statusText = exception.Message;
                    resultException = exception;
                }
                
                return Task.FromResult<HealthCheckResult>(new HealthCheckResult(status, statusText, resultException, data));

            }, cancellationToken);
        }
    }
}
