using System.ComponentModel;
using Logistica.Dominio;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace Logistica.McpServer.Tools;

/// <summary>
/// Camada fina: traduz o protocolo MCP para o domínio.
/// Nenhuma regra de negócio mora aqui.
/// </summary>
[McpServerToolType]
public sealed class PedidoTools(IPedidoRepositorio repositorio, CalculadoraPrazo calculadora)
{
    [McpServerTool(ReadOnly = true, OpenWorld = false)]
    [Description("Consulta status, cliente, destino e ocorrências de um pedido pelo número.")]
    public PedidoResposta ConsultarPedido(
        [Description("Número do pedido no formato PED-0000. Exemplo: PED-1001.")] string numeroPedido)
    {
        var pedido = repositorio.ObterPorNumero(numeroPedido)
            ?? throw new McpException($"Pedido {numeroPedido} não encontrado.");

        return PedidoResposta.De(pedido, repositorio.ListarOcorrencias(pedido.Numero));
    }

    [McpServerTool(ReadOnly = true, OpenWorld = false)]
    [Description("Calcula o prazo de entrega em dias úteis e a data prevista para um CEP de destino.")]
    public PrevisaoEntrega CalcularPrazoEntrega(
        [Description("CEP de destino com 8 dígitos. Aceita com ou sem hífen.")] string cep)
    {
        try
        {
            return calculadora.Calcular(cep);
        }
        catch (CepInvalidoException ex)
        {
            // McpException: a mensagem chega ao modelo. Qualquer outra exceção vira erro genérico.
            throw new McpException(ex.Message);
        }
    }

    [McpServerTool(ReadOnly = true, OpenWorld = false)]
    [Description("Lista os pedidos em um status. Valores aceitos: AguardandoColeta, EmTransito, Entregue, Extraviado.")]
    public IReadOnlyList<PedidoResposta> ListarPedidosPorStatus(
        [Description("Status do pedido.")] string status)
    {
        if (!Enum.TryParse<StatusPedido>(status, ignoreCase: true, out var statusPedido))
            throw new McpException($"Status '{status}' inválido. Use: {string.Join(", ", Enum.GetNames<StatusPedido>())}.");

        return [.. repositorio.ListarPorStatus(statusPedido).Select(p => PedidoResposta.De(p, []))];
    }

    [McpServerTool(ReadOnly = false, Destructive = false, Idempotent = false, OpenWorld = false)]
    [Description("Registra uma ocorrência de atendimento em um pedido existente. Use somente quando o usuário pedir.")]
    public Ocorrencia RegistrarOcorrencia(
        [Description("Número do pedido no formato PED-0000.")] string numeroPedido,
        [Description("Descrição objetiva da ocorrência, até 280 caracteres.")] string descricao)
    {
        if (repositorio.ObterPorNumero(numeroPedido) is null)
            throw new McpException($"Pedido {numeroPedido} não encontrado.");

        if (string.IsNullOrWhiteSpace(descricao) || descricao.Length > 280)
            throw new McpException("A descrição deve ter entre 1 e 280 caracteres.");

        return repositorio.RegistrarOcorrencia(numeroPedido, descricao);
    }
}

public sealed record PedidoResposta(
    string Numero,
    string Cliente,
    string Destino,
    string Status,
    DateOnly DataPostagem,
    IReadOnlyList<string> Ocorrencias)
{
    public static PedidoResposta De(Pedido pedido, IReadOnlyList<Ocorrencia> ocorrencias) => new(
        pedido.Numero,
        pedido.Cliente,
        $"{pedido.CidadeDestino} ({pedido.CepDestino})",
        pedido.Status.ToString(),
        pedido.DataPostagem,
        [.. ocorrencias.Select(o => $"{o.RegistradaEm:dd/MM/yyyy HH:mm} UTC: {o.Descricao}")]);
}
