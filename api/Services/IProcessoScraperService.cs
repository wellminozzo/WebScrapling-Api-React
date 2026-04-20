using api.DTOs;

namespace api.Services
{
    public interface IProcessoScraperService
    {
        Task<List<ProcessoDto>> BuscarProcessosAsync(List<string> numeros);
    }
}