using Microsoft.EntityFrameworkCore;
using ShRt.Data;
using ShRt.Models;
using ShRt.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<LinksContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("LinksContext")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<LinksContext>();
        var created = context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error has been found while creating DB");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var post = LinkShortener.PostLink;
var fallback = LinkShortener.FallbackLink;

app.MapPost("/", (LinkDto link, LinksContext db, HttpContext ctx) => post(link, db, ctx));
app.MapFallback((LinksContext db, HttpContext ctx) => fallback(db, ctx));

app.Run();