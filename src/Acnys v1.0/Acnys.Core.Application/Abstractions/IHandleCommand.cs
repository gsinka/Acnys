using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Abstractions;

namespace Acnys.Core.Application.Abstractions
{
    public interface IHandleCommand<in T> where T: ICommand
    {
        Task Handle(T command, IDictionary<string, object> arguments = null, CancellationToken cancellationToken = default);
    }
}