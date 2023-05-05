using APIComptageVDG.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace APIComptageVDG.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatusController : ControllerBase
    {

        private IConfiguration _config;
        public StatusController(IConfiguration config)
        {
            _config = config;
           
        }


        /// <summary>
        /// retour le status de l'api
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<String> Get()
        {
            Gestion.Info("Keep Alive !!!");
            return Ok();
        }

        /// <summary>
        /// Logs the specified trace.
        /// </summary>
        /// <param name="trace">The trace.</param>
        /// <returns></returns>
        [HttpGet("Log")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<String> Log(String trace)
        {
            Gestion.Obligatoire($"[Log externe] : {trace} ");
            return Ok();            
        }
    }
}
