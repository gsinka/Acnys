namespace Acnys.Core.Request
{
    //public class QuerySender : ISendQuery
    //{
    //    private readonly ILogger _log;
    //    private readonly IQueryDispatcherSelector _dispatcherSelector;

    //    public QuerySender(ILogger log, IQueryDispatcherSelector dispatcherSelector)
    //    {
    //        _log = log;
    //        _dispatcherSelector = dispatcherSelector;
    //    }

    //    public async Task<T> Send<T>(IQuery<T> query, CancellationToken cancellationToken = default)
    //    {
    //        _log.Verbose("Selecting dispatcher for query {queryType}", query.GetType().Name);
    //        var dispatcher = _dispatcherSelector.GetDispatcherFor(query) ?? throw new InvalidOperationException($"No dispatcher found for command {query.GetType().Name}");

    //        _log.Verbose("Dispatching query {queryType} with dispatcher {dispatcherType}", query.GetType().Name, dispatcher.GetType().Name);
    //        return await dispatcher.Dispatch(query, cancellationToken);
    //    }
    //}
}