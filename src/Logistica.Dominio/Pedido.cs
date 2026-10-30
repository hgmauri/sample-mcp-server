namespace Logistica.Dominio;

public enum StatusPedido
{
    AguardandoColeta,
    EmTransito,
    Entregue,
    Extraviado
}

public sealed record Pedido(
    string Numero,
    string Cliente,
    string CepDestino,
    string CidadeDestino,
    StatusPedido Status,
    DateOnly DataPostagem);

public sealed record Ocorrencia(
    string NumeroPedido,
    string Descricao,
    DateTimeOffset RegistradaEm);

public sealed record PrevisaoEntrega(
    string Cep,
    string Regiao,
    int DiasUteis,
    DateOnly DataPrevista);
