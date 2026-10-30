using Logistica.McpServer;

// stdio (padrão): o cliente (Claude Desktop, VS Code, Claude Code) sobe o processo localmente.
// --http: o servidor roda como API, pronto para container, App Service ou Container Apps.
if (args.Contains("--http"))
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services
        .AddLogisticaMcp()
        .WithHttpTransport(); // Streamable HTTP, stateless por padrão no SDK 2.x

    var app = builder.Build();

    // Em produção: autenticação (Entra ID/OAuth) e .RequireAuthorization() aqui.
    app.MapMcp("/mcp");

    await app.RunAsync();
}
else
{
    var builder = Host.CreateApplicationBuilder(args);

    // No stdio, o stdout é do protocolo. Log vai para o stderr, sempre.
    builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

    builder.Services
        .AddLogisticaMcp()
        .WithStdioServerTransport();

    await builder.Build().RunAsync();
}
