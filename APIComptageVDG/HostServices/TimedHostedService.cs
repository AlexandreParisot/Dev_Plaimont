using APIComptageVDG.Helpers;
using APIComptageVDG.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using RestSharp;
using System.Configuration;
using System.Reflection;
using System.Security.Policy;

namespace APIComptageVDG.HostServices
{


    public class Startup
    {
        public String hostUrl { get; set; }

        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            try            
            {
                Configuration = configuration;
                hostUrl = Configuration["url"];
            }
            catch (Exception ex)
            {
                Gestion.Erreur($"Erreur au demarrage : {ex.Message}");
            }
        }

        public void ConfigureServices(IServiceCollection services)
        {

            try
            {
                services.AddCors(options =>
                {
                    options.AddPolicy("CorsPolicy", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().AllowCredentials().Build());
                });

                //services.AddSingleton<LavilogService>();

                services.AddControllers();
              
                services.AddHttpContextAccessor();

                services.AddSwaggerGen(c => {
                    c.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Title = "APIComptageVDG",
                        Version = "v1",
                        Description = "Api Rest qui permet la gestion des campagnes pour le vers de grappe."
                                            ,
                        TermsOfService = new Uri("https://www.plaimont.fr/terms"),
                        Contact = new OpenApiContact
                        {
                            Name = "Contact Developper ",
                            Email = "c.heurtebize@plaimont.fr"
                        },
                        License = new OpenApiLicense
                        {
                            Name = "License PLAIMONT",
                            Url = new Uri("https://www.plaimont.fr/license")
                        }
                    });
                });               

                services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();

                

            }
            catch (Exception ex)
            {
                Gestion.Erreur($"ConfigurationServices authentification  err :{ex.Message}");
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            try
            {

                Gestion.Obligatoire($"Url API Web : {hostUrl}");

                //if (app.ApplicationServices.GetService(typeof(LavilogService)) is LavilogService serviceLavilog)
                //    serviceLavilog.SetConnexion(Configuration.GetConnectionString("SqlConnexion"));

                // Configure the HTTP request pipeline.
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(
                    Path.Combine(Path.GetDirectoryName(Environment.ProcessPath)!, "wwwroot", "css")),
                    RequestPath = "/css"
                });

                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "APIComptageVDG v1");
                    c.InjectStylesheet("/css/custom-swagger-ui.css");

                });
                app.UseHttpsRedirection();

                app.UseRouting();
                app.UseAuthorization();
                app.UseEndpoints(endpoints =>
                {
                   
                    endpoints.MapControllers();
                });

                
            }
            catch (Exception ex)
            {
                Gestion.Erreur($"Configure Error :{ex.Message}");
            }

        }
    }



    public class TimedHostedService:IHostedService, IDisposable
    {

        private Timer _timer;
        private CancellationTokenSource _stoppingCts;
        private IHost myHost;
        private static IConfiguration configWs;
        private static WebApplication app;

        public static IHostBuilder CreateHostBuilder(string[] args) =>
           Host.CreateDefaultBuilder(args)
               .ConfigureWebHostDefaults(webBuilder =>
               {
                   try
                   {
                       configWs = new ConfigurationBuilder()
                                .SetBasePath(Gestion.Current_Dir).AddJsonFile("appsettings.json").Build();

                       var app = webBuilder.UseContentRoot(Gestion.Current_Dir).UseKestrel().UseConfiguration(configWs).UseUrls(configWs["url"]);

                       //init connexion string dans le service lavilog
                       //var connectionString = app.Configure.GetConnectionString("SqlConnexion");
                       //var serviceSql = (LavilogService)app.ConfigureServices.GetService(typeof(LavilogService));
                       //if (serviceSql != null)
                       //    serviceSql.SetConnexion(connectionString);

                       app.UseIISIntegration().UseStartup<Startup>();
                   }
                   catch (Exception ex)
                   {
                       Gestion.Erreur($"General, Erreur dans le demarrage du service : {ex.Message}");
                   }
               });


        public TimedHostedService(ILogger<TimedHostedService> logger)
        {
            Gestion.SetLoggerProvider(logger);
            Gestion.Obligatoire($"Version : {Gestion.Version}");
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            Gestion.Obligatoire("Demarrage Service.");


            myHost = CreateHostBuilder(null).Build();
            myHost.StartAsync(_stoppingCts.Token);
            int time = 30;
            if (!int.TryParse(configWs?["Gestion:timer"], out time))
            {
                time = 30;
            }
            Gestion.Obligatoire($"Service Timer : {time} s");
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(time));


            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            try
            {
                StatusService();
            }
            catch (Exception ex)
            {
                Gestion.Obligatoire("Timer API Err : " + ex.Message);
            }
        }

        public async Task StopAsync(CancellationToken stoppingToken)
        {

            //_timer?.Change(Timeout.Infinite, 0);
            try
            {
                var result = app.StopAsync(); // stoppingCts.Token
                await result.ConfigureAwait(true);

            }
            catch (Exception ex)
            {
                Gestion.Obligatoire($"Arret Service. er : {ex.Message}");
            }

            Dispose();
            Gestion.Obligatoire("Arret Service.");
            // return await Task.CompletedTask;
        }


        private void StatusService()
        {

            var url = $"http://localhost:{configWs?["url"].Split(":").ToArray()[2].ToString().Trim()}/api/Status";
            var client = new RestClient(url);
            //client.Timeout = -1;
            var request = new RestRequest("", Method.Get);
            RestResponse response = client.Execute(request);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                try
                {
                    Gestion.Obligatoire("Status API : " + response.StatusCode + $" / Redemarrage service : Url control:  {url}");
                    myHost = CreateHostBuilder(null).Build();
                    myHost.StartAsync(_stoppingCts.Token);
                }
                catch (Exception ex)
                {
                    Gestion.Erreur("Status API  Err : " + ex.Message);
                }
            }
        }


        public void Dispose()
        {
            //_timer?.Dispose();
            _stoppingCts.Dispose();
        }

    }
}
