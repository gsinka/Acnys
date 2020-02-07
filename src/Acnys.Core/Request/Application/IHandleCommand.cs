using System.Threading;
using System.Threading.Tasks;

namespace Acnys.Core.Request.Application
{
    public interface IHandleCommand<in T> where T: ICommand
    {
        Task Handle(T command, CancellationToken cancellationToken = default);
    }
}