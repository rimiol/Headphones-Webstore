using Headphones_Webstore.Data;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSwaggerGen();

builder.Services.AddLogging(loggingBuilder => {
    loggingBuilder.AddConsole();
    loggingBuilder.AddDebug();
});


var app = builder.Build();

app.UseCors("AllowAll");

// Добавление middleware Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler(a => a.Run(async context =>
{
    context.Response.ContentType = "application/json";
    var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
    var exception = exceptionHandlerPathFeature?.Error;

    await context.Response.WriteAsync(JsonSerializer.Serialize(new
    {
        type = "unhandled",
        message = "Критическая ошибка",
        details = exception?.Message
    }));
}));

app.MapGet("/", () => Results.Redirect("/index.html"));

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseCors("AllowProduction");
app.UseAuthorization();

app.MapRazorPages();

app.MapControllers();

app.Run();
