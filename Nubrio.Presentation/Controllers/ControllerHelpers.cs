using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace Nubrio.Presentation.Controllers;

public static class ControllerHelpers
{
    public static ActionResult FromResult(this ControllerBase controllerBase, Result result)
    {
        return new ObjectResult(result);
    }
}