using System.ComponentModel;
using ModelContextProtocol.Server;

namespace Logistica.McpServer.Resources;

[McpServerResourceType]
public sealed class PoliticaEntregaResource
{
    [McpServerResource(UriTemplate = "logistica://politicas/entrega", Name = "politica_entrega", MimeType = "text/markdown")]
    [Description("Política oficial de prazos e ocorrências de entrega.")]
    public static string PoliticaDeEntrega() => """
        # Política de entrega

        - Prazos contados em dias úteis a partir da data da consulta.
        - Sudeste: 2 dias úteis. Sul: 3. Centro-Oeste, TO, RO e AC: 4.
        - Nordeste (BA, SE, PE, AL, PB, RN): 5. Demais estados do Norte e Nordeste: 7.
        - Pedido extraviado: abrir ocorrência e acionar o time de sinistros em até 24h.
        - Nunca prometer data diferente da calculada pelo sistema.
        """;
}
