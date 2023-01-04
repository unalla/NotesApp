using CloudNotes;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);
Startup startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);
var app = builder.Build();
startup.Configure(app);
