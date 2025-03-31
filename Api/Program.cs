using Api.Manager;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(type => type.FullName);
});

var _myPolicy = "Policy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(_myPolicy, policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";
var configFile = $"appsettings.{env}.json";
var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile(configFile).Build();
builder.Configuration.AddConfiguration(config);

var credentials = await GoogleCredential.FromFileAsync(builder.Configuration["FIREBASE_JSON_FILE"], CancellationToken.None);
FirebaseApp.Create(new AppOptions()
{
    Credential = credentials,
    ProjectId = "1077768805408",
});
if(FirebaseApp.DefaultInstance == null)
{
    throw new Exception("FirebaseApp.DefaultInstance is null");
}

builder.Services.AddSingleton<IFirestoreManager, FirestoreManager>();
builder.Services.AddSingleton<IRequirementGroupManager, RequirementGroupManager>();
builder.Services.AddSingleton<IRequirementGroupManager, RequirementGroupManager>();
builder.Services.AddSingleton<IEventCategoryManager, EventCategoryManager>();
builder.Services.AddSingleton<IDepartmentManager, DepartmentManager>();
builder.Services.AddSingleton<IGroupManager, GroupManager>();
builder.Services.AddSingleton<IRoleManager, RoleManager>();
builder.Services.AddSingleton<IMemberManager, MemberManager>();
builder.Services.AddSingleton<IEventManager, EventManager>();
builder.Services.AddSingleton<IUserManager, UserManager>();
builder.Services.AddSingleton<IHelperManager, HelperManager>();
builder.Services.AddSingleton<IUpdatedTimeManager, UpdatedTimeManager>();
builder.Services.AddHttpClient();

var app = builder.Build();

app.UseCors(_myPolicy);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
