using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Myportal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace Myportal.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IWebHostEnvironment _env;
    private readonly ICompositeViewEngine _viewEngine;

    public HomeController(
        ILogger<HomeController> logger, 
        IWebHostEnvironment env,
        ICompositeViewEngine viewEngine)
    {
        _logger = logger;
        _env = env;
        _viewEngine = viewEngine;
    }

    public IActionResult Index()
    {
        _logger.LogInformation("Accessed Home/Index");
        ViewData["WelcomeMessage"] = "Welcome to MyPortal";
        ViewData["Environment"] = _env.EnvironmentName;
        return View();
    }

    public IActionResult Privacy()
    {
        _logger.LogInformation("Accessed Privacy Policy");
        return View();
    }

    public IActionResult About()
    {
        _logger.LogInformation("Accessed About Page");
        var versionInfo = new {
            Version = "1.0.0",
            BuildDate = System.IO.File.GetLastWriteTime(GetType().Assembly.Location),
            Environment = _env.EnvironmentName
        };
        return View(versionInfo);
    }

    [ResponseCache(Duration = 30)]
    public IActionResult Contact()
    {
        _logger.LogInformation("Accessed Contact Page");
        return View();
    }

    [Route("/health")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult HealthCheck()
    {
        var healthInfo = new 
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Version = GetType().Assembly.GetName().Version?.ToString() ?? "1.0",
            Environment = _env.EnvironmentName
        };
        
        _logger.LogInformation("Health check: {@HealthInfo}", healthInfo);
        return Ok(healthInfo);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        var statusCode = HttpContext.Response.StatusCode;
        var exception = exceptionHandlerPathFeature?.Error;
        
        _logger.LogError(exception, "Error {StatusCode} at {Path}", statusCode, exceptionHandlerPathFeature?.Path);

        return View(new ErrorViewModel 
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            ErrorMessage = _env.IsDevelopment() ? exception?.Message : "An error occurred",
            ExceptionDetails = _env.IsDevelopment() ? exception?.ToString() : null,
            StatusCode = statusCode,
            Path = exceptionHandlerPathFeature?.Path,
            StatusCodeDescription = ReasonPhrases.GetReasonPhrase(statusCode)
        });
    }

    [Route("/error/{statusCode:int}")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult StatusCodeError(int statusCode)
    {
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var path = HttpContext.Features.Get<IStatusCodeReExecuteFeature>()?.OriginalPath;
        
        _logger.LogWarning("Status code {StatusCode} occurred for request {RequestId} at {Path}", 
            statusCode, requestId, path);

        var viewName = GetViewNameForStatusCode(statusCode);
        if (!ViewExists(viewName))
        {
            _logger.LogWarning("View {ViewName} not found, falling back to Error view", viewName);
            viewName = "Error";
        }

        return View(viewName, new ErrorViewModel
        {
            StatusCode = statusCode,
            RequestId = requestId,
            Path = path,
            StatusCodeDescription = ReasonPhrases.GetReasonPhrase(statusCode)
        });
    }

    [Route("/api/info")]
    [ResponseCache(Duration = 60)]
    public IActionResult GetAppInfo()
    {
        var info = new
        {
            AppName = "MyPortal",
            Version = GetType().Assembly.GetName().Version?.ToString() ?? "1.0.0",
            Environment = _env.EnvironmentName,
            Runtime = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
            OS = System.Runtime.InteropServices.RuntimeInformation.OSDescription
        };
        
        _logger.LogDebug("App info requested: {@AppInfo}", info);
        return Json(info);
    }

    [Authorize]
    [Route("/dashboard")]
    public IActionResult Dashboard()
    {
        _logger.LogInformation("Dashboard accessed by {User}", User.Identity?.Name);
        return View();
    }

    private string GetViewNameForStatusCode(int statusCode)
    {
        return statusCode switch
        {
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "NotFound",
            _ => "Error"
        };
    }

    private bool ViewExists(string viewName)
    {
        var result = _viewEngine.GetView("", $"~/Views/Shared/{viewName}.cshtml", false);
        return result.Success;
    }
}