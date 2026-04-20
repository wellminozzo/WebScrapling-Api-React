using api.DTOs;
using HtmlAgilityPack;
using System.Net;
using System.Text.RegularExpressions;


namespace api.Services;

    public class ProcessoScraperService : IProcessoScraperService
    {
        private readonly HttpClient _client;

        public ProcessoScraperService(HttpClient client)
        {
            _client = client;


            _client.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
        }
    

        public async Task<List<ProcessoDto>> BuscarProcessosAsync(List<string> numeros)
        {
            var resultados = new List<ProcessoDto>();

            foreach (var numero in numeros)
            {
                var processo = await BuscarProcessoAsync(numero);
                if (processo != null)
                    resultados.Add(processo);
            }

            return resultados;
        }

        private async Task<ProcessoDto?> BuscarProcessoAsync(string numero)
        {
            try
            {
                var numeroBase = numero.Substring(0, 15);
                var foro = numero.Substring(numero.Length - 4);

                var url = $"https://esaj.tjsp.jus.br/cpopg/search.do?" +
                          $"cbPesquisa=NUMPROC" +
                          $"&numeroDigitoAnoUnificado={numeroBase}" +
                          $"&foroNumeroUnificado={foro}" +
                          $"&dadosConsulta.valorConsultaNuUnificado={numero}" +
                          $"&dadosConsulta.tipoNuProcesso=UNIFICADO";

                var response = await _client.GetAsync(url);
                var html = await response.Content.ReadAsStringAsync();

                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var processo = new ProcessoDto
                {
                    Numero = numero,
                    Classe = GetText(doc, "//*[@id='classeProcesso']"),
                    Assunto = GetText(doc, "//*[@id='assuntoProcesso']"),
                    Foro = GetText(doc, "//*[@id='foroProcesso']"),
                    Vara = GetText(doc, "//*[@id='varaProcesso']"),
                    Juiz = GetText(doc, "//*[@id='juizProcesso']")
                };

                processo.Movimentacoes = GetMovimentacoes(doc);
                processo.Partes = GetPartes(doc);

                return processo;
            }
            catch
            {
                return null;
            }
        }

        private List<MovimentacaoDto> GetMovimentacoes(HtmlDocument doc)
        {
            var lista = new List<MovimentacaoDto>();

            var nodes = doc.DocumentNode
                .SelectNodes("//*[@id='tabelaUltimasMovimentacoes']//tr");

            if (nodes == null) return lista;

            foreach (var node in nodes)
            {
                var data = node.SelectSingleNode(".//td[contains(@class,'dataMovimentacao')]")?.InnerText;
                var desc = node.SelectSingleNode(".//td[contains(@class,'descricaoMovimentacao')]")?.InnerText;

                if (string.IsNullOrWhiteSpace(data) || string.IsNullOrWhiteSpace(desc))
                    continue;

                lista.Add(new MovimentacaoDto
                {
                    Data = Clean(data),
                    Descricao = Clean(desc)
                });
            }

            return lista;
        }

        private List<ParteDto> GetPartes(HtmlDocument doc)
        {
            var lista = new List<ParteDto>();

            var nodes = doc.DocumentNode
                .SelectNodes("//*[@id='tablePartesPrincipais']//tr");

            if (nodes == null) return lista;

            foreach (var node in nodes)
            {
                var tipo = node
                    .SelectSingleNode(".//span[contains(@class,'tipoDeParticipacao')]")
                    ?.InnerText;

                var nomeAdv = node
                    .SelectSingleNode(".//td[contains(@class,'nomeParteEAdvogado')]")
                    ?.InnerText;

                if (string.IsNullOrWhiteSpace(tipo) || string.IsNullOrWhiteSpace(nomeAdv))
                    continue;

                nomeAdv = Clean(nomeAdv);
                tipo = Clean(tipo);

                var split = nomeAdv.Split("Advogado:");

                lista.Add(new ParteDto
                {
                    Tipo = tipo,
                    Nome = split[0].Trim(),
                    Advogado = split.Length > 1 ? split[1].Trim() : null
                });
            }

            return lista;
        }

        private string Clean(string text)
        {
            text = HtmlEntity.DeEntitize(text);
            text = Regex.Replace(text, @"\s+", " ").Trim();
            return text;
        }

        private string GetText(HtmlDocument doc, string xpath)
        {
            var node = doc.DocumentNode.SelectSingleNode(xpath);
            return node == null ? "" : Clean(node.InnerText);
        }
}




