namespace Acnys.Core.Request
{
    //public class CommandSender : ISendCommand
    //{
    //    private readonly ILogger _log;
    //    private readonly ICommandDispatcherSelector _dispatcherSelector;

    //    public CommandSender(ILogger log, ICommandDispatcherSelector dispatcherSelector)
    //    {
    //        _log = log;
    //        _dispatcherSelector = dispatcherSelector;
    //    }

    //    public async Task Send<T>(T command, CancellationToken cancellationToken = default) where T : ICommand
    //    {
    //        _log.Verbose("Selecting dispatcher for command {commandType}", command.GetType().Name);
    //        var dispatcher = _dispatcherSelector.GetDispatcherFor(command) ?? throw new InvalidOperationException($"No dispatcher found for command {command.GetType().Name}");
            
    //        _log.Verbose("Dispatching command {commandType} with dispatcher {dispatcherType}", command.GetType().Name, dispatcher.GetType().Name);
    //        await dispatcher.Dispatch(command, cancellationToken);
    //    }
    //}
}
