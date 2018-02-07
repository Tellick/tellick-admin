using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using tellick_admin.Repository;

namespace tellick_admin {
    public class Startup {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services) {
            services.AddDbContext<TellickAdminContext>(options => options
                .UseSqlServer(Settings.ConnectionString, providerOptions=>providerOptions.CommandTimeout(60))
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

            services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(cfg => {
                cfg.RequireHttpsMetadata = false;
                cfg.SaveToken = true;
                cfg.TokenValidationParameters = new TokenValidationParameters() {
                    ValidIssuer = Settings.JwtIssuer,
                    ValidAudience = Settings.JwtAudience, 
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Settings.JwtSigningKey))
                };
                cfg.Events = new JwtBearerEvents() {
                    OnAuthenticationFailed = c => {
                        c.NoResult();
                        c.Response.StatusCode = 401;
                        return c.Response.WriteAsync("Invalid Token");
                    }
                };
            });
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            app.UseMvcWithDefaultRoute();
        }
    }
}
