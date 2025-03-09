using Blazored.LocalStorage;
using Firebase.Auth.Repository;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Web;
using Web.Checks;
using Web.Manager;
using Web.Manager.Auth;
using Web.Services;
using Web.Services.Member;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = Environment.GetEnvironmentVariable("API_BASE_URL");
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IBackendManager, BackendManager>();
builder.Services.AddScoped<AuthManager>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => provider.GetRequiredService<AuthManager>());
builder.Services.AddScoped<IAuthManager>(provider => provider.GetRequiredService<AuthManager>());
builder.Services.AddScoped<IFirebaseAuthService, FirebaseAuthService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IHelperService, HelperService>();
builder.Services.AddScoped<IEventCategoryService, EventCategoryService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IRequirementGroupService, RequirementsGroupService>();
builder.Services.AddScoped<ILoginCheck, LoginCheck>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IUserRepository, FileUserRepository>(serviceProvider => new FileUserRepository("localUser"));

builder.Services.AddOptions();
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddBlazorBootstrap();

await builder.Build().RunAsync();
