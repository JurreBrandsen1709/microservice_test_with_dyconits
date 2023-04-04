using System.Diagnostics;

namespace DotnetDyconits.Middleware
{
    public class TimingMiddleware
    {
        private readonly ILogger<TimingMiddleware> _logger;
        private readonly RequestDelegate _next;
        private readonly IHostEnvironment _env;
        private readonly string _message;

        public TimingMiddleware(ILogger<TimingMiddleware> logger, RequestDelegate next, IHostEnvironment env, string message)
        {
            _logger = logger;
            _next = next;
            _env = env;
            _message = message;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path == "/hello")
            {
                await HandleHelloRequest(context);
            }
            else if (context.Request.Path == "/status")
            {
                await HandleStatusRequest(context);
            }
            else if (context.Request.Path == "/goodbye")
            {
                await HandleGoodbyeRequest(context);
            }
            else if (context.Request.Path == "/time")
            {
                await HandleTimeRequest(context);
            }
            else
            {
                var stopwatch = Stopwatch.StartNew();
                await _next(context);
                stopwatch.Stop();
                _logger.LogInformation($"Request {context.Request.Path} took {stopwatch.ElapsedMilliseconds}ms");
            }
        }

        private async Task HandleGoodbyeRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync("Goodbye!");
        }

        private async Task HandleHelloRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync(_message);
        }

        private async Task HandleStatusRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync("Server is running");
        }

        private async Task HandleTimeRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync(DateTime.Now.ToString());
        }
    }

    public static class TimingMiddlewareExtensions
    {
        public static IApplicationBuilder UseTimingMiddleware(this IApplicationBuilder builder, string message)
        {
            return builder.UseMiddleware<TimingMiddleware>(message);
        }

        public static IServiceCollection AddTimingMiddleware(this IServiceCollection services)
        {
            services.AddTransient<TimingMiddleware>();
            return services;
        }

        public static IServiceCollection AddTimingMiddleware(this IServiceCollection services, string message)
        {
            services.AddTransient<TimingMiddleware>(s => new TimingMiddleware(
                s.GetService<ILogger<TimingMiddleware>>(),
                s.GetService<RequestDelegate>(),
                s.GetService<IHostEnvironment>(),
                message
            ));
            return services;
        }
    }
}
