using api.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProcessosController : ControllerBase
    {
        private readonly IProcessoScraperService _service;

        public ProcessosController(IProcessoScraperService service)
        {
            _service = service;
        }


        [HttpPost]
        public async Task<IActionResult> BuscarProcessos([FromBody] List<string> numeros)
        {
            if (numeros == null || !numeros.Any())
                return BadRequest("Lista de Processos vazia");

            var resultado = await _service.BuscarProcessosAsync(numeros);

            return Ok(resultado);
        }

    }
}
