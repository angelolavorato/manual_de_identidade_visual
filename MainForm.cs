using System.Diagnostics;
using System.Drawing;
using System.Text;

namespace GeradorMateriaisDesktop;

public sealed class MainForm : Form
{
    private static readonly Color Navy = ColorTranslator.FromHtml("#173B63");
    private static readonly Color Blue = ColorTranslator.FromHtml("#2879B8");
    private static readonly Color Pale = ColorTranslator.FromHtml("#EEF3F8");
    private static readonly Color Dark = ColorTranslator.FromHtml("#203145");

    private readonly CourseCatalog _catalog = new();
    private readonly ComboBox _year = Cb("1º ano", "2º ano", "3º ano");
    private readonly ComboBox _subject = Cb();
    private readonly ComboBox _term = Cb("1º trimestre", "2º trimestre", "3º trimestre", "Revisão anual");
    private readonly TextBox _class = Tb();
    private readonly ComboBox _type = Cb("Apostila", "Avaliação", "Lista de exercícios", "Tutorial", "Slides", "Imagem técnica");
    private readonly TextBox _theme = Tb();
    private readonly TextBox _details = Tb(multiline: true);
    private readonly ComboBox _level = Cb("Introdutório", "Intermediário", "Avançado");
    private readonly ComboBox _focus = Cb("Predominantemente teórico", "Teórico e prático", "Predominantemente prático");
    private readonly ComboBox _visual = Cb("Discreta", "Equilibrada", "Intensiva");
    private readonly ComboBox _format = Cb("DOCX", "PDF");
    private readonly CheckBox _practical = Check("Incluir exemplos e atividades práticas", true);
    private readonly CheckBox _references = Check("Incluir referências bibliográficas", true);
    private readonly ComboBox _questions = Cb("8 objetivas (A–E) e 2 discursivas", "10 objetivas (A–E)", "10 objetivas (A–E) e 2 discursivas", "10 questões variadas");
    private readonly ComboBox _points = Cb("10", "20", "30");
    private readonly CheckBox _consult = Check("Avaliação com consulta", true);
    private readonly CheckBox _answer = Check("Gabarito comentado em arquivo separado", true);
    private readonly CheckBox _individual = Check("Entrega individual", true);
    private readonly CheckBox _evidence = Check("Exigir prints como evidência de execução", false);
    private readonly ComboBox _slideCount = Cb("8 a 10", "12 a 15", "20 a 25");
    private readonly ComboBox _imageRatio = Cb("4:3 (horizontal)", "16:9 (horizontal)", "1:1 (quadrada)", "3:4 (vertical)");
    private readonly ComboBox _imageType = Cb("Diagrama técnico", "Ilustração conceitual", "Fluxograma", "Infográfico");
    private readonly CheckBox _tutorialPrints = Check("Exigir capturas reais ou indicar prints necessários", true);
    private readonly Label _manualLabel = new() { Text = "Nenhum manual carregado", AutoEllipsis = true, ForeColor = Color.DimGray, AutoSize = false, Height = 26, TextAlign = ContentAlignment.MiddleLeft };
    private readonly RichTextBox _preview = new();
    private readonly Label _status = new();
    private readonly Label _length = new();
    private readonly Dictionary<string, Control> _specificCards = new();
    private readonly List<TableLayoutPanel> _cards = [];
    private FlowLayoutPanel _left = null!;
    private SplitContainer _split = null!;
    private string _manualPath = "";
    private bool _suspendEvents;
    private readonly ComboBox _themeSelector = Cb("Tema claro", "Tema escuro");
    private AppTheme _appTheme = new(false);
    private bool _hasPromptWarning;

    public MainForm()
    {
        Text = "Gerador de Materiais Didáticos | Escola Estadual Olinto Almada";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(1410, 900);
        MinimumSize = new Size(1080, 700);
        BackColor = Pale;
        Font = new Font("Segoe UI", 9.5f);
        AutoScaleMode = AutoScaleMode.Dpi;
        BuildUi();
        LoadTheme();
        ConnectEvents();

        try
        {
            var old = Storage.ReadJson<MaterialConfig>(Storage.LastProfilePath);
            if (old is not null) Apply(old);
            else Apply(new MaterialConfig());
        }
        catch
        {
            Apply(new MaterialConfig());
        }

        Shown += (_, _) =>
        {
            int intended = (int)(_split.ClientSize.Width * .44);
            _split.SplitterDistance = Math.Clamp(intended, 405, _split.ClientSize.Width - 405);
            _split.Panel1MinSize = 400;
            _split.Panel2MinSize = 400;
            ResizeCards();
        };
        FormClosing += (_, _) =>
        {
            try { Storage.WriteJson(Storage.LastProfilePath, CaptureMaterialConfig()); }
            catch { /* Ausência de permissão de gravação não impede o fechamento. */ }
        };
    }

