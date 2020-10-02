using Jaeger.Senders;

namespace Acnys.Core.AspNet.Tracing
{
    public class TracingOptions
    {
        public ISenderFactory SenderFactory { get; set; }
        public ISender Sender { get; set; }
    }
}
