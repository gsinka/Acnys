using DotBadge;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Acnys.Core.AspNet
{
    internal class BadgeDefinition
    {
        public string Label { get; }
        public Func<Task<string>> Status { get; }
        public Func<string, string> Color { get; }

        public BadgeDefinition(string label, Func<Task<string>> status, Func<string, string> color)
        {
            Label = label;
            Status = status;
            Color = color;
        }        
    }

    public interface IBadgeRegistry
    {
        void RegistrateVersionBadge();
        void RegistrateReadinessBadge();
        void RegistrateLivenessBadge();
        void RegistrateBadge(string key, string label, Func<Task<string>> status, string color);
        void RegistrateBadge(string key, string label, Func<Task<string>> status, Func<string, string> color);
    }

    public class BadgeService : IBadgeRegistry
    {
        private readonly BadgePainter _badgePainter = new BadgePainter();
        private readonly HealthCheckService _healthCheckService;

        private IDictionary<string, BadgeDefinition> _badges { get; } = new Dictionary<string, BadgeDefinition>();

        public BadgeService(HealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService;
        }

        public async Task<IActionResult> GetAllBadgeAsync()
        {
            var content = new StringBuilder();

            foreach (var definition in _badges.Values)
            {
                var status = await definition.Status();
                content.Append("<div>");
                content.Append(_badgePainter.DrawSVG(definition.Label, status, definition.Color(status), Style.Plastic));
                content.Append("</div>");
            }

            return CreateResult("text/html", content.ToString());
        }

        public async Task<IActionResult> GetBadgeAsync(string name)
        {
            if (!_badges.ContainsKey(name))
            {
                return CreateResult("image/svg+xml", _badgePainter.DrawSVG("Unkown", "Badge not found!", ColorScheme.Grey, Style.Plastic));
            }

            var definition = _badges[name];
            var status = await definition.Status();
            return CreateResult("image/svg+xml", _badgePainter.DrawSVG(definition.Label, status, definition.Color(status), Style.Plastic));
        }  

        private ContentResult CreateResult(string contentType, string content)
        {
            return new ContentResult
            {
                StatusCode = StatusCodes.Status200OK,
                ContentType = contentType,
                Content = content
            };
        }

        public void RegistrateBadge(string key, string label, Func<Task<string>> status, string color)
        {
            RegistrateBadge(key, label, status, (status) => color);
        }

        public void RegistrateBadge(string key, string label, Func<Task<string>> status, Func<string, string> color)
        {
            _badges.Add(key, new BadgeDefinition(label, status, color));
        }

        public void RegistrateVersionBadge()
        {
            RegistrateBadge("version", "Version", () => Task.FromResult(Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>().Version), ColorScheme.Blue);
        }

        public void RegistrateReadinessBadge()
        {
            RegistrateBadge("ready", "Ready", async () => (await _healthCheckService.CheckHealthAsync(c => c.Tags.Contains("Readiness"))).Status.ToString(), (status) => status.Equals(HealthStatus.Healthy.ToString()) ? ColorScheme.Green : ColorScheme.Red);
        }

        public void RegistrateLivenessBadge()
        {
            RegistrateBadge("live", "Live", async () => (await _healthCheckService.CheckHealthAsync(c => c.Tags.Contains("Liveness"))).Status.ToString(), (status) => status.Equals(HealthStatus.Healthy.ToString()) ? ColorScheme.Green : ColorScheme.Red);
        }
    }
}
