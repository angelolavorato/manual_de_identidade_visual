using System.Text;

namespace GeradorMateriaisDesktop;

public static class PromptGenerator
{
    public static string Build(MaterialConfig c, out string? warning)
    {
        var s = new StringBuilder();
        warning = null;
        s.AppendLine("SOLICITAÇÃO DE MATERIAL DIDÁTICO");
        s.AppendLine("================================");
        s.AppendLine();
        s.AppendLine("Escola: Escola Estadual Olinto Almada");
        s.AppendLine("Curso: Técnico em Informática — Ensino Médio");
        s.AppendLine($"Público-alvo: {c.Ano}{(string.IsNullOrWhiteSpace(c.Turma) ? "" : ", turma " + c.Turma.Trim())}");
        s.AppendLine($"Disciplina: {c.Disciplina}");
        s.AppendLine($"Período: {c.Trimestre}");
        s.AppendLine($"Material: {c.Tipo}");
        s.AppendLine($"Tema: {(string.IsNullOrWhiteSpace(c.Tema) ? "[PREENCHER O TEMA]" : c.Tema.Trim())}");
        if (!string.IsNullOrWhiteSpace(c.Detalhes))
        {
            s.AppendLine();
            s.AppendLine("CONTEÚDOS E ORIENTAÇÕES ESPECÍFICAS:");
            s.AppendLine(c.Detalhes.Trim());
        }
        s.AppendLine();
        s.AppendLine("DIRETRIZES PEDAGÓGICAS:");
        s.AppendLine($"- Profundidade: {c.Nivel}.");
        s.AppendLine($"- Enfoque: {c.Enfoque}.");
        s.AppendLine("- Adapte a linguagem e os pré-requisitos ao ano escolar indicado.");
        s.AppendLine("- Evite pressupor que a turma já estudou conceitos avançados não informados.");
        s.AppendLine("- Utilize exemplos verificáveis e explicações tecnicamente rigorosas.");
        s.AppendLine();
        s.AppendLine("IDENTIDADE VISUAL:");
        s.AppendLine("- Azul institucional #173B63, azul técnico #2879B8, verde-água #14A3A6,");
        s.AppendLine("  cinza #E8EEF3 e laranja #F0A53A apenas para destaque.");
        s.AppendLine("- Fundo claro, boa hierarquia visual, fontes Aptos ou Arial, acessibilidade e contraste.");
        s.AppendLine($"- Densidade visual: {c.Visual}.");
        s.AppendLine("- Figuras com legendas. Não invente telas de software, diagramas elétricos ou comandos.");

        string kind = c.Tipo;
        s.AppendLine();
        s.AppendLine("REQUISITOS DO MATERIAL:");
        switch (kind)
        {
            case "Apostila":
                s.AppendLine("- Organize capa, objetivos, introdução, conteúdo sequenciado, exemplos, atividades e síntese.");
                s.AppendLine("- Prefira seções numeradas e figuras centralizadas com legenda e fonte, se aplicável.");
                if (c.AtividadesPraticas) s.AppendLine("- Acrescente exercícios e aplicações práticas, sem divulgar soluções junto aos enunciados.");
                break;
            case "Avaliação":
                s.AppendLine($"- Elabore {c.QuestaoModelo}, distribuindo {c.Pontos} pontos no total.");
                s.AppendLine("- Indique o valor de cada questão e proponha situações contextualizadas.");
                s.AppendLine("- Não inclua respostas na versão destinada aos alunos.");
                s.AppendLine(c.ComConsulta ? "- Avaliação COM consulta ao material didático." : "- Avaliação SEM consulta.");
                if (c.Gabarito) s.AppendLine("- Entregue o gabarito comentado e os critérios de correção em arquivo SEPARADO para o professor.");
                break;
            case "Lista de exercícios":
                s.AppendLine("- Crie questões progressivas, com enunciados claros e espaço adequado de resposta.");
                s.AppendLine(c.Individual ? "- Entrega individual." : "- Modalidade de entrega a ser definida.");
                if (c.Evidencias) s.AppendLine("- Solicite capturas de tela que comprovem as etapas executadas.");
                s.AppendLine("- Não apresente soluções na versão para o aluno.");
                break;
            case "Tutorial":
                s.AppendLine("- Inclua requisitos iniciais, passos numerados, verificações e resolução de falhas comuns.");
                if (c.PrintsTutorial) s.AppendLine("- Utilize capturas reais das interfaces ou indique claramente os prints que precisam ser coletados.");
                if (c.AtividadesPraticas) s.AppendLine("- Inclua tarefa de aplicação ao final e evidências da execução quando pertinentes.");
                break;
            case "Slides":
                s.AppendLine($"- Crie aproximadamente {c.QuantidadeSlides} slides em proporção 16:9.");
                s.AppendLine("- Um conceito principal por slide; textos enxutos, fontes legíveis e diagramas pertinentes.");
                if (c.AtividadesPraticas) s.AppendLine("- Inclua slides de demonstração e uma atividade de aplicação.");
                break;
            case "Imagem técnica":
                s.AppendLine($"- Tipo de imagem: {c.TipoImagem}.");
                s.AppendLine($"- Proporção solicitada: {c.ProporcaoImagem}.");
                s.AppendLine("- Mantenha a precisão técnica; minimize textos internos e evite rótulos ilegíveis.");
                s.AppendLine("- Forneça uma imagem independente e adequada à inserção em apostilas.");
                break;
        }
        if (kind != "Imagem técnica" && c.Referencias)
            s.AppendLine("- Inclua referências bibliográficas reais e verificáveis, formatadas conforme ABNT quando aplicável.");
        s.AppendLine();
        s.AppendLine("ENTREGA E REVISÃO:");
        s.AppendLine($"- Formato principal: {c.Formato}.");
        if (c.Formato == "DOCX") s.AppendLine("- A4 retrato, editável, com estilos de títulos e texto, não apenas imagens de páginas.");
        else if (c.Formato == "PPTX") s.AppendLine("- Apresentação editável, proporção widescreen 16:9.");
        else if (c.Formato == "PDF") s.AppendLine("- PDF paginado, revisado, adequado para impressão e distribuição.");
        else if (c.Formato == "PNG") s.AppendLine("- PNG em alta resolução, legível no tamanho de utilização.");
        s.AppendLine("- Revise ortografia, tabelas, cortes, paginação, consistência de cores e fidelidade técnica.");

        if (!string.IsNullOrWhiteSpace(c.ManualPath))
        {
            s.AppendLine();
            s.AppendLine("MANUAL VISUAL PERSONALIZADO:");
            try
            {
                string manual = File.ReadAllText(c.ManualPath).Trim();
                if (manual.Length <= 30000)
                {
                    s.AppendLine("Aplique também as regras a seguir, prevalecendo as instruções explícitas do usuário:");
                    s.AppendLine("--- INÍCIO DO MANUAL ---");
                    s.AppendLine(manual);
                    s.AppendLine("--- FIM DO MANUAL ---");
                }
                else
                {
                    warning = "O manual tem mais de 30 mil caracteres. Anexe-o à conversa para usar seu conteúdo integral.";
                    s.AppendLine($"Anexe à conversa o arquivo '{Path.GetFileName(c.ManualPath)}' e siga suas instruções.");
                }
            }
            catch (Exception ex)
            {
                warning = "Não foi possível ler o manual selecionado: " + ex.Message;
                s.AppendLine("Anexe o manual de identidade visual à conversa e utilize-o como referência.");
            }
        }
        return s.ToString().TrimEnd();
    }
}
