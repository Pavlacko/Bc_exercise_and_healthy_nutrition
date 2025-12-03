using Bc_exercise_and_healthy_nutrition.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews();
builder.Services.AddSession();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseInMemoryDatabase("NutritionDb");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSession(); 

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Welcome}/{action=Index}/{id?}");  

app.Run();