using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

// Cliente MCP sem LLM: útil para testar o servidor de forma determinística (e rodar no CI).
// Uso:
//   dotnet run --project src/Logistica.Cliente                          (stdio, sobe o servidor)
//   dotnet run --project src/Logistica.Cliente http://localhost:5000/mcp (HTTP, servidor já rodando)
var destino = args.FirstOrDefault()
    ?? Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Logistica.McpServer"));

IClientTransport transporte = destino.StartsWith("http", StringComparison.OrdinalIgnoreCase)
    ? new HttpClientTransport(new() { Endpoint = new Uri(destino) })
    : new StdioClientTransport(new()
    {
        Name = "logistica",
        Command = "dotnet",
        Arguments = ["run", "--project", destino, "--no-build"]
    });

await using var cliente = await McpClient.CreateAsync(transporte);

Console.WriteLine($"Conectado a {cliente.ServerInfo?.Name} {cliente.ServerInfo?.Version}\n");

// 1. Descoberta: o cliente não conhece as ferramentas de antemão, ele pergunta ao servidor
var ferramentas = await cliente.ListToolsAsync();

foreach (var ferramenta in ferramentas)
{
    var somenteLeitura = ferramenta.ProtocolTool.Annotations?.ReadOnlyHint == true ? "leitura" : "escrita";
    Console.WriteLine($"- {ferramenta.Name} [{somenteLeitura}]: {ferramenta.Description}");
}

// 2. Chamadas
await Chamar("consultar_pedido", new() { ["numeroPedido"] = "PED-1001" });
await Chamar("calcular_prazo_entrega", new() { ["cep"] = "29050-000" });
await Chamar("calcular_prazo_entrega", new() { ["cep"] = "123" });
await Chamar("listar_pedidos_por_status", new() { ["status"] = "EmTransito" });
await Chamar("registrar_ocorrencia", new() { ["numeroPedido"] = "PED-1004", ["descricao"] = "Cliente pediu reentrega no período da tarde." });
await Chamar("consultar_pedido", new() { ["numeroPedido"] = "PED-1004" });
await Chamar("consultar_pedido", new() { ["numeroPedido"] = "PED-9999" });

// 3. Recursos: contexto que o host pode anexar à conversa
var politica = await cliente.ReadResourceAsync("logistica://politicas/entrega");
Console.WriteLine("\n>> recurso logistica://politicas/entrega");
Console.WriteLine(politica.Contents.OfType<TextResourceContents>().First().Text);

async Task Chamar(string nome, Dictionary<string, object?> argumentos)
{
    var resultado = await cliente.CallToolAsync(nome, argumentos);
    var texto = string.Join("\n", resultado.Content.OfType<TextContentBlock>().Select(c => c.Text));

    Console.WriteLine($"\n>> {nome}({string.Join(", ", argumentos.Select(a => $"{a.Key}: {a.Value}"))})");
    Console.WriteLine(resultado.IsError == true ? $"[erro] {texto}" : texto);
}
