using API.Data;
using API.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace API.Helpers
{
    public class LogUserActivity : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(
            ActionExecutingContext context, 
            ActionExecutionDelegate next)
        {
            ActionExecutedContext resultContext = await next();

            if (context.HttpContext.User.Identity?.IsAuthenticated != true)
                return;

            string memberId = resultContext.HttpContext.User.GetMemberId();

            AppDbContext dbContext = resultContext.HttpContext.RequestServices
                .GetRequiredService<AppDbContext>();

            await dbContext.Members
                .Where(x => x.Id == memberId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.LastActive, DateTime.UtcNow)
                );
        }
    }
}