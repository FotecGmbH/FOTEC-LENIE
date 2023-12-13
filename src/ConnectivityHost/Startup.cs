// (C) 2023 FOTEC Forschungs- und Technologietransfer GmbH
// Das Forschungsunternehmen der Fachhochschule Wiener Neustadt
// 
// Kontakt biss@fotec.at / www.fotec.at
// 
// Erstversion vom 26.11.2023 18:45

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Biss.Apps;
using Biss.Apps.Base.Connectivity.Interface;
using Biss.Apps.Base.Connectivity.Model;
using Biss.Apps.Blazor;
using Biss.Apps.Blazor.Extensions;
using Biss.Apps.Service.Connectivity;
using Biss.Dc.Core;
using Biss.Dc.Transport.Server.SignalR;
using Biss.Log.Producer;
using ConnectivityHost.BaseApp;
using ConnectivityHost.Controllers;
using ConnectivityHost.DataConnector;
using ConnectivityHost.Helper;
using ConnectivityHost.Services;
using Database;
using Exchange;
using Exchange.Resources;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace ConnectivityHost
{
    /// <summary>
    ///     <para>Startup</para>
    ///     Klasse Startup. (C) 2021 FOTEC Forschungs- und Technologietransfer GmbH
    /// </summary>
    public class Startup
    {
        /// <summary>
        ///     Startup
        /// </summary>
        /// <param name="configuration">Config</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Logging.Init(c => c.AddDebug().SetMinimumLevel(LogLevel.Trace).AddConsole().SetMinimumLevel(LogLevel.Trace));
            Logging.Log.LogTrace("[ServerApp] Launch App ...");

            //Db.CreateAndFillUp(); //IM ECHTBETRIEB AUSKOMMENTIEREN
        }

        #region Properties

        /// <summary>
        ///     Config
        /// </summary>
        public IConfiguration Configuration { get; }

        #endregion

        /// <summary>
        ///     This method gets called by the runtime. Use this method to add services to the container.
        ///     For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        /// </summary>
        /// <param name="services">Services</param>
        public void ConfigureServices(IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddBissLog(l => l.AddDebug().AddConsole().SetMinimumLevel(LogLevel.Information));

            services.InitBissApps(AppSettings.Current());

            //BISS Apps
            if (!VmBase.DisableConnectivityBuildInApp)
            {
                Logging.Log.LogInfo("Blazor: Init started");

                AppSettings.Current().DefaultViewNamespace = "ConnectivityHost.Pages.";
                AppSettings.Current().DefaultViewAssembly = "ConnectivityHost";

                var init = new BissInitializer();
                init.Initialize(AppSettings.Current(), new Language());

                VmProjectBase.InitializeApp().ConfigureAwait(true);

                Logging.Log.LogInfo("Blazor: Init finished");
            }

            services.AddCors(options =>
                options.AddDefaultPolicy(builder =>
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod()));
            services.AddRazorPages();
            services.AddServerSideBlazor();

            //Datenbank
            services.AddDbContext<Db>();

            //Connectivity
            services.AddDcSignalRCore<IServerRemoteCalls>(typeof(ServerRemoteCalls));

            // configure DI for application services
            services.AddScoped<IAuthUserService, AuthUserService>();

            // Für Mails
            services.AddScoped<ICustomRazorEngine, CustomRazorEngine>();

            // Hintergrund-Service
            services.AddHostedService<BackgroundService<IServerRemoteCalls>>();

            // Authentication
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<ILocalStorageService, LocalStorageService>();

            services.AddMvc().AddControllersAsServices();

            services.AddControllers(o => { o.Conventions.Add(new ActionHidingConvention()); });

            services.AddRestAccess(new RestAccessService
                                   {
                                       PingResult = new ExPingResult
                                                    {
                                                        VersionNr = AppSettings.Current().AppVersion,
                                                        VersionUpdatedAt = new DateTime(2023, 08, 21, 12, 0, 0),
                                                    },
                                   });

            try
            {
                services.AddSwaggerGen(c =>
                {
                    try
                    {
                        c.SwaggerDoc("v1", new OpenApiInfo {Title = "API - LENIE", Version = "v1"});

                        var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                        c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
                        c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Exchange.xml"));

                        //Security
                        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                                                          {
                                                              Name = "Authorization",
                                                              Type = SecuritySchemeType.ApiKey,
                                                              Scheme = "Bearer",
                                                              BearerFormat = "Token",
                                                              In = ParameterLocation.Header,
                                                              Description = "Zugriffstoken hier einfügen - Wird vom WPF-Tool zur Verfügung gestellt"
                                                          });
                        c.AddSecurityRequirement(new OpenApiSecurityRequirement
                                                 {
                                                     {
                                                         new OpenApiSecurityScheme
                                                         {
                                                             Reference = new OpenApiReference
                                                                         {
                                                                             Type = ReferenceType.SecurityScheme,
                                                                             Id = "Bearer"
                                                                         }
                                                         },
                                                         new string[] { }
                                                     }
                                                 });
                    }
                    catch (Exception e)
                    {
                        Logging.Log.LogError($"{e}");
                    }
                });
            }
            catch (Exception e)
            {
                Logging.Log.LogError($"{e}");
            }
        }

        /// <summary>
        ///     This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApplication1 v1"));

            app.UseCors();
            app.UseStaticFiles();

            app.UseRouting();

            //Authorization & Authentication
            app.UseAuthentication();
            app.UseAuthorization();

            // custom jwt auth middleware
            app.UseMiddleware<JwtMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
                endpoints.MapControllers();
            });

            app.UseEndpoints(endpoints => { endpoints.MapHub<DcCoreHub<IServerRemoteCalls>>(DcHelper.DefaultHubRoute); });
        }
    }

    /// <summary>
    ///     Kontroller auf Swagger UI verbergen
    /// </summary>
    public class ActionHidingConvention : IActionModelConvention
    {
        #region Interface Implementations

        /// <summary>
        ///     Durchführen
        /// </summary>
        /// <param name="action">Action</param>
        public void Apply(ActionModel action)
        {
            if (action == null!)
            {
                throw new ArgumentNullException($"[{nameof(ActionHidingConvention)}]({nameof(Apply)}): {nameof(action)}");
            }

            //Liste der Controller, die versteckt werden sollen
            List<string> lstHideControllers = new();
            lstHideControllers.Add("Authentication");
            lstHideControllers.Add("Info");
            lstHideControllers.Add("WebLinks");
            lstHideControllers.Add("SaDemo");

            // Controller verstecken
            if (lstHideControllers.Contains(action.Controller.ControllerName))
            {
                action.ApiExplorer.IsVisible = false;
            }
        }

        #endregion
    }
}