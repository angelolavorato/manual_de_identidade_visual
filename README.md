# Gerador de Materiais Didáticos — Desktop (v1.0)

Projeto-fonte de um aplicativo Windows Forms em C# para planejar e gerar prompts
para apostilas, avaliações, listas de exercícios, tutoriais, apresentações e imagens.
Foi estruturado para o Curso Técnico em Informática da Escola Estadual Olinto Almada.

## Requisitos

- Windows 10 ou Windows 11, preferencialmente 64 bits.
- **Visual Studio 2026** com a carga de trabalho **Desenvolvimento para desktop com .NET**.
- **SDK .NET 10**. Para instalar: https://dotnet.microsoft.com/download/dotnet/10.0

Não são necessárias bibliotecas NuGet de terceiros.

## Como executar no Visual Studio

1. Extraia todo o conteúdo do ZIP para uma pasta do Windows, por exemplo `C:\Projetos\GeradorMateriaisDesktop`.
2. Abra o arquivo **GeradorMateriaisDesktop.sln** no Visual Studio 2026.
3. Aguarde a restauração automática do projeto.
4. Pressione **F5** para executar com depuração ou **Ctrl+F5** para executar sem depuração.
5. Caso seja solicitado, instale o SDK .NET 10 e a carga de trabalho de desktop.

**Atenção:** o formulário é montado por código em `MainForm.cs`, e não por um arquivo `Form1.Designer.cs`.
Os controles podem ser personalizados editando as funções `BuildUi()`, `Card()` e `Field()`.

## Como gerar o executável independente

### Maneira 1 — arquivo BAT incluído

No Windows, após instalar o SDK .NET 10, dê duplo clique em `Publicar_EXE.bat`.
O executável será produzido em:

`publicado\GeradorMateriaisDesktop.exe`

### Maneira 2 — terminal do Visual Studio

Abra **Exibir > Terminal** no Visual Studio, na pasta do projeto, e execute:

```powershell
dotnet publish GeradorMateriaisDesktop.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=None -p:DebugSymbols=false -p:PublishTrimmed=false -o publicado
```

O arquivo `.exe` resultante poderá ser copiado para outros computadores Windows de arquitetura x64.
A publicação *self-contained* embute o runtime, portanto não exige instalação separada do .NET no computador de destino.
O primeiro início pode demorar um pouco mais por causa da extração interna de bibliotecas nativas.

## Como usar

1. Selecione ano, disciplina, trimestre e turma.
2. Escolha o tipo de material.
3. Informe o tema e os conteúdos específicos.
4. Ajuste profundidade, enfoque, ilustrações e opções condicionais.
5. Confira o texto gerado no painel à direita.
6. Use **Copiar prompt**, **Exportar TXT** ou **Abrir ChatGPT**.
7. Cole o texto no ChatGPT. O aplicativo não envia mensagens automaticamente e não utiliza uma chave de API.

### Recursos extras

- **Tema claro e escuro:** alterne pelo seletor no canto superior direito. A preferência é salva automaticamente em `preferencias.json` e restaurada ao abrir o programa, independentemente do perfil de material.
- **Editar disciplinas deste ano:** abre um editor simples, uma disciplina por linha.
- **Salvar perfil:** salva a configuração atual em `.json`.
- **Carregar perfil:** reutiliza uma configuração salva.
- **Último perfil:** restaurado automaticamente ao abrir o programa.
- **Manual visual:** carregue um arquivo `.md` ou `.txt` no campo indicado.
  Manuais com até 30 mil caracteres têm o conteúdo incorporado ao prompt.
  Acima desse limite, o app orienta o usuário a anexar o manual à conversa.
- **Exemplo de manual:** `Guias\MANUAL_VISUAL_EXEMPLO.md`.

Os arquivos de configuração local são salvos em:

`%LOCALAPPDATA%\GeradorMateriaisDidaticos\`

É possível excluí-los para restaurar as disciplinas e configurações iniciais.

## Organização dos arquivos

| Arquivo | Responsabilidade |
|---|---|
| `MainForm.cs` | Interface e interações do desktop |
| `CourseCatalog.cs` | Catálogo por ano escolar e personalização |
| `PromptGenerator.cs` | Regras de montagem dos prompts |
| `Models.cs` | Modelo e persistência JSON |
| `Program.cs` | Inicialização da aplicação |
| `GeradorMateriaisDesktop.csproj` | Configurações do projeto |
| `Publicar_EXE.bat` | Compilação e publicação para Windows x64 |
| `Guias/MANUAL_VISUAL_EXEMPLO.md` | Manual de identidade visual inicial |

## Usar com Visual Studio 2022

O SDK .NET 10 requer Visual Studio 2026. Se você utilizar Visual Studio 2022
(17.8 ou superior), altere a linha de `GeradorMateriaisDesktop.csproj`:

`<TargetFramework>net10.0-windows</TargetFramework>`

para:

`<TargetFramework>net8.0-windows</TargetFramework>`

Depois instale o SDK .NET 8 e compile. O .NET 8 está próximo do fim do suporte em
novembro de 2026; a opção preferencial para novos projetos é migrar para o
Visual Studio 2026 e .NET 10.

## Limitações desta primeira versão

- **Não gera DOCX, PDF, PPTX nem imagens diretamente.** Gera prompts que podem
  ser enviados ao ChatGPT para solicitar esses arquivos.
- Não há conexão com a API da OpenAI: nenhuma chave, custo ou autenticação externa.
- Arquivos `.docx` e `.pdf` não são interpretados como manuais visuais diretamente;
  forneça um manual `.txt`/`.md`, ou anexe o original na conversa do ChatGPT.
- Perfis são arquivos locais em JSON; não há sincronização na nuvem.

## Evolução possível

Integração opcional com a API da OpenAI, exportação direta de documentos,
repositório de conteúdos por disciplina, histórico e instalador MSI.

## Segurança

O aplicativo salva perfis apenas no computador e abre o navegador para o ChatGPT
quando você clicar. Não insira senhas, tokens ou chaves de API nos campos de
material. O código-fonte deve ser revisto e compilado antes de distribuição.
# manual_de_identidade_visual
