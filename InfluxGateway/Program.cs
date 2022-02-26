var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<IInfluxDatabase, InfluxDatabase>();
builder.Services.Configure<InfluxConnectionSettings>(builder.Configuration.GetSection(nameof(InfluxConnectionSettings)));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
