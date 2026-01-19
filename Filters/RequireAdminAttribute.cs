using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Bc_exercise_and_healthy_nutrition.Filters
{
    public class RequireAdminAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var role = context.HttpContext.Session.GetString("UserRole");

            if (role != "Admin")
            {
                context.Result = new RedirectToActionResult(
                    "Index", "Welcome", null);
            }
        }
    }
}