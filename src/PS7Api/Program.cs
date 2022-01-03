using Microsoft.EntityFrameworkCore;
using PS7Api.Models;
using PS7Api.Utilities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<Ps7Context>(opt => opt.UseInMemoryDatabase("Ps7"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        DataGenerator.Initialize(scope.ServiceProvider.GetRequiredService<Ps7Context>());
    }

    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();