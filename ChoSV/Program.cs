
using ChoSV.Configurations;
using ChoSV.Data;
using ChoSV.Middlewares;
using ChoSV.Models.Entities;
using ChoSV.Services;
using ChoSV.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ChoSV
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultSQLConnection")));

            builder.Services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
                options.AddPolicy("UserPolicy", policy => policy.RequireRole("User", "Admin"));
            });


            builder.Services.AddIdentity<User, IdentityRole>(options =>
            {
                //options.SignIn.RequireConfirmedAccount = true; // can phai confirm email khi dang ky
                options.SignIn.RequireConfirmedEmail = false;
                options.Password.RequireDigit = true; // password phai co it nhat 1 chu so
                options.Password.RequireLowercase = true; // password phai co it nhat 1 chu thuong
                options.Password.RequireNonAlphanumeric = true; // password phai co it nhat 1 ky tu dac biet
                options.Password.RequireUppercase = true; // password phai co it nhat 1 chu hoa
                options.Password.RequiredLength = 8; // password phai co it nhat 8 ky tu
                options.User.RequireUniqueEmail = true; // email phai la duy nhat
            })
    .AddEntityFrameworkStores<ApplicationDBContext>()
    .AddDefaultTokenProviders(); // de reset password, confirm email, ...
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                options.DefaultChallengeScheme =
                options.DefaultForbidScheme =
                options.DefaultScheme =
                options.DefaultSignInScheme =
                options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["JWT:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["JWT:Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                    System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT:SigningKey"]!)
                    )
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                            path.StartsWithSegments("/chathub"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder => builder.WithOrigins("http://localhost:5173")
                                      .AllowAnyHeader()
                                      .AllowAnyMethod()
                                      .AllowCredentials());
                options.AddPolicy("AllowAll",
                    builder => builder.AllowAnyOrigin()
                                      .AllowAnyHeader()
                                      .AllowAnyMethod());
            });

            builder.Services.Configure<IdentityOptions>(options =>
            {
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // khóa 5 phút
                options.Lockout.MaxFailedAccessAttempts = 3; // sai 3 lần sẽ khóa
                options.Lockout.AllowedForNewUsers = true; // áp dụng cho cả user mới
            });

            var emailConfig = builder.Configuration
                .GetSection("EmailConfiguration")
                .Get<EmailConfiguration>();
            builder.Services.AddSingleton(emailConfig);

            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IUserService, UserService>();


            var app = builder.Build();

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("AllowAll");

            app.UseAuthentication();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
