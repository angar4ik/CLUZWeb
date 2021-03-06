using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CLUZWeb.Areas.Identity;
using CLUZWeb.Data;
using CLUZWeb.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Sotsera.Blazor.Toaster.Core.Models;
using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.HttpOverrides;
using Blazored.Modal;

namespace CLUZWeb
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;
        public IConfiguration Configuration { get; }
        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            _env = env;
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<KestrelServerOptions>(
                Configuration.GetSection("Kestrel"));
            //services.AddHttpsRedirection(options =>
            //    {
            //        options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
            //        options.HttpsPort = 443;
            //    });
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddDefaultIdentity<IdentityUser>(options =>
                { 
                    options.SignIn.RequireConfirmedAccount = false;
                    options.SignIn.RequireConfirmedEmail = false;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddAuthentication().AddFacebook(facebookOptions =>
            {
                if (_env.IsDevelopment())
                {
                    facebookOptions.AppId = "";
                    facebookOptions.AppSecret = "";
                }
                else
                {
                    facebookOptions.AppId = "";
                    facebookOptions.AppSecret = "";
                }

                facebookOptions.ClaimActions.MapJsonKey("urn:facebook:name", "name", "string");
                facebookOptions.ClaimActions.MapJsonKey("urn:facebook:id", "id", "string");
                facebookOptions.ClaimActions.MapJsonKey("urn:facebook:email", "email", "string");
                facebookOptions.SaveTokens = true;

                facebookOptions.Events.OnCreatingTicket = ctx =>
                {
                    List<AuthenticationToken> tokens = ctx.Properties.GetTokens().ToList();

                    tokens.Add(new AuthenticationToken()
                    {
                        Name = "TicketCreated",
                        Value = DateTime.UtcNow.ToString()
                    });

                    ctx.Properties.StoreTokens(tokens);

                    return Task.CompletedTask;
                };
            });
            services.AddAuthentication().AddGoogle(options =>
            {
                if (_env.IsDevelopment())
                {
                    options.ClientId = "";
                    options.ClientSecret = "";
                }
                else
                {
                    options.ClientId = "";
                    options.ClientSecret = "";
                }

                options.ClaimActions.MapJsonKey("urn:google:name", "name", "string");
                options.ClaimActions.MapJsonKey("urn:google:picture", "picture", "url");
                options.ClaimActions.MapJsonKey("urn:google:email", "email", "string");
                options.SaveTokens = true;

                options.Events.OnCreatingTicket = ctx =>
                {
                    List<AuthenticationToken> tokens = ctx.Properties.GetTokens().ToList();

                    tokens.Add(new AuthenticationToken()
                    {
                        Name = "TicketCreated",
                        Value = DateTime.UtcNow.ToString()
                    });

                    ctx.Properties.StoreTokens(tokens);

                    return Task.CompletedTask;
                };

                //options.Scope.Add("https://www.googleapis.com/auth/user.birthday.read");
            });
            services.AddToaster(config =>
            {
                //example customizations
                config.PositionClass = Defaults.Classes.Position.TopRight;
                config.PreventDuplicates = false;
                config.NewestOnTop = false;
                config.HideTransitionDuration = 250;
                config.ShowTransitionDuration = 250;
                config.VisibleStateDuration = 4000;
                config.ShowCloseIcon = false;
            });

            services.AddBlazoredModal();

            services.AddTransient<IEmailSender, EmailSender>();
            services.Configure<AuthMessageSenderOptions>(Configuration);

            services.AddRazorPages()
                .AddRazorPagesOptions(options =>
                {
                    options.Conventions.AllowAnonymousToPage("/About");
                    options.Conventions.AllowAnonymousToPage("/Policy");
                });

            //change policies for password
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 3;
                options.Password.RequiredUniqueChars = 0;
            });

            services.AddServerSideBlazor();
            services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();

            services.AddSingleton<GamePoolService>();
            services.AddHostedService<Scavenger>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
