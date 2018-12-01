using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using quizrtAuthServer.Models;

namespace quizrt_auth_server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                       // ValidIssuer = "http://localhost:5050",
                       // ValidAudience = "http://localhost:5050",
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("superSecretKey@007"))
                    };
            });

            // services.AddCors(o => o.AddPolicy("AllowSpecificOrigin", builder =>
            // builder.AllowAnyHeader()
            //     .AllowAnyMethod()
            //     .AllowAnyOrigin()
            //     .AllowCredentials()
            //     )
            // );

            services.AddCors(options =>
            {
                options.AddPolicy("CORS",
                corsPolicyBuilder => corsPolicyBuilder
                // Apply CORS policy for any type of origin
                .AllowAnyMethod()
                // Apply CORS policy for any type of http methods
                .AllowAnyHeader()
                // Apply CORS policy for any headers
                .AllowCredentials()
                .AllowAnyOrigin()
                // .WithOrigins ("http://localhost:4200","http:localhost:4201")                
                );
                // Apply CORS policy for all users
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            var connString = Environment.GetEnvironmentVariable("SQLSERVER_HOST") ?? "Server=localhost\\SQLEXPRESS;Database=QuizRTAuthDb;Trusted_Connection=True;";
            services.AddDbContext<AuthContext>(options => options.UseSqlServer(connString));
            services.AddScoped<IAuth, AuthRepo>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseCors("CORS");
            app.UseMvc();
        }
    }
}
