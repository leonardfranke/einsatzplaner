using Api.Manager;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Tasks.V2;

var builder = WebApplication.CreateBuilder(args);
var configFile = $"appsettings.{builder.Environment.EnvironmentName}.json";
var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile(configFile).Build();
builder.Configuration.AddConfiguration(config);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(type => type.FullName);
});

var credentials = await GoogleCredential.FromFileAsync(builder.Configuration["SERVICE_ACCOUNT_CREDENTIALS"], CancellationToken.None);
FirebaseApp.Create(new AppOptions()
{
    Credential = credentials,
    ProjectId = "1077768805408",
});
var gTasksClientBuilder = new CloudTasksClientBuilder() { GoogleCredential = credentials };

builder.Services.AddSingleton(sp => gTasksClientBuilder.Build());
builder.Services.AddSingleton<IFirestoreManager, FirestoreManager>();
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
var optimizerEndpoint = builder.Configuration["OPTIMIZER_END_POINT"] ?? throw new ArgumentNullException("OPTIMIZER_END_POINT", "Argument is missing in configuration file");
builder.Services.AddSingleton<ITaskManager>(sp => new TaskManager(sp.GetRequiredService<CloudTasksClient>(), optimizerEndpoint));

var app = builder.Build();
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}




app.UseAuthorization();

app.MapControllers();

app.Run();
