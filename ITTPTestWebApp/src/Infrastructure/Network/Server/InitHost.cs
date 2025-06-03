using System;
using System.Runtime.CompilerServices;
using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using ITTPTestWebApp.Data;
using ITTPTestWebApp.Logging;

namespace ITTPTestWebApp.Network
{
    partial class Server
    {
        private bool TryInitHost()
        {
            try
            {
                var builder = WebApplication.CreateBuilder();
                builder.WebHost.UseUrls(Config.Instance.Read("ServerConnectionUrl"));

                AddAuthenticationSettings(builder.Services);

                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddControllers();

                AddSwaggerGenSettings(builder.Services);

                builder.Logging.ClearProviders();
                builder.Logging.AddProvider(new HostLoggerProvider());

                _App = builder.Build();

                _App.Use(async (ctx, next) => { ctx.Request.EnableBuffering(); await next(); });

                _App.UseSwagger();
                _App.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ITTPTestWebApp API");
                    c.DefaultModelsExpandDepth(-1);
                });

                _App.UseRouting();

                _App.UseAuthentication();
                _App.UseAuthorization();

                _App.UseMiddleware<RequestTimingMiddleware>();

                _App.MapControllers();
                foreach (var controller in _Controllers)
                { controller.Register(_App); }
                _Controllers.Clear();

                return _App != null;
            }
            catch (Exception ex) { Logger.Instance.Log(ex); return false; }
        }

        private void AddAuthenticationSettings(IServiceCollection services)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _JwtIssuer,

                    ValidateAudience = true,
                    ValidAudience = _JwtAudience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_JwtSecretKey)),

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

            services.AddAuthorization();
        }

        private void AddSwaggerGenSettings(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.EnableAnnotations();
                c.AddSecurityDefinition("Authorization", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme."
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id   = "Authorization"
                                }
                            },
                            new string[]{}
                        }
                    });
            });
        }
    }
}
