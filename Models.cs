using System.Text.Json;

namespace GeradorMateriaisDesktop;

public sealed class MaterialConfig
{
    public string Ano { get; set; } = "3º ano";
    public string Disciplina { get; set; } = "Redes de Computadores";
    public string Trimestre { get; set; } = "3º trimestre";
    public string Turma { get; set; } = "";
    public string Tipo { get; set; } = "Apostila";
    public string Tema { get; set; } = "";
    public string Detalhes { get; set; } = "";
    public string Nivel { get; set; } = "Intermediário";
    public string Enfoque { get; set; } = "Teórico e prático";
    public string Visual { get; set; } = "Equilibrada";
    public string Formato { get; set; } = "DOCX";
    public bool AtividadesPraticas { get; set; } = true;
    public bool Referencias { get; set; } = true;
    public string QuestaoModelo { get; set; } = "8 objetivas (A–E) e 2 discursivas";
    public string Pontos { get; set; } = "10";
    public bool ComConsulta { get; set; } = true;
    public bool Gabarito { get; set; } = true;
    public bool Individual { get; set; } = true;
    public bool Evidencias { get; set; } = false;
    public string QuantidadeSlides { get; set; } = "12 a 15";
    public string ProporcaoImagem { get; set; } = "4:3 (horizontal)";
    public string TipoImagem { get; set; } = "Diagrama técnico";
    public bool PrintsTutorial { get; set; } = true;
    public string ManualPath { get; set; } = "";
}

internal static class Storage
{
    public static string DirectoryPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "GeradorMateriaisDidaticos");

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public static string LastProfilePath => Path.Combine(DirectoryPath, "ultimo-perfil.json");
    public static string CatalogPath => Path.Combine(DirectoryPath, "disciplinas.json");

    public static void WriteJson<T>(string path, T value)
    {
        string? directory = Path.GetDirectoryName(path);
        if (directory is not null) Directory.CreateDirectory(directory);
        File.WriteAllText(path, JsonSerializer.Serialize(value, JsonOptions));
    }

    public static T? ReadJson<T>(string path)
    {
        if (!File.Exists(path)) return default;
        return JsonSerializer.Deserialize<T>(File.ReadAllText(path), JsonOptions);
    }
}
