using Logistica.Dominio;
using Logistica.McpServer.Resources;
using Logistica.McpServer.Tools;

namespace Logistica.McpServer;

public static class LogisticaMcpExtensions
{
    /// <summary>
    /// Registra o domínio e o servidor MCP. O transporte (stdio ou HTTP) é escolhido por quem chama.
    /// </summary>
    public static IMcpServerBuilder AddLogisticaMcp(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IPedidoRepositorio, PedidoRepositorioEmMemoria>();
        services.AddSingleton<CalculadoraPrazo>();

        return services
            .AddMcpServer(options =>
            {
                options.ServerInfo = new() { Name = "logistica-mcp", Version = "1.0.0" };
                options.ServerInstructions = "Servidor de logística da Dotcode. Use as ferramentas para consultar pedidos e prazos. Nunca invente dados.";
            })
            .WithTools<PedidoTools>()
            .WithResources<PoliticaEntregaResource>();
    }
}
