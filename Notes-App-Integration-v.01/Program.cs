
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Notes_App_Integration_v._01.Data;
using Notes_App_Integration_v._01.Model;
using Notes_App_Integration_v._01.ModelViews;
using Notes_App_Integration_v._01.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddIdentity<AccountUserModel, AccountRoleModel>(options =>
{
    options.SignIn.RequireConfirmedEmail = true;
    options.Lockout.MaxFailedAccessAttempts = 6;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

}).AddEntityFrameworkStores< AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("MyConnection")
    ));

builder.Services.AddTransient<IEmailSender, EmailSender>();

//builder.Services.AddTransient<EmailSender>();

builder.Services.AddCors((setup)=>
{
    setup.AddPolicy("default", (options) =>
    {
        options.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("default");

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseAuthentication();

app.MapControllers();

app.Run();
