using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.EntityFrameworkCore;
using System;
using UrlShortener.Api;
using URLShortenerApi.Entities;
using URLShortenerApi.Extensions;
using URLShortenerApi.Models;
using URLShortenerApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("Database")));

builder.Services.AddScoped<UrlShorteningService>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.ApplyMigrations();
}

app.MapPost("api/shorten", async (
    ShortenUrlRequest request,
    UrlShorteningService urlShorteningService,
    AppDbContext dbContext,
    HttpContext httpContext) =>
{
    if (!Uri.TryCreate(request.Url, UriKind.Absolute, out _))
    {
        return Results.BadRequest("The specified URL is invalid");
    }
    var code = await urlShorteningService.GenerateUniqueCode();
    var shortenedUrl = new ShortenedUrl()
    {
        Id = Guid.NewGuid(),
        LongUrl = request.Url,
        Code = code,
        ShortUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/api/{code}",
        CreatedOnUtc = DateTime.Now
    };

    dbContext.ShortenedUrls.Add(shortenedUrl);
    await dbContext.SaveChangesAsync();

    return Results.Ok(shortenedUrl.ShortUrl);
});



app.MapGet("api/{code}", async (string code, AppDbContext dbContext) =>
{
    var shortenedUrl = await dbContext.ShortenedUrls.FirstOrDefaultAsync(s => s.Code == code);

    if (shortenedUrl is null)
    {
        return Results.NotFound();
    }

    return Results.Redirect(shortenedUrl.LongUrl);
});

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
