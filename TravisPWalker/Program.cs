using Library.Database.Auth;
using Library.Database.Auth.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Principal;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration;

// Add services to the container.
// For EF
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("Main"), x => x.MigrationsAssembly("TravisPWalker")));


// For Asp.net Identity
builder.Services.AddIdentity<SecureUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddSession();
builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IAuthConfigManager, AuthConfigManager>();
builder.Services.AddTransient<ITokenService, TokenService>();

// Adding JWT Auth
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
// Adding Jwt Bearer
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = Convert.ToBoolean(configuration["JWT:RequireHttpsMetadata"]);
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.FromSeconds(60),
        ValidAudiences = configuration.GetSection("JWT:ValidAudiences").Get<string[]>(),
        ValidIssuers = configuration.GetSection("JWT:ValidIssuers").Get<string[]>(),
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"])),
    };
});
var app = builder.Build();

//lets make sure our user roles are created when the app starts...
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<Library.Database.Auth.SecureUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await Seeder.SeedRolesAsync(userManager, roleManager);
        await Seeder.SeedRoleClaimsAsync(userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogError(ex, "An error occurred seeding the DB.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSession();
app.Use(async (context, next) =>
{
    var token = context.Session.GetString("AccessToken");
    if (!string.IsNullOrEmpty(token))
    {
        Debug.WriteLine("**** START OF FOUND TOKEN ****");
        Debug.WriteLine(token);
        Debug.WriteLine("**** END OF FOUND TOKEN ****");
        context.Request.Headers.Add("Authorization", "Bearer " + token);
    }
    await next();
});


app.UseHttpsRedirection();
app.UseStaticFiles();

app.MapRazorPages();

app.UseAuthentication();
app.UseRouting();
// must be between authentication and authorization
// needed because of jwt auth
// if we were using cookies it would be as simple as
// services.ConfigureApplicationCookie(options => options.LoginPath = "pathToLoginPage");
app.UseStatusCodePages(async context =>
{
    var request = context.HttpContext.Request;
    var response = context.HttpContext.Response;
    var path = request.Path.Value ?? "";

    if (response.StatusCode == (int)HttpStatusCode.Unauthorized)
    {
        response.Redirect($"/Account/RefreshTokenAccess?redirectURL={path}");
    }
    else
    {
        response.Redirect("/Error");
    }
});

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
