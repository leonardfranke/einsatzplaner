using Blazored.LocalStorage;
using Firebase.Auth.Repository;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Web;
using Web.Checks;
using Web.Manager;
using Web.Services;
using Web.Services.Member;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

using var http = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";
var configFile = $"appsettings.{env}.json";
using var response = await http.GetAsync(configFile);
using var stream = await response.Content.ReadAsStreamAsync();
var config = new ConfigurationBuilder()
    .AddJsonStream(stream)
    .Build();
builder.Configuration.AddConfiguration(config);

builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<AuthManager>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<AuthManager>());
builder.Services.AddScoped<IAuthManager>(provider => provider.GetRequiredService<AuthManager>());
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IHelperService, HelperService>();
builder.Services.AddScoped<IEventCategoryService, EventCategoryService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IRequirementGroupService, RequirementsGroupService>();
builder.Services.AddScoped<ILoginCheck, LoginCheck>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IUserRepository, FileUserRepository>(serviceProvider => new FileUserRepository("localUser"));

builder.Services.AddHttpClient("BACKEND", client => client.BaseAddress = new Uri(builder.Configuration["BACKEND_ADDRESS"]));

builder.Services.AddOptions();
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddBlazorBootstrap();

await builder.Build().RunAsync();
