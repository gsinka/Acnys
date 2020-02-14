using System.Threading;
using System.Threading.Tasks;

namespace Acnys.Core.Request.Abstractions
{
    public interface IHandleCommand<in T> where T: ICommand
    {
        Task Handle(T command, CancellationToken cancellationToken = default);
    }
}