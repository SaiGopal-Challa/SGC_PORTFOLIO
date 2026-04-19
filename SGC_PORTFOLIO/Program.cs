using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using SGC_PORTFOLIO.Services;
using SGC_PORTFOLIO.Background;

var builder = WebApplication.CreateBuilder(args);

// Clear default configuration sources
builder.Configuration.Sources.Clear();

// Configure app settings
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    //.AddJsonFile("appsettings.Production.json", optional: true, reloadOnChange: true) // explicit
    //.AddJsonFile("appsettings.SecureProd.json", optional: true, reloadOnChange: true) // your custom locked file
    .AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllersWithViews();

// Register services for DI
builder.Services.AddScoped<BlogService>();
builder.Services.AddSingleton<AuthenticationService>();

// Register the hosted service for newsletter background processing
builder.Services.AddHostedService<NewsletterHostedService>();

// Register newsletter and clustering services
builder.Services.AddScoped<TagSummaryService>();
builder.Services.AddScoped<PdfClusterMappingService>();
builder.Services.AddScoped<UserSelectionService>();
builder.Services.AddScoped<EmailDispatchService>();
builder.Services.AddScoped<ClusterService>();
builder.Services.AddScoped<GenAiClient>();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SGC Portfolio API", Version = "v1" });
});

// Configure Gzip Compression
builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Optimal;
});
//builder.WebHost.UseUrls("http://127.0.0.1:5001");
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
var rewriteOptions = new RewriteOptions()
    .AddRewrite(@"^certs/kvpycert$", "certs/kvpycert.pdf", skipRemainingRules: true);
app.UseRewriter(rewriteOptions);

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        if (!app.Environment.IsDevelopment())
        {
            // Set cache control headers for static assets
            ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=31536000");
        }
    }
});

app.UseRouting();

app.UseAuthorization();

// Enable Swagger middleware
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SGC Portfolio API v1");
    c.RoutePrefix = "swagger";
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();







