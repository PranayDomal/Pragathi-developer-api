using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PragathiApi
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ServiceKeyAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var expected = context.HttpContext.RequestServices
                .GetRequiredService<IConfiguration>()["Internal:ServiceKey"];
            var given = context.HttpContext.Request.Headers["X-Service-Key"].ToString();

            if (string.IsNullOrEmpty(expected) || !Same(given, expected))
            {
                context.Result = new UnauthorizedResult();
                return;
            }
            await next();
        }

        private static bool Same(string a, string b)
        {
            var x = SHA256.HashData(Encoding.UTF8.GetBytes(a));
            var y = SHA256.HashData(Encoding.UTF8.GetBytes(b));
            return CryptographicOperations.FixedTimeEquals(x, y);
        }
    }
}
