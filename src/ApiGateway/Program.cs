using OrdersApi.Client;
using Refit;

namespace ApiGateway;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        ConfigureServices(builder);

        var app = builder.Build();

        ConfigureApp(app);

        app.Run();
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddSwaggerGen();

        builder.Services.AddRefitClient<IOrdersApi>()
            .ConfigureHttpClient(c =>
                c.BaseAddress = new Uri(builder.Configuration["Services:Orders:BaseUrl"]!))
            .AddStandardResilienceHandler();

        builder.Services.AddReverseProxy()
            .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
    }

    private static void ConfigureApp(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthorization();
        app.MapControllers();
        app.MapReverseProxy();
    }
}
