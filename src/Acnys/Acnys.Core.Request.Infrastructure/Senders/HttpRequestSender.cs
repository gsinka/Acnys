using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Request.Abstractions;
using Serilog;

namespace Acnys.Core.Request.Infrastructure.Senders
{
    //public class HttpRequestSender : ISendRequest
    //{
    //    private readonly ISendCommand _commandSender;
    //    private readonly ISendQuery _querySender;

    //    public HttpRequestSender(ISendCommand commandSender, ISendQuery querySender)
    //    {
    //        _commandSender = commandSender;
    //        _querySender = querySender;
    //    }

    //    private readonly Func<Type, string> _typeNameBuilder = type => $"{type.FullName}, {type.Assembly.GetName().Name}";

    //    public async Task Send<T>(T command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default) where T : ICommand
    //    {
    //        await _commandSender.Send(command, arguments, cancellationToken);
    //    }

    //    public async Task<T> Send<T>(IQuery<T> query, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default)
    //    {
    //        return await _querySender.Send(query, arguments, cancellationToken);
    //    }
    //}
}