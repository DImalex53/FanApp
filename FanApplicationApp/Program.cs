using Microsoft.EntityFrameworkCore;
using SpeedCalc.Repositories;
using SpeedCalc.Data;
using SpeedCalc.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddScoped<IAerodynamicsDataRepository, AerodynamicsDataRepository>();
builder.Services.AddScoped<IAerodynamicService, AerodynamicService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();