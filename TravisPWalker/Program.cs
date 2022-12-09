using Library.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
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
        ClockSkew = TimeSpan.Zero,
        ValidAudiences = configuration.GetSection("JWT:ValidAudiences").Get<string[]>(),
        ValidIssuers = configuration.GetSection("JWT:ValidIssuers").Get<string[]>(),
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"])),
    };
    //options.SecurityTokenValidators.Clear();
    //options.SecurityTokenValidators.Add(new JwtSecurityTokenHandler
    //{
    //    MapInboundClaims = false
    //});
    //options.TokenValidationParameters.NameClaimType = "name";
    //options.TokenValidationParameters.RoleClaimType = "role";
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
        var userManager = services.GetRequiredService<UserManager<SecureUser>>();
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
    app.UseExceptionHandler("/Home/Error");
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSession();
app.Use(async (context, next) =>
{
    var token = context.Session.GetString("Token");
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

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
