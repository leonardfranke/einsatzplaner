using Api.Manager;
using Api.Manager.Tasks;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
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

if(builder.Environment.IsProduction())
{
    var gTasksClientBuilder = new CloudTasksClientBuilder();
    builder.Services.AddSingleton(sp => gTasksClientBuilder.Build());
}
else
{
    builder.Services.AddSingleton<CloudTasksClient>(sp => new CloudTaskMock());
}

    var firestoreDbBuilder =
        builder.Environment.IsProduction() ?
        new FirestoreDbBuilder() { ProjectId = "einsatzplaner" } :
        new FirestoreDbBuilder() { EmulatorDetection = Google.Api.Gax.EmulatorDetection.EmulatorOnly, ProjectId = "emulator" };
builder.Services.AddSingleton(sp => firestoreDbBuilder.Build());

if (builder.Environment.IsProduction())
{
    var firebaseCredentials = Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS");
    FirebaseApp.Create(new AppOptions()
    {
        Credential = GoogleCredential.FromJson(firebaseCredentials),
        ProjectId = "einsatzplaner",
    });
}
else
{    
    FirebaseApp.Create(new AppOptions { Credential = GoogleCredential.FromAccessToken("mock-tocken"), ProjectId = "emulator"});
}

var firebaseAuthApiBasePath =
    builder.Environment.IsProduction() ?
    "https://identitytoolkit.googleapis.com/v1/" :
    "http://127.0.0.1:9099/identitytoolkit.googleapis.com/v1/";
builder.Services.AddHttpClient("FIREBASE_AUTH", client => client.BaseAddress = new Uri(firebaseAuthApiBasePath));

builder.Services.AddSingleton<IUserManager, UserManager>();
builder.Services.AddSingleton<IRequirementGroupManager, RequirementGroupManager>();
builder.Services.AddSingleton<IEventCategoryManager, EventCategoryManager>();
builder.Services.AddSingleton<IDepartmentManager, DepartmentManager>();
builder.Services.AddSingleton<IGroupManager, GroupManager>();
builder.Services.AddSingleton<IRoleManager, RoleManager>();
builder.Services.AddSingleton<IMemberManager, MemberManager>();
builder.Services.AddSingleton<IEventManager, EventManager>();
builder.Services.AddSingleton<IHelperManager, HelperManager>();
builder.Services.AddSingleton<IUpdatedTimeManager, UpdatedTimeManager>();
builder.Services.AddHttpClient();
var optimizerEndpoint = Environment.GetEnvironmentVariable("OPTIMIZER_ENDPOINT");
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
