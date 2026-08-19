using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Yellowtail.API.Validators;

namespace Yellowtail.API.Filters;

/// <summary>
/// Automatically validates action arguments using FluentValidation. For each action argument,
/// resolves a matching <see cref="IValidator{T}"/> from the DI container (if one is registered
/// for that argument's type) and validates it before the action executes. Registered globally
/// in <c>Program.cs</c> so no controller needs to inject a validator or call it manually.
/// </summary>
public class ValidationFilter : IAsyncActionFilter
{
    /// <summary>
    /// Validates every action argument that has a matching registered validator, short-circuiting
    /// the pipeline with a 400 <see cref="ValidationProblemDetails"/> response on the first failure;
    /// otherwise invokes the action.
    /// </summary>
    /// <param name="context">The context for the action being executed.</param>
    /// <param name="next">The delegate that invokes the next filter or the action itself.</param>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (context.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator)
            {
                continue;
            }

            var validationContext = new ValidationContext<object>(argument);
            var result = await validator.ValidateAsync(validationContext);

            if (!result.IsValid)
            {
                context.Result = new BadRequestObjectResult(result.ToProblemDetails());
                return;
            }
        }

        await next();
    }
}
