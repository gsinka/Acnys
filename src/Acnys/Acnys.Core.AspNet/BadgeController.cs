using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Acnys.Core.AspNet
{
    public class BadgeController : Controller
    {
        private readonly BadgeService _badgeService;

        public BadgeController(BadgeService badgeService)
        {
            _badgeService = badgeService;
        }

        public async Task<IActionResult> Get([FromRoute(Name = "name")]string name)
        {
            return string.IsNullOrWhiteSpace(name) ? await _badgeService.GetAllBadgeAsync() : await _badgeService.GetBadgeAsync(name);
        }
    }
}
