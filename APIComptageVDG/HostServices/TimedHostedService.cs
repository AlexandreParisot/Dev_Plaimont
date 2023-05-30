using APIComptageVDG.Helpers;
using APIComptageVDG.Provider;
using APIComptageVDG.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using Renci.SshNet.Sftp;
using RestSharp;
using System.Configuration;
using System.Reflection;
using System.Security.Policy;
using static Org.BouncyCastle.Math.EC.ECCurve;

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

        #region service lavilog / Instagrappe

        private InstagrappeApiProvider _provider;
        private InstagrappeServcie _instaService;
        private LavilogService _service;
        private bool _initService = false;

        private bool initService()
        {
           var  _config = configWs;
            //_service = service;
            if (_service == null)
                _service = new LavilogService();

            if (!string.IsNullOrEmpty(_config.GetConnectionString("SqlConnexion")))
                _service.SetConnexion(_config.GetConnectionString("SqlConnexion")!);
            else
                return false;

            var url_token = _config["Instagrappe:URL_Token"];
            var url_api = _config["Instagrappe:URL_API"];
            var user = _config["Instagrappe:ID"];
            var pwd = _config["Instagrappe:MDP"];

            if (_instaService == null)
                _instaService = new InstagrappeServcie();

            if (!string.IsNullOrEmpty(url_token) && !string.IsNullOrEmpty(url_api) && !string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pwd))
            {
                _provider = new InstagrappeApiProvider(url_token, url_api, user, pwd);
                _instaService.SetApiProviderInstagrappe(_provider);
            }
            else
                return false;

            return true;
        }
        #endregion



        private Timer _timer;
        private CancellationTokenSource _stoppingCts;
        private IHost myHost;
        private static IConfiguration configWs;
        private static WebApplication app;
        private string tacheSynchro = string.Empty;

        public static IHostBuilder CreateHostBuilder(string[] args) =>
           Host.CreateDefaultBuilder(args)
               .ConfigureWebHostDefaults(webBuilder =>
               {
                   try
                   {
                       configWs = new ConfigurationBuilder()
                                .SetBasePath(Gestion.Current_Dir).AddJsonFile("appsettings.json").Build();
                       Gestion.Obligatoire($"Config  url : {configWs["url"]}");

                       var app = webBuilder.UseContentRoot(Gestion.Current_Dir).UseKestrel().UseConfiguration(configWs).UseUrls(configWs["url"]);
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
            myHost.StartAsync(_stoppingCts.Token).ContinueWith((x) =>
            {
                int time = 30;
                if (!int.TryParse(configWs?["Gestion:timer"], out time))
                {
                    time = 30;
                }
                Gestion.Obligatoire($"Service Timer : {time} s");
                if (!string.IsNullOrEmpty(configWs?["Gestion:tacheSynchro"]))
                {
                    tacheSynchro = configWs?["Gestion:tacheSynchro"];
                    Gestion.Obligatoire($"Une tache de Synchro avec Instagrappe est plannifier à {tacheSynchro}");
                    _initService = initService();
                    if (_initService)
                        Gestion.Obligatoire($"l'initialisation des services pour la tache planifier sont reussi.");
                    else
                        Gestion.Erreur($"l'initialisation des services pour la tache planifier sont en erreur.");
                }
                else
                    Gestion.Obligatoire($"il n'y a pas de tache planifier pour la synchro auto d'instagraape.");

                _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(time));


            });
           
           
            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            try
            {
                StatusService();
                SynchroInstagrappe();

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


        private async void SynchroInstagrappe()
        {

            //verifier si la derniére synchre est inferieure a l'heure de la et que c'est l'heure voulu
            if (tacheSynchro == DateTime.Now.ToString("HH:mm"))
            {
                if (_initService)
                {
                    var lastSynchro = await _service.AsyncGetLastSynchro();
                    DateTime lastSynchroDateLavilog;
                    DateTime lastSynchroDate;
                    if (!string.IsNullOrEmpty(lastSynchro) && DateTime.TryParse(lastSynchro, out lastSynchroDateLavilog) && DateTime.TryParse($"{DateTime.Today.ToString("dd/MM/yyyy")} {tacheSynchro}:00", out lastSynchroDate))
                    {
                        if (lastSynchroDateLavilog < lastSynchroDate)
                        {
                            synchroGrappe();
                        }
                    }
                    else if (!string.IsNullOrEmpty(lastSynchro))
                    {
                        synchroGrappe();
                    }
                    else
                        Gestion.Erreur($"Impossible de faire la synchro : lastSynchroDate {lastSynchro}");

                    PurgeFichierSynchro();
                }
            }
            //lancer la synchro

        }

        private void PurgeFichierSynchro()
        {
            Gestion.Obligatoire($"Debut de la purge des fichiers de synchros. ");
            int nb_delete_file = 0;
            if (System.IO.Directory.Exists(System.IO.Path.Combine(Gestion.Current_Dir, "API", "Echec"))){
                foreach(var file in System.IO.Directory.GetFiles(System.IO.Path.Combine(Gestion.Current_Dir, "API", "Echec")))
                {
                    FileInfo fi = new FileInfo(file);
                    if(fi.CreationTime >= DateTime.Now.AddDays(-30)){
                        try
                        {
                            fi.Delete();
                            nb_delete_file++;
                        }
                        catch(Exception ex) {
                            Gestion.Erreur($"Impossible de supprimer : {fi.Name} - {ex.Message}");
                        }
                    }
                }
            }

            if (System.IO.Directory.Exists(System.IO.Path.Combine(Gestion.Current_Dir, "API", "Reussi")))
            {
                foreach (var file in System.IO.Directory.GetFiles(System.IO.Path.Combine(Gestion.Current_Dir, "API", "Reussi")))
                {
                    FileInfo fi = new FileInfo(file);
                    if (fi.CreationTime >= DateTime.Now.AddDays(-30))
                    {
                        try
                        {
                            fi.Delete();
                            nb_delete_file++;
                        }
                        catch (Exception ex)
                        {
                            Gestion.Erreur($"Impossible de supprimer : {fi.Name} - {ex.Message}");
                        }
                    }
                }
            }
            Gestion.Obligatoire($"Nombre de fichier supprimé(s) : {nb_delete_file}");
            Gestion.Obligatoire($"Fin de la purge des fichiers de synchros. ");
        }

        private async void synchroGrappe()
        {

            Gestion.Obligatoire($"Lancement de la synchro Intagrappe.");
            // appel API  public async Task<ActionResult<bool>> GetSynchroDeclaration(int year)
            var url = $"http://localhost:{configWs?["url"].Split(":").ToArray()[2].ToString().Trim()}/CampagneVDG/Instagrappe/SynchroDeclaration?year={DateTime.Today.ToString("yyyy")}";
            var client = new RestClient(url);

            var request = new RestRequest("", Method.Get);
            RestResponse response = client.Execute(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {

                // appel API public async Task<ActionResult<bool>> GetSynchroCompteur(int year)

                url = $"http://localhost:{configWs?["url"].Split(":").ToArray()[2].ToString().Trim()}/CampagneVDG/Instagrappe/SynchroCompteur?year={DateTime.Today.ToString("yyyy")}";

                client = new RestClient(url);

                request = new RestRequest("", Method.Get);
                response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    if (await _service.AsyncSetLastSynchro())
                    {
                        Gestion.Obligatoire($"Synchro instagrappe effectué : {DateTime.Now}");

                    }
                }
                else
                    Gestion.Erreur($"Erreur lors de la synchro des compteurs");
            }
            else
                Gestion.Erreur($"Erreur lors de la synchro des parcelles");

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
