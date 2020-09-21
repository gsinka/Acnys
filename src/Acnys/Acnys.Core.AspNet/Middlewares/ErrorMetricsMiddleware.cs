﻿using Acnys.Core.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Acnys.Core.AspNet.Middlewares
{
    public class ErrorMetricsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly MetricsService _metricsService;

        public ErrorMetricsMiddleware(RequestDelegate next, MetricsService metricsService)
        {
            _next = next;
            _metricsService = metricsService;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (Exception e)
            {
                // type = var x = e.GetType().Name;
                _metricsService.AddException(e);
                throw;
            }

        }
    }
}
