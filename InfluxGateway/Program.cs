
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<InfluxGateway.IInfluxDatabase, InfluxGateway.InfluxDatabase>();
builder.Services.AddSingleton<InfluxGateway.IInfluxConnectionSettings, InfluxGateway.InfluxConnectionSettings>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
