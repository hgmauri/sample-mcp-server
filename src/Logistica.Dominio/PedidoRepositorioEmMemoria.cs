using System.Collections.Concurrent;

namespace Logistica.Dominio;

/// <summary>
/// Repositório em memória só para o exemplo.
/// No mundo real, aqui entra o seu banco, o seu ERP ou a sua API interna.
/// </summary>
public sealed class PedidoRepositorioEmMemoria(TimeProvider relogio) : IPedidoRepositorio
{
    private readonly Dictionary<string, Pedido> _pedidos = new(StringComparer.OrdinalIgnoreCase)
    {
        ["PED-1001"] = new("PED-1001", "Café Serrano", "29050-000", "Vitória/ES", StatusPedido.EmTransito, new(2026, 10, 27)),
        ["PED-1002"] = new("PED-1002", "Agro Linhares", "01310-100", "São Paulo/SP", StatusPedido.Entregue, new(2026, 10, 20)),
        ["PED-1003"] = new("PED-1003", "Frutas do Vale", "30130-010", "Belo Horizonte/MG", StatusPedido.AguardandoColeta, new(2026, 10, 29)),
        ["PED-1004"] = new("PED-1004", "Casa Nordeste", "50030-230", "Recife/PE", StatusPedido.EmTransito, new(2026, 10, 22)),
        ["PED-1005"] = new("PED-1005", "Sul Embalagens", "90010-150", "Porto Alegre/RS", StatusPedido.Extraviado, new(2026, 10, 15))
    };

    private readonly ConcurrentDictionary<string, List<Ocorrencia>> _ocorrencias = new(StringComparer.OrdinalIgnoreCase);

    public Pedido? ObterPorNumero(string numero) =>
        _pedidos.GetValueOrDefault(numero.Trim());

    public IReadOnlyList<Pedido> ListarPorStatus(StatusPedido status) =>
        [.. _pedidos.Values.Where(p => p.Status == status).OrderBy(p => p.DataPostagem)];

    public Ocorrencia RegistrarOcorrencia(string numeroPedido, string descricao)
    {
        var ocorrencia = new Ocorrencia(numeroPedido.ToUpperInvariant(), descricao.Trim(), relogio.GetUtcNow());
        var lista = _ocorrencias.GetOrAdd(numeroPedido, _ => []);

        lock (lista)
        {
            lista.Add(ocorrencia);
        }

        return ocorrencia;
    }

    public IReadOnlyList<Ocorrencia> ListarOcorrencias(string numeroPedido)
    {
        if (!_ocorrencias.TryGetValue(numeroPedido, out var lista))
            return [];

        lock (lista)
        {
            return [.. lista];
        }
    }
}
