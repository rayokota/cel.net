using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cel.Server;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddGrpc();
        services.AddGrpc(options =>
        {
            {
                options.Interceptors.Add<ServerLoggerInterceptor>();
                options.EnableDetailedErrors = true;
            }
        });
        services.AddGrpcReflection();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, 
        IHostApplicationLifetime lifetime)
    {
        if (env.IsDevelopment())
            app.UseDeveloperExceptionPage();
        else
            app.UseExceptionHandler("/Error");

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGrpcService<ConformanceServiceImpl>();
            endpoints.MapGrpcReflectionService();
        });
        
        lifetime.ApplicationStarted.Register(() => {
            Console.WriteLine("Listening on {0}:{1}", "localhost", 5000);
        });
    }

    public class ServerLoggerInterceptor : Interceptor
    {
        private readonly ILogger<ServerLoggerInterceptor> _logger;

        public ServerLoggerInterceptor(ILogger<ServerLoggerInterceptor> logger)
        {
            _logger = logger;
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            //LogCall<TRequest, TResponse>(MethodType.Unary, context);

            try
            {
                return await continuation(request, context);
            }
            catch (Exception ex)
            {
                // Note: The gRPC framework also logs exceptions thrown by handlers to .NET Core logging.
                _logger.LogError(ex, $"Error thrown by {context.Method}.");

                throw;
            }
        }
    }
}