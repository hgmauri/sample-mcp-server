namespace Logistica.Dominio;

/// <summary>
/// A regra de prazo mora aqui, e só aqui.
/// Quem chama (API, tela, agente de IA ou servidor MCP) não decide nada sobre prazo.
/// </summary>
public sealed class CalculadoraPrazo(TimeProvider relogio)
{
    public PrevisaoEntrega Calcular(string cep)
    {
        var cepNormalizado = Normalizar(cep);
        var (regiao, diasUteis) = PrazoPorRegiao(cepNormalizado[0]);
        var hoje = DateOnly.FromDateTime(relogio.GetLocalNow().DateTime);

        return new PrevisaoEntrega(
            Cep: $"{cepNormalizado[..5]}-{cepNormalizado[5..]}",
            Regiao: regiao,
            DiasUteis: diasUteis,
            DataPrevista: SomarDiasUteis(hoje, diasUteis));
    }

    public static string Normalizar(string cep)
    {
        var somenteDigitos = new string(cep.Where(char.IsAsciiDigit).ToArray());

        if (somenteDigitos.Length != 8)
            throw new CepInvalidoException(cep);

        return somenteDigitos;
    }

    // Primeiro dígito do CEP indica a região postal
    private static (string Regiao, int DiasUteis) PrazoPorRegiao(char primeiroDigito) => primeiroDigito switch
    {
        >= '0' and <= '3' => ("Sudeste", 2),
        '4' or '5' => ("Nordeste (BA, SE, PE, AL, PB, RN)", 5),
        '6' => ("Norte e Nordeste (CE, PI, MA, PA, AM, AP, RR)", 7),
        '7' => ("Centro-Oeste, TO, RO e AC", 4),
        _ => ("Sul", 3)
    };

    private static DateOnly SomarDiasUteis(DateOnly inicio, int diasUteis)
    {
        var data = inicio;

        while (diasUteis > 0)
        {
            data = data.AddDays(1);

            if (data.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday)
                diasUteis--;
        }

        return data;
    }
}

public sealed class CepInvalidoException(string cep)
    : ArgumentException($"CEP '{cep}' inválido. Informe 8 dígitos.");
