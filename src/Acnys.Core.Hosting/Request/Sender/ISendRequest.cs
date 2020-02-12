using Acnys.Core.Request.Application;

namespace Acnys.Core.Hosting.Request.Sender
{
    public interface ISendRequest : ISendCommand, ISendQuery
    {
    }
}