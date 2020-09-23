using Acnys.Core.Attributes;
using Microsoft.Extensions.Primitives;
using OpenTracing;
using OpenTracing.Propagation;
using OpenTracing.Tag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Acnys.Core.Extensions
{
    public static class TracingExtensions
    {
        public static ISpan StartNewSpanForHandler(
            ITracer tracer,
            string handlerNamespace,
            string handlerName,
            string triggerNamespace,
            string triggerName,
            HumanReadableInformationAttribute handlerInfo = null,
            HumanReadableInformationAttribute triggerInfo = null,
            IDictionary<string, object> arguments = null)
        {
            if (arguments == null)
            {
                arguments = new Dictionary<string, object>();
            }
            var operationName = $"{handlerName}({triggerName})";
            if (triggerInfo != null && handlerInfo != null)
            {
                operationName = $"{handlerInfo.Name}";
            }

            var headerExtractor = new TextMapExtractAdapter(arguments.ToDictionary(e => e.Key, v => v.Value.ToString()));
            var previousSpan = tracer.Extract(BuiltinFormats.HttpHeaders, headerExtractor);

            ISpanBuilder spanbuilder;
            if (previousSpan != null)
            {
                spanbuilder = tracer.BuildSpan(operationName).AddReference(References.ChildOf, previousSpan);
            }
            else
            {
                spanbuilder = tracer.BuildSpan(operationName);
            }

            arguments.Remove("uber-trace-id");
            var traceExtendedHeaders = arguments.ToDictionary(e => e.Key, v => v.Value.ToString());
            var headerInjector = new TextMapInjectAdapter(traceExtendedHeaders);
            var span = spanbuilder.Start();
            tracer.Inject(span.Context, BuiltinFormats.HttpHeaders, headerInjector);
            arguments.Add("uber-trace-id", new StringValues(traceExtendedHeaders["uber-trace-id"]).First());

            EnrichWithTags(handlerNamespace, handlerName, triggerNamespace, triggerName, handlerInfo, triggerInfo, arguments, span);

            return span;
        }

        private static void EnrichWithTags(string handlerNamespace, string handlerName, string triggerNamespace, string triggerName, HumanReadableInformationAttribute handlerInfo, HumanReadableInformationAttribute triggerInfo, IDictionary<string, object> arguments, ISpan span)
        {
            span.SetTag(new StringTag("handler-namespace"), handlerNamespace);
            span.SetTag(new StringTag("handler-name"), handlerName);
            span.SetTag(new StringTag("trigger-namespace"), triggerNamespace);
            span.SetTag(new StringTag("trigger-name"), triggerName);
            if (handlerInfo != null)
            {
                span.SetTag(new StringTag("handler-info"), handlerInfo.Name);
                span.SetTag(new StringTag("handler-description"), handlerInfo.Description);
            }
            if (triggerInfo != null)
            {
                span.SetTag(new StringTag("trigger-info"), triggerInfo.Name);
                span.SetTag(new StringTag("trigger-description"), triggerInfo.Description);
            }
            foreach (var arg in arguments)
            {
                span.SetTag(new StringTag(arg.Key), arg.Value.ToString());
            }
        }
    }
}
