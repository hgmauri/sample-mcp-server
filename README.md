# sample-mcp-server

Exemplo de **servidor MCP (Model Context Protocol) em .NET 10** usando o SDK oficial em C# (`ModelContextProtocol` 2.3.0).

O cenário é o mesmo do [sample-agent-framework](https://github.com/hgmauri/sample-agent-framework): uma empresa de logística que precisa consultar pedidos e prazos. A diferença é que agora a regra de negócio sai de dentro do agente e vira um servidor MCP. Qualquer cliente compatível (Claude Desktop, Claude Code, VS Code com Copilot, um agente com Microsoft Agent Framework) passa a usar as mesmas ferramentas, sem reescrever nada.

Artigo completo: [Criando um servidor MCP com o .NET](https://henriquemauri.net/criando-um-servidor-mcp-com-o-net/)

## Estrutura

```
src/
  Logistica.Dominio      Regras de negócio puras (pedidos, cálculo de prazo). Sem dependência de IA.
  Logistica.McpServer    Servidor MCP: tools, resource e os transportes stdio e HTTP.
  Logistica.Cliente      Cliente MCP sem LLM, para testar o servidor de forma determinística.
.vscode/mcp.json         Configuração para o VS Code
.mcp.json                Configuração para o Claude Code
```

## O que o servidor expõe

| Tipo | Nome | Leitura/Escrita |
|------|------|-----------------|
| Tool | `consultar_pedido` | leitura |
| Tool | `calcular_prazo_entrega` | leitura |
| Tool | `listar_pedidos_por_status` | leitura |
| Tool | `registrar_ocorrencia` | escrita (não destrutiva) |
| Resource | `logistica://politicas/entrega` | leitura |

As anotações `ReadOnly`, `Destructive` e `OpenWorld` vão para o cliente. Hosts como o Claude Desktop e o VS Code usam essas dicas para decidir quando pedir confirmação ao usuário.

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Como rodar

```bash
dotnet build

# Testa o servidor via stdio, sem LLM
dotnet run --project src/Logistica.Cliente

# Sobe o servidor via HTTP (Streamable HTTP em /mcp)
dotnet run --project src/Logistica.McpServer -- --http --urls http://localhost:5000

# Em outro terminal, testa via HTTP
dotnet run --project src/Logistica.Cliente -- http://localhost:5000/mcp
```

## Usando no Claude Desktop

Edite o `claude_desktop_config.json` (Settings > Developer > Edit Config):

```json
{
  "mcpServers": {
    "logistica": {
      "command": "dotnet",
      "args": ["run", "--project", "C:/caminho/sample-mcp-server/src/Logistica.McpServer"]
    }
  }
}
```

Reinicie o Claude Desktop e pergunte: *"Qual o status do pedido PED-1004 e quando chega uma entrega para o CEP 29050-000?"*

## Usando no Claude Code e no VS Code

O repositório já traz `.mcp.json` (Claude Code) e `.vscode/mcp.json` (VS Code). Basta abrir a pasta. No Claude Code também funciona por linha de comando:

```bash
claude mcp add logistica -- dotnet run --project src/Logistica.McpServer
```

## Usando com Microsoft Agent Framework

O `McpClientTool` herda de `AIFunction`, então as ferramentas do servidor entram direto no agente:

```csharp
await using var mcp = await McpClient.CreateAsync(new StdioClientTransport(new()
{
    Name = "logistica",
    Command = "dotnet",
    Arguments = ["run", "--project", "src/Logistica.McpServer"]
}));

var ferramentas = await mcp.ListToolsAsync();

AIAgent agent = new AIProjectClient(new Uri(endpoint), new DefaultAzureCredential())
    .AsAIAgent(
        model: deploymentName,
        name: "AgenteLogistica",
        instructions: "Você atende clientes de uma empresa de logística. Nunca invente dados.",
        tools: [.. ferramentas]);
```

## Pontos de atenção para produção

- **Log no stdio vai para o stderr.** O stdout é do protocolo. Um `Console.WriteLine` perdido quebra a comunicação.
- **Só `McpException` chega ao modelo.** Qualquer outra exceção vira mensagem genérica, o que evita vazar detalhe interno.
- **HTTP precisa de autenticação.** O SDK tem suporte a OAuth (`ModelContextProtocol.AspNetCore.Authentication`). Em Azure, Entra ID.
- **A tool é uma porta de entrada.** Valide argumentos como validaria em qualquer API pública.

## Licença

MIT

---

Feito por [Henrique Mauri](https://henriquemauri.net) | [Dotcode](https://dotcode.com.br)
