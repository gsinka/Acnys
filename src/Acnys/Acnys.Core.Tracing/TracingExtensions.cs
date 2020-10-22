using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Acnys.Core.Eventing.Abstractions;
using Acnys.Core.Request.Abstractions;
using Acnys.Core.Tracing.Attributes;
using Acnys.Core.ValueObjects;
using Autofac;
using OpenTracing;
using OpenTracing.Propagation;
using OpenTracing.Tag;
using OpenTracing.Util;
using Serilog;

namespace Acnys.Core.Tracing
{
    public static class TracingExtensions
    {
        public static ContainerBuilder AddTracing(this ContainerBuilder builder)
        {
            builder.RegisterTracingContext();

            //builder.AddRequestSenderTracingBehaviour();

            builder.AddCommandTracingBehaviour();
            builder.AddCommandSenderTracingBehaviour();

            builder.AddQueryTracingBehaviour();
            builder.AddQuerySenderTracingBehaviour();

            builder.AddEventTracingBehaviour();
            builder.AddEventPublisherTracingBehaviour();

            return builder;
        }

        public static ContainerBuilder RegisterTracingContext(this ContainerBuilder builder)
        {
            builder.RegisterType<TracingContext>().InstancePerLifetimeScope().AsSelf();
            return builder;
        }

        public static ContainerBuilder AddCommandTracingBehaviour(this ContainerBuilder builder)
        {
            builder.RegisterType<CommandTracingBehavior>().InstancePerLifetimeScope().AsSelf();
            builder.RegisterDecorator(typeof(CommandTracingBehavior), typeof(IDispatchCommand));
            return builder;
        }

        public static ContainerBuilder AddQueryTracingBehaviour(this ContainerBuilder builder)
        {
            builder.RegisterType<QueryTracingBehavior>().InstancePerLifetimeScope().AsSelf();
            builder.RegisterDecorator(typeof(QueryTracingBehavior), typeof(IDispatchQuery));
            return builder;
        }

        public static ContainerBuilder AddEventTracingBehaviour(this ContainerBuilder builder)
        {
            builder.RegisterType<EventTracingBehavior>().InstancePerLifetimeScope().AsSelf();
            builder.RegisterDecorator(typeof(EventTracingBehavior), typeof(IDispatchEvent));
            return builder;
        }

        public static ContainerBuilder AddEventPublisherTracingBehaviour(this ContainerBuilder builder)
        {
            builder.RegisterType<EventPublisherTracingBehavior>().InstancePerLifetimeScope().AsSelf();
            builder.RegisterDecorator(typeof(EventPublisherTracingBehavior), typeof(IPublishEvent));
            return builder;
        }

        //public static ContainerBuilder AddRequestSenderTracingBehaviour(this ContainerBuilder builder)
        //{
        //    builder.RegisterType<RequestSenderTracingBehavior>().InstancePerLifetimeScope().AsSelf();
        //    builder.RegisterDecorator(typeof(RequestSenderTracingBehavior), typeof(ISendRequest));
        //    return builder;
        //}

        public static ContainerBuilder AddCommandSenderTracingBehaviour(this ContainerBuilder builder)
        {
            builder.RegisterType<CommandSenderTracingBehavior>().InstancePerLifetimeScope().AsSelf();
            builder.RegisterDecorator(typeof(CommandSenderTracingBehavior), typeof(ISendCommand));
            return builder;
        }

        public static ContainerBuilder AddQuerySenderTracingBehaviour(this ContainerBuilder builder)
        {
            builder.RegisterType<QuerySenderTracingBehavior>().InstancePerLifetimeScope().AsSelf();
            builder.RegisterDecorator(typeof(QuerySenderTracingBehavior), typeof(ISendQuery));
            return builder;
        }

        public static string TraceId(this IDictionary<string, object> arguments)
        {
            if (arguments == null || !arguments.ContainsKey(RequestConstants.TraceId)) return null;

            var value = arguments[RequestConstants.TraceId];

            return value.ToString();
        }

        public static IDictionary<string, object> UseTraceId(this IDictionary<string, object> arguments, string traceId)
        {
            if (string.IsNullOrWhiteSpace(traceId)) return arguments;
            if (!arguments.ContainsKey(RequestConstants.TraceId))
            {
                arguments.Add(RequestConstants.TraceId, traceId);
            }
            else
            {
                arguments[RequestConstants.TraceId] = traceId;
            }
            return arguments;
        }

        public static TracingContext Update(this TracingContext tracingContext, ILogger log, IEvent evnt, IDictionary<string, object> arguments)
        {
            tracingContext.TraceId = arguments?.TraceId();
            log.Debug("Tracing context {contextId} updated: {@context}", tracingContext.GetHashCode(), tracingContext);

            return tracingContext;
        }

        public static IDictionary<string, object> UpdateWithTracingContext(this IDictionary<string, object> arguments, ILogger log, TracingContext tracingContext)
        {
            arguments.UseTraceId(tracingContext.TraceId);
            log.Debug("Updating arguments from tracing context {contextId}: {@context}", tracingContext.GetHashCode(), tracingContext);

            return arguments;
        }

        public static TracingContext Update(this TracingContext tracingContext, ILogger log, IRequest request, IDictionary<string, object> arguments)
        {
            tracingContext.TraceId = arguments?.TraceId();
            log.Debug("Tracing context {contextId} updated: {@context}", tracingContext.GetHashCode(), tracingContext);

            return tracingContext;
        }

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
            if (handlerInfo != null)
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
            tracer.ScopeManager.Activate(span, true);
            tracer.Inject(span.Context, BuiltinFormats.HttpHeaders, headerInjector);
            arguments.Add("uber-trace-id", traceExtendedHeaders["uber-trace-id"]);

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