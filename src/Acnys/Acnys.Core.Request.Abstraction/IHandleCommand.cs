using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Acnys.Core.Request.Abstractions
{
    public interface IHandleCommand<in T> where T: ICommand
    {
        Task Handle(T command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default);
    }
}