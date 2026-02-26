using SolarWindsTask2.Clients;
using SolarWindsTask2.Interfaces;
using SolarWindsTask2.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddControllers();


builder.Services.AddHttpClient<IRickAndMortyClient, RickAndMortyClient>(c =>
{
    c.BaseAddress = new Uri("https://rickandmortyapi.com/api/");
});


builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<ITopPairsService, TopPairsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