    private static ComboBox Cb(params string[] items)
    {
        var result = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            FlatStyle = FlatStyle.Flat,
            IntegralHeight = false,
            Height = 29,
            Font = new Font("Segoe UI", 9.5f)
        };
        result.Items.AddRange(items);
        if (items.Length > 0) result.SelectedIndex = 0;
        return result;
    }

    private static TextBox Tb(bool multiline = false) => new()
    {
        Multiline = multiline,
        ScrollBars = multiline ? ScrollBars.Vertical : ScrollBars.None,
        BorderStyle = BorderStyle.FixedSingle,
        Font = new Font("Segoe UI", 9.5f)
    };

    private static CheckBox Check(string label, bool selected) => new()
    {
        Text = label,
        Checked = selected,
        AutoSize = true,
        MaximumSize = new Size(460, 0),
        Margin = new Padding(0, 4, 0, 6)
    };

    private static Button Btn(string caption, EventHandler action, bool filled = false)
    {
        var button = new Button
        {
            Text = caption,
            Tag = filled ? ThemeRole.PrimaryButton : null,
            FlatStyle = FlatStyle.Flat,
            BackColor = filled ? Blue : Color.White,
            ForeColor = filled ? Color.White : Navy,
            Font = new Font("Segoe UI Semibold", 9.5f),
            Height = 38,
            Cursor = Cursors.Hand,
            Margin = new Padding(3)
        };
        button.FlatAppearance.BorderColor = filled ? Blue : ColorTranslator.FromHtml("#C7D4E1");
        button.FlatAppearance.BorderSize = 1;
        button.Click += action;
        return button;
    }

    private static Label SectionTitle(string title) => new()
    {
        Text = title,
        Tag = ThemeRole.Heading,
        Font = new Font("Segoe UI Semibold", 11f),
        ForeColor = Navy,
        Height = 32,
        TextAlign = ContentAlignment.MiddleLeft,
        Margin = new Padding(0, 0, 0, 5),
        AutoSize = false
    };

    private static Panel Field(string caption, Control input, int inputHeight = 30)
    {
        var panel = new Panel
        {
            Height = inputHeight + 30,
            Dock = DockStyle.Top,
            Margin = new Padding(0, 0, 0, 10)
        };
        var label = new Label
        {
            Text = caption,
            Dock = DockStyle.Top,
            ForeColor = Dark,
            Height = 23,
            TextAlign = ContentAlignment.BottomLeft,
            Font = new Font("Segoe UI Semibold", 9f)
        };
        input.Dock = DockStyle.Bottom;
        input.Height = inputHeight;
        panel.Controls.Add(input);
        panel.Controls.Add(label);
        return panel;
    }

    private static TableLayoutPanel TwoColumns(Control left, Control right)
    {
        var layout = new TableLayoutPanel
        {
            ColumnCount = 2,
            RowCount = 1,
            Dock = DockStyle.Top,
            Height = 70,
            Margin = new Padding(0, 0, 0, 4)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        left.Margin = new Padding(0, 0, 7, 0);
        right.Margin = new Padding(7, 0, 0, 0);
        layout.Controls.Add(left, 0, 0);
        layout.Controls.Add(right, 1, 0);
        return layout;
    }

    private TableLayoutPanel Card(string title, params Control[] children)
    {
        var card = new TableLayoutPanel
        {
            ColumnCount = 1,
            RowCount = children.Length + 1,
            GrowStyle = TableLayoutPanelGrowStyle.FixedSize,
            Padding = new Padding(15, 12, 15, 11),
            Margin = new Padding(3, 4, 3, 12),
            BackColor = Color.White,
            Tag = ThemeRole.Surface,
            CellBorderStyle = TableLayoutPanelCellBorderStyle.None
        };
        card.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        var heading = SectionTitle(title);
        heading.Dock = DockStyle.Fill;
        card.Controls.Add(heading, 0, 0);
        int total = card.Padding.Vertical + heading.Height + heading.Margin.Vertical;
        card.RowStyles.Add(new RowStyle(SizeType.Absolute, heading.Height + heading.Margin.Vertical));
        for (int i = 0; i < children.Length; i++)
        {
            Control child = children[i];
            if (child is Label) child.Dock = DockStyle.Fill;
            card.Controls.Add(child, 0, i + 1);
            int rowHeight = child.Height + child.Margin.Vertical;
            total += rowHeight;
            card.RowStyles.Add(new RowStyle(SizeType.Absolute, rowHeight));
        }
        card.Height = total + 4;
        _cards.Add(card);
        _left.Controls.Add(card);
        return card;
    }

    private void BuildUi()
    {
        var banner = new Panel { Height = 88, Dock = DockStyle.Top, BackColor = Navy, Tag = ThemeRole.Banner };
        var title = new Label
        {
            Text = "GERADOR DE MATERIAIS DIDÁTICOS",
            Font = new Font("Segoe UI Semibold", 18f),
            ForeColor = Color.White,
            Location = new Point(24, 13),
            AutoSize = true
        };
        var subtitle = new Label
        {
            Text = "Curso Técnico em Informática  •  Planejamento e padronização de conteúdos",
            Font = new Font("Segoe UI", 10f),
            ForeColor = ColorTranslator.FromHtml("#CADCEB"),
            Location = new Point(26, 53),
            AutoSize = true
        };
        banner.Controls.Add(title);
        banner.Controls.Add(subtitle);
        var themePanel = new Panel { Dock = DockStyle.Right, Width = 190, Padding = new Padding(12, 28, 20, 20) };
        _themeSelector.Dock = DockStyle.Top;
        _themeSelector.AccessibleName = "Tema da interface";
        themePanel.Controls.Add(_themeSelector);
        banner.Controls.Add(themePanel);

        _split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            BackColor = Pale,
            SplitterWidth = 7
        };
        Controls.Add(_split);
        Controls.Add(banner);

        _left = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Pale,
            Padding = new Padding(12, 14, 5, 12)
        };
        _split.Panel1.Controls.Add(_left);
        _left.Resize += (_, _) => ResizeCards();

        var editSubjects = Btn("Editar disciplinas deste ano", (_, _) => EditSubjects());
        editSubjects.Dock = DockStyle.Top;
        Card("1  •  Identificação", Field("Ano escolar", _year), Field("Disciplina", _subject),
            TwoColumns(Field("Trimestre", _term), Field("Turma (opcional)", _class)), editSubjects);
        Card("2  •  Conteúdo", Field("Tipo de material", _type),
            Field("Tema principal", _theme), Field("Conteúdos específicos / orientações", _details, 82));
        Card("3  •  Diretrizes pedagógicas", Field("Nível de aprofundamento", _level),
            Field("Enfoque", _focus), Field("Quantidade de elementos visuais", _visual), _practical, _references);

        _specificCards["Avaliação"] = Card("4  •  Opções da avaliação",
            Field("Distribuição das questões", _questions), Field("Valor total (pontos)", _points), _consult, _answer);
        _specificCards["Lista de exercícios"] = Card("4  •  Opções da atividade", _individual, _evidence);
        _specificCards["Tutorial"] = Card("4  •  Opções do tutorial", _tutorialPrints);
        _specificCards["Slides"] = Card("4  •  Opções da apresentação", Field("Quantidade de slides", _slideCount));
        _specificCards["Imagem técnica"] = Card("4  •  Opções da imagem", Field("Categoria visual", _imageType),
            Field("Proporção", _imageRatio));

        var manualPick = Btn("Selecionar manual .md / .txt", (_, _) => SelectManual());
        var manualRemove = Btn("Limpar manual", (_, _) =>
        {
            _manualPath = "";
            _manualLabel.Text = "Nenhum manual carregado";
            RefreshPrompt();
        });
        var manualBtns = TwoColumns(manualPick, manualRemove);
        manualPick.Dock = DockStyle.Fill;
        manualRemove.Dock = DockStyle.Fill;
        Card("5  •  Saída e identidade visual", Field("Formato de entrega", _format),
            _manualLabel, manualBtns);

        var right = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = Pale,
            Padding = new Padding(14, 15, 16, 15)
        };
        right.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 49));
        right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 106));
        right.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        _split.Panel2.Controls.Add(right);

        var previewHeader = new Panel { Dock = DockStyle.Fill };
        previewHeader.Controls.Add(new Label
        {
            Text = "Solicitação gerada",
            Tag = ThemeRole.Heading,
            Font = new Font("Segoe UI Semibold", 13),
            ForeColor = Navy,
            Dock = DockStyle.Left,
            Width = 250,
            TextAlign = ContentAlignment.MiddleLeft
        });
        _length.ForeColor = Color.DimGray;
        _length.Dock = DockStyle.Right;
        _length.Width = 165;
        _length.TextAlign = ContentAlignment.MiddleRight;
        previewHeader.Controls.Add(_length);
        right.Controls.Add(previewHeader, 0, 0);

        _preview.Dock = DockStyle.Fill;
        _preview.BorderStyle = BorderStyle.FixedSingle;
        _preview.BackColor = Color.White;
        _preview.ForeColor = Dark;
        _preview.Font = new Font("Consolas", 9.8f);
        _preview.ReadOnly = true;
        _preview.WordWrap = true;
        _preview.DetectUrls = false;
        _preview.Margin = new Padding(0, 0, 0, 12);
        right.Controls.Add(_preview, 0, 1);

        var buttons = new TableLayoutPanel
        {
            ColumnCount = 2,
            RowCount = 2,
            Dock = DockStyle.Fill,
            Margin = new Padding(0)
        };
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        buttons.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        buttons.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        var copy = Btn("Copiar prompt", (_, _) => CopyPrompt(), true);
        var save = Btn("Exportar TXT", (_, _) => ExportPrompt());
        var saveProfile = Btn("Salvar perfil", (_, _) => SaveProfile());
        var loadProfile = Btn("Carregar perfil", (_, _) => LoadProfile());
        foreach (var b in new[] { copy, save, saveProfile, loadProfile }) b.Dock = DockStyle.Fill;
        buttons.Controls.Add(copy, 0, 0);
        buttons.Controls.Add(save, 1, 0);
        buttons.Controls.Add(saveProfile, 0, 1);
        buttons.Controls.Add(loadProfile, 1, 1);
        right.Controls.Add(buttons, 0, 2);

        var footer = new Panel { Dock = DockStyle.Fill };
        _status.Dock = DockStyle.Fill;
        _status.Text = "Pronto para configurar o material.";
        _status.TextAlign = ContentAlignment.MiddleLeft;
        _status.ForeColor = Color.DimGray;
        _status.AutoEllipsis = true;
        var openChat = Btn("Abrir ChatGPT ↗", (_, _) => OpenChat());
        openChat.Dock = DockStyle.Right;
        openChat.Width = 160;
        footer.Controls.Add(_status);
        footer.Controls.Add(openChat);
        right.Controls.Add(footer, 0, 3);
    }

    private void ResizeCards()
    {
        if (_left is null) return;
        int width = Math.Max(385, _left.ClientSize.Width - _left.Padding.Horizontal - 11);
        foreach (var card in _cards) card.Width = width;
    }

    private void LoadTheme()
    {
        bool dark = false;
        try { dark = Storage.ReadJson<AppSettings>(Storage.SettingsPath)?.DarkTheme ?? false; }
        catch { /* Preferências inválidas não impedem a abertura. */ }
        _themeSelector.SelectedIndex = dark ? 1 : 0;
        ApplyTheme(dark);
        _themeSelector.SelectedIndexChanged += (_, _) =>
        {
            bool selectedDark = _themeSelector.SelectedIndex == 1;
            ApplyTheme(selectedDark);
            try { Storage.WriteJson(Storage.SettingsPath, new AppSettings { DarkTheme = selectedDark }); }
            catch (Exception ex) { ShowError(ex); }
        };
    }

    private void ApplyTheme(bool dark)
    {
        _appTheme = new AppTheme(dark);
        _manualLabel.Tag = ThemeRole.Muted;
        _length.Tag = ThemeRole.Muted;
        SuspendLayout();
        try
        {
            _appTheme.Apply(this);
            _status.ForeColor = _hasPromptWarning ? _appTheme.Warning : _appTheme.Muted;
        }
        finally { ResumeLayout(true); }
        Invalidate(true);
    }

    private void ConnectEvents()
    {
        _year.SelectedIndexChanged += (_, _) =>
        {
            if (_suspendEvents) return;
            FillSubjects();
            RefreshPrompt();
        };
        _type.SelectedIndexChanged += (_, _) =>
        {
            if (_suspendEvents) return;
            UpdateType(true);
            RefreshPrompt();
        };
        foreach (ComboBox control in new[] { _subject, _term, _level, _focus, _visual, _format,
                     _questions, _points, _slideCount, _imageRatio, _imageType })
            control.SelectedIndexChanged += (_, _) => RefreshPrompt();
        foreach (TextBox control in new[] { _class, _theme, _details })
            control.TextChanged += (_, _) => RefreshPrompt();
        foreach (CheckBox control in new[] { _practical, _references, _consult, _answer,
                     _individual, _evidence, _tutorialPrints })
            control.CheckedChanged += (_, _) => RefreshPrompt();
    }

    private void FillSubjects(string? selected = null)
    {
        string? old = selected ?? _subject.SelectedItem?.ToString();
        _subject.Items.Clear();
        foreach (string item in _catalog.Subjects(_year.Text)) _subject.Items.Add(item);
        if (_subject.Items.Count > 0)
        {
            int index = _subject.Items.IndexOf(old ?? "");
            _subject.SelectedIndex = index >= 0 ? index : 0;
        }
    }

    private void UpdateType(bool resetFormat)
    {
        string type = _type.Text;
        foreach (var item in _specificCards)
            item.Value.Visible = item.Key == type;
        string current = resetFormat ? "" : _format.Text;
        string[] formats = type switch
        {
            "Slides" => ["PPTX", "PDF"],
            "Imagem técnica" => ["PNG"],
            _ => ["DOCX", "PDF"]
        };
        _format.Items.Clear();
        _format.Items.AddRange(formats);
        _format.SelectedItem = formats.Contains(current) ? current : formats[0];
        _practical.Visible = type is "Apostila" or "Tutorial" or "Slides";
        _references.Visible = type != "Imagem técnica";
    }

    private static void Select(ComboBox combo, string value)
    {
        int idx = combo.Items.IndexOf(value);
        if (idx >= 0) combo.SelectedIndex = idx;
    }

    private void Apply(MaterialConfig c)
    {
        _suspendEvents = true;
        try
        {
            Select(_year, c.Ano);
            FillSubjects(c.Disciplina);
            Select(_term, c.Trimestre);
            _class.Text = c.Turma;
            Select(_type, c.Tipo);
            _theme.Text = c.Tema;
            _details.Text = c.Detalhes;
            Select(_level, c.Nivel);
            Select(_focus, c.Enfoque);
            Select(_visual, c.Visual);
            _practical.Checked = c.AtividadesPraticas;
            _references.Checked = c.Referencias;
            Select(_questions, c.QuestaoModelo);
            Select(_points, c.Pontos);
            _consult.Checked = c.ComConsulta;
            _answer.Checked = c.Gabarito;
            _individual.Checked = c.Individual;
            _evidence.Checked = c.Evidencias;
            Select(_slideCount, c.QuantidadeSlides);
            Select(_imageRatio, c.ProporcaoImagem);
            Select(_imageType, c.TipoImagem);
            _tutorialPrints.Checked = c.PrintsTutorial;
            _manualPath = c.ManualPath;
            _manualLabel.Text = _manualPath.Length == 0 ? "Nenhum manual carregado" : Path.GetFileName(_manualPath);
            UpdateType(false);
            Select(_format, c.Formato);
        }
        finally { _suspendEvents = false; }
        RefreshPrompt();
    }

    private MaterialConfig CaptureMaterialConfig() => new()
    {
        Ano = _year.Text,
        Disciplina = _subject.Text,
        Trimestre = _term.Text,
        Turma = _class.Text,
        Tipo = _type.Text,
        Tema = _theme.Text,
        Detalhes = _details.Text,
        Nivel = _level.Text,
        Enfoque = _focus.Text,
        Visual = _visual.Text,
        Formato = _format.Text,
        AtividadesPraticas = _practical.Checked,
        Referencias = _references.Checked,
        QuestaoModelo = _questions.Text,
        Pontos = _points.Text,
        ComConsulta = _consult.Checked,
        Gabarito = _answer.Checked,
        Individual = _individual.Checked,
        Evidencias = _evidence.Checked,
        QuantidadeSlides = _slideCount.Text,
        ProporcaoImagem = _imageRatio.Text,
        TipoImagem = _imageType.Text,
        PrintsTutorial = _tutorialPrints.Checked,
        ManualPath = _manualPath
    };

    private void RefreshPrompt()
    {
        if (_suspendEvents || _preview.IsDisposed) return;
        string generated = PromptGenerator.Build(CaptureMaterialConfig(), out string? warning);
        _preview.Text = generated;
        _length.Text = $"{generated.Length:N0} caracteres";
        _status.Text = warning ?? "Configuração pronta. Copie o prompt e utilize-o no ChatGPT.";
        _hasPromptWarning = warning is not null;
        _status.ForeColor = _hasPromptWarning ? _appTheme.Warning : _appTheme.Muted;
    }

    private void CopyPrompt()
    {
        try
        {
            Clipboard.SetText(_preview.Text);
            _status.Text = "Prompt copiado para a área de transferência.";
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private void ExportPrompt()
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "Arquivo de texto (*.txt)|*.txt|Markdown (*.md)|*.md",
            Title = "Exportar solicitação didática",
            FileName = "Prompt_" + SafeFileName(_type.Text) + ".txt"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            File.WriteAllText(dialog.FileName, _preview.Text, new UTF8Encoding(true));
            _status.Text = "Prompt exportado: " + Path.GetFileName(dialog.FileName);
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private static string SafeFileName(string input)
    {
        foreach (char ch in Path.GetInvalidFileNameChars()) input = input.Replace(ch, '_');
        return input.Replace(' ', '_');
    }

    private void SaveProfile()
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "Perfil de material (*.json)|*.json",
            Title = "Salvar perfil de configuração",
            FileName = "Perfil_" + SafeFileName(_subject.Text) + ".json"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            Storage.WriteJson(dialog.FileName, CaptureMaterialConfig());
            _status.Text = "Perfil salvo com sucesso.";
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private void LoadProfile()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Perfil de material (*.json)|*.json",
            Title = "Carregar perfil"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            var config = Storage.ReadJson<MaterialConfig>(dialog.FileName)
                ?? throw new InvalidDataException("O perfil está vazio.");
            Apply(config);
            _status.Text = "Perfil carregado com sucesso.";
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private void SelectManual()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Manuais de texto (*.md;*.txt)|*.md;*.txt|Todos os arquivos (*.*)|*.*",
            Title = "Selecionar manual de identidade visual"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        _manualPath = dialog.FileName;
        _manualLabel.Text = Path.GetFileName(_manualPath);
        RefreshPrompt();
    }

    private void EditSubjects()
    {
        string year = _year.Text;
        using var dialog = new Form
        {
            Text = "Editar disciplinas — " + year,
            StartPosition = FormStartPosition.CenterParent,
            Size = new Size(565, 530),
            MinimumSize = new Size(460, 390),
            BackColor = Pale,
            Font = Font
        };
        var main = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            RowCount = 3,
            ColumnCount = 1
        };
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        main.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        main.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
        main.Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            Text = $"Disciplinas do {year}: informe uma por linha.",
            Tag = ThemeRole.Heading,
            ForeColor = Navy,
            Font = new Font("Segoe UI Semibold", 11),
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, 0);
        var input = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            Font = new Font("Segoe UI", 10),
            Lines = _catalog.Subjects(year).ToArray()
        };
        main.Controls.Add(input, 0, 1);
        var save = Btn("Salvar disciplinas", (_, _) =>
        {
            try
            {
                _catalog.SetSubjects(year, input.Text);
                dialog.DialogResult = DialogResult.OK;
                dialog.Close();
            }
            catch (Exception ex) { MessageBox.Show(dialog, ex.Message, "Verifique a lista", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }, true);
        save.Dock = DockStyle.Right;
        save.Width = 190;
        main.Controls.Add(save, 0, 2);
        dialog.Controls.Add(main);
        _appTheme.Apply(dialog);
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            FillSubjects();
            RefreshPrompt();
        }
    }

    private void OpenChat()
    {
        try
        {
            Process.Start(new ProcessStartInfo("https://chatgpt.com/") { UseShellExecute = true });
            _status.Text = "ChatGPT aberto no navegador. Utilize 'Copiar prompt' para transferir as instruções.";
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private void ShowError(Exception exception) => MessageBox.Show(this,
        exception.Message, "Não foi possível concluir a operação", MessageBoxButtons.OK, MessageBoxIcon.Error);
}
