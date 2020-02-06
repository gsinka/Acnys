using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Abstractions;

namespace Acnys.Core.Application.Abstractions
{
    public interface IHandleCommand<in T> where T: ICommand
    {
        Task Handle(T command, CancellationToken cancellationToken = default);
    }
}