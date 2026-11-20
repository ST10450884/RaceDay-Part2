
using RaceDay.Mvc.Services;

var builder = WebApplication.CreateBuilder(args);

// Register MVC
builder.Services.AddControllersWithViews();

// Enable session storage
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.Name = "RaceDay.Mvc.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// Allow controllers to access the current HTTP context
builder.Services.AddHttpContextAccessor();

// Register the RaceDay API HTTP client
builder.Services.AddHttpClient("RaceDayApi", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ApiSettings:BaseUrl"]!
    );
});

builder.Services.AddScoped<ApiService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

// Make the RaceDay login page the default landing page
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
