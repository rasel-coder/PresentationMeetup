using Microsoft.EntityFrameworkCore;
using PresentationMeetup.Data;
using PresentationMeetup.Services;
using PresentationMeetup.Utility;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
builder.Services.AddSignalR();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<PresentationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DB")));

builder.Services.AddScoped<PresentationService, PresentationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.MapHub<CollaborationHub>("/collaborationHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
