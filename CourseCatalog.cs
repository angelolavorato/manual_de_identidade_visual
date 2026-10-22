namespace GeradorMateriaisDesktop;

public sealed class CourseCatalog
{
    private readonly Dictionary<string, List<string>> _data;

    // Disciplinas por ano conforme Ementa EMTI.pdf, sem a divisão em bimestres.
    private static Dictionary<string, List<string>> Defaults() => new()
    {
        ["1º ano"] = [
            "Informática Básica", "Lógica de Programação"
        ],
        ["2º ano"] = [
            "Arquitetura e Manutenção de Computadores", "HTML / CSS",
            "Gestão do Tempo", "Sistemas Operacionais",
            "Programação Orientada a Objetos / Java",
            "Programação Web (JavaScript / PHP / MySQL)", "Noções de Robótica"
        ],
        ["3º ano"] = [
            "Rede de Computadores", "Design", "Gerenciador de Conteúdo / Banco de Dados",
            "Programação Web II (PHP / MySQL)", "Laboratório de Hardware",
            "Laboratório de Software", "Laboratório Web"
        ]
    };

    public CourseCatalog()
    {
        try
        {
            _data = Storage.ReadJson<Dictionary<string, List<string>>>(Storage.CatalogPath)
                ?? Defaults();
            foreach (string year in Defaults().Keys)
                if (!_data.ContainsKey(year)) _data[year] = Defaults()[year];
        }
        catch
        {
            _data = Defaults();
        }
    }

    public IReadOnlyList<string> Subjects(string year)
    {
        return _data.TryGetValue(year, out var values)
            ? values : Array.Empty<string>();
    }

    public void SetSubjects(string year, string lines)
    {
        var subjects = lines.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => s.Length > 0)
            .Distinct(StringComparer.CurrentCultureIgnoreCase)
            .ToList();
        if (subjects.Count == 0) throw new ArgumentException("Informe ao menos uma disciplina.");
        _data[year] = subjects;
        Storage.WriteJson(Storage.CatalogPath, _data);
    }
}
