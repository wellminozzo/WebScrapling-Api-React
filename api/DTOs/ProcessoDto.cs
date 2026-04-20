namespace api.DTOs;

public class ProcessoDto
{
    public string Numero { get; set; }
    public string Classe { get; set; }
    public string Assunto { get; set; }
    public string Foro { get; set; }
    public string Vara { get; set; }
    public string Juiz { get; set; }

    public List<MovimentacaoDto> Movimentacoes { get; set; } = new();
    public List<ParteDto> Partes { get; set; } = new();
}

public class MovimentacaoDto
{
    public string Data { get; set; }
    public string Descricao { get; set; }
}

public class ParteDto
{
    public string Tipo { get; set; }
    public string Nome { get; set; }
    public string Advogado { get; set; }
}
