using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Bc_exercise_and_healthy_nutrition.Filters
{
    public class RequireLoginAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var loggedIn = context.HttpContext.Session.GetString("LoggedIn");
            if (loggedIn != "true")
            {
                context.Result = new RedirectToActionResult("Login", "User", null);
            }
        }
    }
}