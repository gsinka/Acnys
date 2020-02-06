using Acnys.Core.Abstractions;

namespace Acnys.Core.Infrastructure.Abstractions
{
    public interface ICommandDispatcherSelector
    {
        IDispatchCommand GetDispatcherFor(ICommand command);
    }
}