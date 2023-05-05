using Microsoft.Extensions.Configuration;
using APIComptageVDG.Helpers;
using APIComptageVDG.HostServices;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using Microsoft.VisualBasic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime;
using System.ServiceProcess;

System.IO.Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);

Gestion.Version = "1.0.0";
Gestion.NameApp = "APIComptageVDG";

if (args.Length > 0 && args[0].ToLower().Contains("/install"))
{
    string nameService = args[0];
    Console.WriteLine("Installation de l'API en service windows ...");

    if (nameService.Contains("="))
    {
        nameService = nameService.Split("=").ToArray()[1].Trim();
        if (string.IsNullOrEmpty(nameService))
            nameService = "APIComptageVDG";
    }
    else
        nameService = "APIComptageVDG";

    Gestion.CreateService(nameService);

}
else if (args.Length > 0 && args[0].ToLower().Contains("/remove"))
{
    string nameService = args[0];
    Console.WriteLine("Suppression du service windows ...");

    if (nameService.Contains("="))
    {
        nameService = nameService.Split("=").ToArray()[1].Trim();
        if (string.IsNullOrEmpty(nameService))
            nameService = "APIComptageVDG";
    }
    else
        nameService = "APIComptageVDG";

    Gestion.RemoveService(nameService);

}else if (args.Length > 0 && args[0].ToLower().Contains("/help"))
{
    Console.WriteLine($"Options disponible pour l'api APIComptageVDG");
    Console.WriteLine($"/install");
    Console.WriteLine($"    Permet de faire l'installation en service Windows avec le nom par defaut APIComptageVDG.");
    Console.WriteLine($"/install=Nom du Service");
    Console.WriteLine($"    Permet de faire l'installation en service Windows avec le nom renseigné à la suite du /install.");
    Console.WriteLine($"/remove");
    Console.WriteLine($"    Permet de supprimer le service Windows avec le nom par defaut APIComptageVDG.");
    Console.WriteLine($"/remove=Nom du Service");
    Console.WriteLine($"    Permet de de supprimer le service Windows avec le nom renseigné à la suite du /remove.");
    Console.WriteLine($"/console");
    Console.WriteLine($"    Permet de demarrer en mode console.");
}
else if (args.Length > 0 && args[0].ToLower().Contains("/console"))
{
    //Demarrage en mode console.

    
    var builder = WebApplication.CreateBuilder(args);
    // Add services to the container.
    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c => {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "APIComptageVDG",
            Version = "v1",
            Description = "Api Rest qui permet la gestion des campagnes pour le vers de grappe."
                                ,
            TermsOfService = new Uri("https://www.a-g-i.fr/terms"),
            Contact = new OpenApiContact
            {
                Name = "Contact Developper ",
                Email = "c.heurtebize@plaimont.fr"
            },
            License = new OpenApiLicense
            {
                Name = "License PLAIMONT",
                Url = new Uri("https://www.a-g-i.fr/license")
            }
        });
    });

    builder.WebHost.ConfigureLogging((hostingContext, logging) =>
    {
        // The ILoggingBuilder minimum level determines the
        // the lowest possible level for logging. The log4net
        // level then sets the level that we actually log at.
        logging.AddLog4Net();
        logging.SetMinimumLevel(LogLevel.Debug);
    });

    var app = builder.Build();
    Gestion.SetLoggerProvider(app.Logger);
    Gestion.Obligatoire($"Version : {Gestion.Version}");

    var url = app.Configuration["url"];
    Gestion.Obligatoire($"Url API Web : {url}");

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

    app.UseAuthorization();

    app.MapControllers();

    app.Run(url);

    Gestion.Obligatoire($"Api : Start");
}
else
{
    //demarrage en Service
    try
    {
         CreateHostBuilder(args).ConfigureLogging((hostingContext, logging) =>
         {
             logging.AddLog4Net();
             logging.SetMinimumLevel(LogLevel.Debug);
         }).Build().Run();
    }
    catch (Exception ex)
    {
        Gestion.Erreur($"Erreur Start : {ex.Message}");
    }
}


 static IHostBuilder CreateHostBuilder(string[] args) =>
          Host.CreateDefaultBuilder(args).ConfigureServices((hostContext, services) =>
              {
                  services.AddHostedService<TimedHostedService>();
              }).UseWindowsService();