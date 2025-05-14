using Cutify.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Cutify.Web.Controllers;

public abstract class BaseController : Controller
{
    protected readonly ILogService _logService;

    protected BaseController(ILogService logService)
    {
        _logService = logService;
    }

    protected async Task LogErrorAsync(string message, Exception? exception = null)
    {
        await _logService.LogErrorAsync(
            message,
            exception,
            GetType().Name,
            User.Identity?.Name,
            Request.Path,
            Request.Method);
    }

    protected async Task LogInfoAsync(string message)
    {
        await _logService.LogInfoAsync(
            message,
            GetType().Name,
            User.Identity?.Name,
            Request.Path,
            Request.Method);
    }
} 