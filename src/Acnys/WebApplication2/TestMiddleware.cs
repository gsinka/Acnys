using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace WebApplication2
{
    public class TestMiddleware : IMiddleware
    {
        private readonly UserContext _userContext;

        public TestMiddleware(UserContext userContext)
        {
            _userContext = userContext;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            await next(context);
        }
    }

    public class UserContext
    {
        public string UserName { get; set; }

        public UserContext()
        {
            
        }
    }
}
