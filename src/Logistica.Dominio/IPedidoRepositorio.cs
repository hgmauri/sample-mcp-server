namespace Logistica.Dominio;

public interface IPedidoRepositorio
{
    Pedido? ObterPorNumero(string numero);

    IReadOnlyList<Pedido> ListarPorStatus(StatusPedido status);

    Ocorrencia RegistrarOcorrencia(string numeroPedido, string descricao);

    IReadOnlyList<Ocorrencia> ListarOcorrencias(string numeroPedido);
}
