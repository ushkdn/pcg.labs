using Application.Services;
using Domain.Entities;
using Infrastructure.Repositories;

namespace Presentation;

public partial class Form1 : Form
{
    private readonly GraphicContainer container = new();
    private readonly ContainerService containerService;
    private readonly Panel canvas;
    private readonly Button btnNewPalette;
    private readonly Button btnEditPalette;
    private readonly Button btnClearPixels;
    private readonly Button btnSave;
    private readonly Button btnContrast;
    private readonly Button btnLoad;
    private readonly Button btnSaveAscii;
    private readonly Button btnTransformations;
    private TransformationParameters currentTransform = new();
    private ColorTransformationParameters currentColorTransform = new();
    private GraphicContainer previewContainer = new();
    private bool isPreviewActive = false;
    private readonly Button btnLoadAscii;
    private readonly Button btnShowAscii;
    private readonly AsciiContainerService asciiService;
    private readonly Label lblInfo;
    private readonly Random rnd = new Random();
    private readonly Button btnGeneratePattern;
    private PatternType currentPatternType = PatternType.Tiles;
    private Color primaryColor = Color.Red;
    private Color secondaryColor = Color.Blue;
    private int elementSize = 20;
    private int patternWidth = 400;
    private int patternHeight = 400;

    private readonly float radius = 180f;

    public Form1()
    {
        // Create repository and use-cases (no DI)
        var fileContainerRepository = new FileContainerRepository();
        containerService = new ContainerService(fileContainerRepository);

        Text = "Hex Palette Graphic Container (Clean Arch)";
        ClientSize = new Size(800, 600);
        StartPosition = FormStartPosition.CenterScreen;

        btnNewPalette = new Button { Text = "Создать палитру", Location = new Point(620, 20), Width = 140 };
        btnEditPalette = new Button { Text = "Редактировать палитру", Location = new Point(620, 60), Width = 140 };
        btnClearPixels = new Button { Text = "Очистить пиксели", Location = new Point(620, 100), Width = 140 };
        btnSave = new Button { Text = "Сохранить контейнер", Location = new Point(620, 140), Width = 140 };
        btnLoad = new Button { Text = "Загрузить контейнер", Location = new Point(620, 180), Width = 140 };
        btnContrast = new Button { Text = "Изменить контраст", Location = new Point(620, 220), Width = 140 };
        Controls.Add(btnContrast);
        btnContrast.Click += BtnContrast_Click;
        var asciiRepository = new AsciiContainerRepository();
        asciiService = new AsciiContainerService(asciiRepository);
        btnTransformations = new Button
        {
            Text = "Преобразования",
            Location = new Point(620, 300),
            Width = 140
        };
        Controls.Add(btnTransformations);
        btnTransformations.Click += BtnTransformations_Click;

        // Добавляем кнопки для работы с ASCII
        btnSaveAscii = new Button
        {
            Text = "Сохранить как ASCII",
            Location = new Point(620, 380),
            Width = 140
        };

        btnLoadAscii = new Button
        {
            Text = "Загрузить из ASCII",
            Location = new Point(620, 420),
            Width = 140
        };

        btnShowAscii = new Button
        {
            Text = "Показать ASCII",
            Location = new Point(620, 460),
            Width = 140
        };

        Controls.Add(btnSaveAscii);
        Controls.Add(btnLoadAscii);
        Controls.Add(btnShowAscii);

        btnSaveAscii.Click += BtnSaveAscii_Click;
        btnLoadAscii.Click += BtnLoadAscii_Click;
        btnShowAscii.Click += BtnShowAscii_Click;

        lblInfo = new Label
        {
            Text = "ЛКМ: добавить пиксель\nПКМ: удалить ближайший\n" +
                  "Редактировать палитру — изменить вершины\n" +
                  "ASCII: текстовое представление контейнера",
            Location = new Point(620, 520),
            Width = 160,
            Height = 80
        };

        canvas = new Panel
        {
            Location = new Point(20, 20),
            Size = new Size(560, 560),
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.White
        };
        btnGeneratePattern = new Button
        {
            Text = "Сгенерировать узор",
            Location = new Point(620, 260),
            Width = 140
        };
        Controls.Add(btnGeneratePattern);
        btnGeneratePattern.Click += BtnGeneratePattern_Click;

        Controls.AddRange(new Control[] { canvas, btnNewPalette, btnEditPalette, btnClearPixels, btnSave, btnLoad, lblInfo });

        // events
        canvas.Paint += Canvas_Paint;
        canvas.MouseClick += Canvas_MouseClick;
        btnNewPalette.Click += (s, e) => { CreateDefaultPalette(); canvas.Invalidate(); };
        btnEditPalette.Click += BtnEditPalette_Click;
        btnClearPixels.Click += (s, e) => { container.Pixels.Clear(); canvas.Invalidate(); };
        btnSave.Click += BtnSave_Click;
        btnLoad.Click += BtnLoad_Click;

        CreateDefaultPalette();
    }

    private void BtnTransformations_Click(object? sender, EventArgs e)
    {
        using var transformDialog = new Form
        {
            Text = "Управление преобразованиями",
            Size = new Size(500, 600),
            StartPosition = FormStartPosition.CenterParent
        };

        var tabControl = new TabControl
        {
            Dock = DockStyle.Fill
        };

        // Вкладка геометрических преобразований
        var geometricTab = CreateGeometricTransformationTab();
        tabControl.TabPages.Add(geometricTab);

        // Вкладка цветовых преобразований
        var colorTab = CreateColorTransformationTab();
        tabControl.TabPages.Add(colorTab);

        // Вкладка предпросмотра
        var previewTab = CreatePreviewTab();
        tabControl.TabPages.Add(previewTab);

        // Кнопки применения и отмены
        var buttonPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Bottom,
            Height = 40
        };

        var btnApply = new Button { Text = "Применить", Width = 80 };
        var btnCancel = new Button { Text = "Отмена", Width = 80 };

        btnApply.Click += (s, e) =>
        {
            ApplyTransformationsToMainContainer();
            transformDialog.DialogResult = DialogResult.OK;
        };

        btnCancel.Click += (s, e) =>
        {
            isPreviewActive = false;
            canvas.Invalidate();
            transformDialog.DialogResult = DialogResult.Cancel;
        };

        buttonPanel.Controls.AddRange(new Control[] { btnCancel, btnApply });

        var mainPanel = new Panel { Dock = DockStyle.Fill };
        mainPanel.Controls.Add(tabControl);
        mainPanel.Controls.Add(buttonPanel);

        transformDialog.Controls.Add(mainPanel);

        // Инициализируем предпросмотр
        UpdatePreviewContainer();

        if (transformDialog.ShowDialog() == DialogResult.OK)
        {
            canvas.Invalidate();
            MessageBox.Show("Преобразования применены к основному контейнеру!");
        }
        else
        {
            // Отменяем предпросмотр
            isPreviewActive = false;
            canvas.Invalidate();
        }
    }

    private void ApplyTransformationsToMainContainer()
    {
        // Применяем геометрические преобразования
        var transformedContainer = TransformationService.ApplyTransformations(container, currentTransform);

        // Заменяем пиксели в основном контейнере
        container.Pixels.Clear();
        foreach (var pixel in transformedContainer.Pixels)
            container.Pixels.Add(pixel);

        // Применяем цветовые преобразования к палитре
        ApplyColorTransformationsToPalette();

        isPreviewActive = false;
    }

    private void ApplyColorTransformationsToPalette()
    {
        if (currentColorTransform.Brightness == 0 &&
            currentColorTransform.Contrast == 1.0f &&
            currentColorTransform.Saturation == 1.0f &&
            currentColorTransform.Hue == 0f)
        {
            return; // Нет изменений
        }

        var newPalette = new List<ColorVertex>();
        foreach (var vertex in container.Palette)
        {
            var transformedColor = TransformationService.ApplySingleColorTransformations(
                vertex.Color, currentColorTransform);
            newPalette.Add(new ColorVertex(transformedColor, vertex.Position));
        }

        container.Palette.Clear();
        foreach (var vertex in newPalette)
            container.Palette.Add(vertex);
    }

    private void UpdatePreviewContainer()
    {
        // Копируем основной контейнер в предпросмотр
        previewContainer.Clear();

        // Копируем палитру с цветовыми преобразованиями
        foreach (var vertex in container.Palette)
        {
            var transformedColor = TransformationService.ApplySingleColorTransformations(
                vertex.Color, currentColorTransform);
            previewContainer.Palette.Add(new ColorVertex(transformedColor, vertex.Position));
        }

        // Копируем пиксели с геометрическими преобразованиями
        var transformedPixels = TransformationService.ApplyTransformations(container, currentTransform);
        foreach (var pixel in transformedPixels.Pixels)
        {
            previewContainer.Pixels.Add(pixel);
        }

        isPreviewActive = true;
        canvas.Invalidate();
    }

    private TabPage CreateGeometricTransformationTab()
    {
        var tab = new TabPage("Геометрические");

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 8,
            Padding = new Padding(10)
        };

        // Масштаб
        layout.Controls.Add(new Label { Text = "Масштаб:", AutoSize = true });
        var trackScale = new TrackBar
        {
            Minimum = 10,
            Maximum = 200,
            Value = (int)(currentTransform.Scale * 100),
            TickFrequency = 10
        };
        var lblScale = new Label { Text = $"{currentTransform.Scale:F2}" };
        trackScale.ValueChanged += (s, e) =>
        {
            currentTransform = currentTransform with { Scale = trackScale.Value / 100f };
            lblScale.Text = $"{currentTransform.Scale:F2}";
            UpdatePreviewContainer();
        };
        layout.Controls.Add(CreateControlWithLabel(trackScale, lblScale));

        // Поворот
        layout.Controls.Add(new Label { Text = "Поворот (°):", AutoSize = true });
        var trackRotation = new TrackBar
        {
            Minimum = -180,
            Maximum = 180,
            Value = (int)currentTransform.Rotation,
            TickFrequency = 45
        };
        var lblRotation = new Label { Text = $"{currentTransform.Rotation:F0}°" };
        trackRotation.ValueChanged += (s, e) =>
        {
            currentTransform = currentTransform with { Rotation = trackRotation.Value };
            lblRotation.Text = $"{currentTransform.Rotation:F0}°";
            UpdatePreviewContainer();
        };
        layout.Controls.Add(CreateControlWithLabel(trackRotation, lblRotation));

        // Сдвиг по X
        layout.Controls.Add(new Label { Text = "Сдвиг X:", AutoSize = true });
        var trackOffsetX = new TrackBar
        {
            Minimum = -100,
            Maximum = 100,
            Value = (int)(currentTransform.OffsetX * 50),
            TickFrequency = 25
        };
        var lblOffsetX = new Label { Text = $"{currentTransform.OffsetX:F2}" };
        trackOffsetX.ValueChanged += (s, e) =>
        {
            currentTransform = currentTransform with { OffsetX = trackOffsetX.Value / 50f };
            lblOffsetX.Text = $"{currentTransform.OffsetX:F2}";
            UpdatePreviewContainer();
        };
        layout.Controls.Add(CreateControlWithLabel(trackOffsetX, lblOffsetX));

        // Сдвиг по Y
        layout.Controls.Add(new Label { Text = "Сдвиг Y:", AutoSize = true });
        var trackOffsetY = new TrackBar
        {
            Minimum = -100,
            Maximum = 100,
            Value = (int)(currentTransform.OffsetY * 50),
            TickFrequency = 25
        };
        var lblOffsetY = new Label { Text = $"{currentTransform.OffsetY:F2}" };
        trackOffsetY.ValueChanged += (s, e) =>
        {
            currentTransform = currentTransform with { OffsetY = trackOffsetY.Value / 50f };
            lblOffsetY.Text = $"{currentTransform.OffsetY:F2}";
            UpdatePreviewContainer();
        };
        layout.Controls.Add(CreateControlWithLabel(trackOffsetY, lblOffsetY));

        // Отражение по X
        layout.Controls.Add(new Label { Text = "Отражение X:", AutoSize = true });
        var chkMirrorX = new CheckBox
        {
            Checked = currentTransform.MirrorX
        };
        chkMirrorX.CheckedChanged += (s, e) =>
        {
            currentTransform = currentTransform with { MirrorX = chkMirrorX.Checked };
            UpdatePreviewContainer();
        };
        layout.Controls.Add(chkMirrorX);

        // Отражение по Y
        layout.Controls.Add(new Label { Text = "Отражение Y:", AutoSize = true });
        var chkMirrorY = new CheckBox
        {
            Checked = currentTransform.MirrorY
        };
        chkMirrorY.CheckedChanged += (s, e) =>
        {
            currentTransform = currentTransform with { MirrorY = chkMirrorY.Checked };
            UpdatePreviewContainer();
        };
        layout.Controls.Add(chkMirrorY);

        // Кнопка сброса
        var btnReset = new Button { Text = "Сброс геометрии", Dock = DockStyle.Bottom };
        btnReset.Click += (s, e) =>
        {
            currentTransform = new TransformationParameters();
            trackScale.Value = 100;
            trackRotation.Value = 0;
            trackOffsetX.Value = 0;
            trackOffsetY.Value = 0;
            chkMirrorX.Checked = false;
            chkMirrorY.Checked = false;
            UpdatePreviewContainer();
        };

        var mainPanel = new Panel { Dock = DockStyle.Fill };
        mainPanel.Controls.Add(layout);
        mainPanel.Controls.Add(btnReset);

        tab.Controls.Add(mainPanel);
        return tab;
    }

    private TabPage CreateColorTransformationTab()
    {
        var tab = new TabPage("Цветовые");

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 6,
            Padding = new Padding(10)
        };

        // Яркость
        layout.Controls.Add(new Label { Text = "Яркость:", AutoSize = true });
        var trackBrightness = new TrackBar
        {
            Minimum = -100,
            Maximum = 100,
            Value = currentColorTransform.Brightness,
            TickFrequency = 25
        };
        var lblBrightness = new Label { Text = $"{currentColorTransform.Brightness}" };
        trackBrightness.ValueChanged += (s, e) =>
        {
            currentColorTransform = currentColorTransform with { Brightness = trackBrightness.Value };
            lblBrightness.Text = $"{currentColorTransform.Brightness}";
            UpdatePreviewContainer();
        };
        layout.Controls.Add(CreateControlWithLabel(trackBrightness, lblBrightness));

        // Контрастность
        layout.Controls.Add(new Label { Text = "Контрастность:", AutoSize = true });
        var trackContrast = new TrackBar
        {
            Minimum = 0,
            Maximum = 300,
            Value = (int)(currentColorTransform.Contrast * 100),
            TickFrequency = 25
        };
        var lblContrast = new Label { Text = $"{currentColorTransform.Contrast:F2}" };
        trackContrast.ValueChanged += (s, e) =>
        {
            currentColorTransform = currentColorTransform with { Contrast = trackContrast.Value / 100f };
            lblContrast.Text = $"{currentColorTransform.Contrast:F2}";
            UpdatePreviewContainer();
        };
        layout.Controls.Add(CreateControlWithLabel(trackContrast, lblContrast));

        // Насыщенность
        layout.Controls.Add(new Label { Text = "Насыщенность:", AutoSize = true });
        var trackSaturation = new TrackBar
        {
            Minimum = 0,
            Maximum = 300,
            Value = (int)(currentColorTransform.Saturation * 100),
            TickFrequency = 25
        };
        var lblSaturation = new Label { Text = $"{currentColorTransform.Saturation:F2}" };
        trackSaturation.ValueChanged += (s, e) =>
        {
            currentColorTransform = currentColorTransform with { Saturation = trackSaturation.Value / 100f };
            lblSaturation.Text = $"{currentColorTransform.Saturation:F2}";
            UpdatePreviewContainer();
        };
        layout.Controls.Add(CreateControlWithLabel(trackSaturation, lblSaturation));

        // Оттенок
        layout.Controls.Add(new Label { Text = "Оттенок:", AutoSize = true });
        var trackHue = new TrackBar
        {
            Minimum = -100,
            Maximum = 100,
            Value = (int)currentColorTransform.Hue,
            TickFrequency = 25
        };
        var lblHue = new Label { Text = $"{currentColorTransform.Hue:F0}" };
        trackHue.ValueChanged += (s, e) =>
        {
            currentColorTransform = currentColorTransform with { Hue = trackHue.Value };
            lblHue.Text = $"{currentColorTransform.Hue:F0}";
            UpdatePreviewContainer();
        };
        layout.Controls.Add(CreateControlWithLabel(trackHue, lblHue));

        // Кнопка сброса
        var btnReset = new Button { Text = "Сброс цветов", Dock = DockStyle.Bottom };
        btnReset.Click += (s, e) =>
        {
            currentColorTransform = new ColorTransformationParameters();
            trackBrightness.Value = 0;
            trackContrast.Value = 100;
            trackSaturation.Value = 100;
            trackHue.Value = 0;
            UpdatePreviewContainer();
        };

        var mainPanel = new Panel { Dock = DockStyle.Fill };
        mainPanel.Controls.Add(layout);
        mainPanel.Controls.Add(btnReset);

        tab.Controls.Add(mainPanel);
        return tab;
    }

    private TabPage CreatePreviewTab()
    {
        var tab = new TabPage("Предпросмотр");

        var previewPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.White
        };

        previewPanel.Paint += (s, e) => DrawPreviewPanel(e.Graphics, previewPanel.Size);

        tab.Controls.Add(previewPanel);
        return tab;
    }

    private void DrawPreviewPanel(Graphics g, Size size)
    {
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        var center = new PointF(size.Width / 2f, size.Height / 2f);
        float previewRadius = Math.Min(size.Width, size.Height) * 0.4f;

        g.Clear(Color.White);

        // Используем контейнер предпросмотра
        var displayContainer = previewContainer;

        // draw hex outline
        var hexPts = new PointF[6];
        for (int i = 0; i < 6; i++)
        {
            double angle = Math.PI / 3.0 * i;
            float px = center.X + (float)Math.Cos(angle) * previewRadius;
            float py = center.Y + (float)Math.Sin(angle) * previewRadius;
            hexPts[i] = new PointF(px, py);
        }
        using var pen = new Pen(Color.Gray, 2);
        g.DrawPolygon(pen, hexPts);

        // draw vertices
        foreach (var v in displayContainer.Palette)
        {
            var screenPos = new PointF(center.X + v.Position.X * previewRadius, center.Y + v.Position.Y * previewRadius);
            using var brush = new SolidBrush(v.Color);
            g.FillEllipse(brush, screenPos.X - 8, screenPos.Y - 8, 16, 16);
            using var p = new Pen(Color.Black, 1);
            g.DrawEllipse(p, screenPos.X - 8, screenPos.Y - 8, 16, 16);
        }

        // draw connecting lines
        for (int i = 0; i < displayContainer.Palette.Count; i++)
        {
            var a = displayContainer.Palette[i];
            var b = displayContainer.Palette[(i + 1) % displayContainer.Palette.Count];
            var sa = new PointF(center.X + a.Position.X * previewRadius, center.Y + a.Position.Y * previewRadius);
            var sb = new PointF(center.X + b.Position.X * previewRadius, center.Y + b.Position.Y * previewRadius);
            using var p = new Pen(Color.DarkGray, 1);
            g.DrawLine(p, sa, sb);
        }

        // draw pixels using application logic
        foreach (var px in displayContainer.Pixels)
        {
            var color = PixelProcessingService.GetColorFromPixel(px, displayContainer);
            var screen = new PointF(center.X + px.X * previewRadius, center.Y + px.Y * previewRadius);
            using var brush = new SolidBrush(color);
            g.FillEllipse(brush, screen.X - 3, screen.Y - 3, 6, 6);
        }

        // center (black)
        g.FillEllipse(Brushes.Black, center.X - 4, center.Y - 4, 8, 8);
    }

    private Control CreateControlWithLabel(Control control, Label label)
    {
        var panel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true
        };
        panel.Controls.Add(control);
        panel.Controls.Add(label);
        return panel;
    }

    private void BtnSaveAscii_Click(object? sender, EventArgs e)
    {
        using var sfd = new SaveFileDialog
        {
            Filter = "ASCII container (*.asciic)|*.asciic|Text files (*.txt)|*.txt|All files|*.*",
            FileName = "container.asciic"
        };

        if (sfd.ShowDialog() == DialogResult.OK)
        {
            try
            {
                asciiService.SaveAsAscii(sfd.FileName, container);
                MessageBox.Show($"ASCII контейнер сохранен!\nФайл: {sfd.FileName}\n\n" +
                               $"Палитра: {container.Palette.Count} цветов\n" +
                               $"Пикселей: {container.Pixels.Count}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения ASCII: {ex.Message}");
            }
        }
    }

    private void BtnLoadAscii_Click(object? sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog
        {
            Filter = "ASCII container (*.asciic)|*.asciic|Text files (*.txt)|*.txt|All files|*.*"
        };

        if (ofd.ShowDialog() == DialogResult.OK)
        {
            try
            {
                var loadedContainer = asciiService.LoadFromAscii(ofd.FileName);

                // Заменяем текущий контейнер
                container.Palette.Clear();
                foreach (var vertex in loadedContainer.Palette)
                    container.Palette.Add(vertex);

                container.Pixels.Clear();
                foreach (var pixel in loadedContainer.Pixels)
                    container.Pixels.Add(pixel);

                canvas.Invalidate();

                MessageBox.Show($"ASCII контейнер загружен!\n\n" +
                               $"Палитра: {container.Palette.Count} цветов\n" +
                               $"Пикселей: {container.Pixels.Count}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки ASCII: {ex.Message}");
            }
        }
    }

    private void BtnShowAscii_Click(object? sender, EventArgs e)
    {
        try
        {
            var asciiContent = asciiService.GetAsciiRepresentation(container);

            using var previewDialog = new Form
            {
                Text = "ASCII представление контейнера",
                Size = new Size(600, 500),
                StartPosition = FormStartPosition.CenterParent,
                Font = new Font("Consolas", 9)
            };

            var textBox = new TextBox
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Both,
                Text = asciiContent,
                ReadOnly = true,
                WordWrap = false
            };

            var buttonPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Bottom,
                Height = 40
            };

            var btnCopy = new Button { Text = "Копировать", Width = 100 };
            var btnClose = new Button { Text = "Закрыть", Width = 80 };

            btnCopy.Click += (s, e) =>
            {
                Clipboard.SetText(asciiContent);
                MessageBox.Show("ASCII представление скопировано в буфер обмена");
            };

            btnClose.Click += (s, e) => previewDialog.Close();

            buttonPanel.Controls.AddRange(new Control[] { btnClose, btnCopy });

            previewDialog.Controls.Add(textBox);
            previewDialog.Controls.Add(buttonPanel);

            previewDialog.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка отображения ASCII: {ex.Message}");
        }
    }

    private void BtnGeneratePattern_Click(object? sender, EventArgs e)
    {
        using var patternDialog = new Form
        {
            Text = "Генератор узоров",
            Size = new Size(500, 400),
            StartPosition = FormStartPosition.CenterParent
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 7
        };

        // Выбор типа узора
        layout.Controls.Add(new Label { Text = "Тип узора:", AutoSize = true });
        var cmbPatternType = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbPatternType.Items.AddRange(Enum.GetNames(typeof(PatternType)));
        cmbPatternType.SelectedItem = currentPatternType.ToString();
        layout.Controls.Add(cmbPatternType);

        // Основной цвет
        layout.Controls.Add(new Label { Text = "Основной цвет:", AutoSize = true });
        var btnPrimaryColor = new Button
        {
            Text = "Выбрать",
            BackColor = primaryColor,
            ForeColor = Invert(primaryColor)
        };
        btnPrimaryColor.Click += (s, e) =>
        {
            using var cd = new ColorDialog { Color = primaryColor };
            if (cd.ShowDialog() == DialogResult.OK)
            {
                primaryColor = cd.Color;
                btnPrimaryColor.BackColor = primaryColor;
                btnPrimaryColor.ForeColor = Invert(primaryColor);
            }
        };
        layout.Controls.Add(btnPrimaryColor);

        // Вторичный цвет
        layout.Controls.Add(new Label { Text = "Вторичный цвет:", AutoSize = true });
        var btnSecondaryColor = new Button
        {
            Text = "Выбрать",
            BackColor = secondaryColor,
            ForeColor = Invert(secondaryColor)
        };
        btnSecondaryColor.Click += (s, e) =>
        {
            using var cd = new ColorDialog { Color = secondaryColor };
            if (cd.ShowDialog() == DialogResult.OK)
            {
                secondaryColor = cd.Color;
                btnSecondaryColor.BackColor = secondaryColor;
                btnSecondaryColor.ForeColor = Invert(secondaryColor);
            }
        };
        layout.Controls.Add(btnSecondaryColor);

        // Размер элемента
        layout.Controls.Add(new Label { Text = "Размер элемента:", AutoSize = true });
        var numElementSize = new NumericUpDown
        {
            Minimum = 5,
            Maximum = 100,
            Value = elementSize
        };
        layout.Controls.Add(numElementSize);

        // Ширина изображения
        layout.Controls.Add(new Label { Text = "Ширина:", AutoSize = true });
        var numWidth = new NumericUpDown
        {
            Minimum = 100,
            Maximum = 1000,
            Value = patternWidth
        };
        layout.Controls.Add(numWidth);

        // Высота изображения
        layout.Controls.Add(new Label { Text = "Высота:", AutoSize = true });
        var numHeight = new NumericUpDown
        {
            Minimum = 100,
            Maximum = 1000,
            Value = patternHeight
        };
        layout.Controls.Add(numHeight);

        // Кнопки генерации и отмены
        var btnGenerate = new Button { Text = "Сгенерировать", DialogResult = DialogResult.OK };
        var btnCancel = new Button { Text = "Отмена", DialogResult = DialogResult.Cancel };

        var buttonPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Bottom,
            Height = 40
        };
        buttonPanel.Controls.AddRange(new Control[] { btnGenerate, btnCancel });

        patternDialog.Controls.Add(layout);
        patternDialog.Controls.Add(buttonPanel);
        patternDialog.AcceptButton = btnGenerate;
        patternDialog.CancelButton = btnCancel;

        if (patternDialog.ShowDialog() == DialogResult.OK)
        {
            currentPatternType = (PatternType)Enum.Parse(typeof(PatternType), cmbPatternType.SelectedItem.ToString());
            elementSize = (int)numElementSize.Value;
            patternWidth = (int)numWidth.Value;
            patternHeight = (int)numHeight.Value;

            // Генерируем узор
            var parameters = new PatternParameters(
                currentPatternType,
                primaryColor,
                secondaryColor,
                elementSize,
                patternWidth,
                patternHeight
            );

            var patternContainer = PatternGenerationService.GeneratePattern(parameters);

            // Заменяем текущий контейнер сгенерированным
            container.Palette.Clear();
            foreach (var vertex in patternContainer.Palette)
                container.Palette.Add(vertex);

            container.Pixels.Clear();
            foreach (var pixel in patternContainer.Pixels)
                container.Pixels.Add(pixel);

            canvas.Invalidate();

            MessageBox.Show($"Узор '{currentPatternType}' сгенерирован!\n" +
                           $"Размер: {patternWidth}x{patternHeight}\n" +
                           $"Элементов: {container.Pixels.Count}");
        }
    }

    private void BtnLoad_Click(object? sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog { Filter = "Hex container (*.hexc)|*.hexc|All files|*.*" };
        if (ofd.ShowDialog() == DialogResult.OK)
        {
            try
            {
                var loaded = containerService.LoadContainer(ofd.FileName);
                // replace container content
                container.Palette.Clear();
                foreach (var v in loaded.Palette)
                    container.Palette.Add(v);
                container.Pixels.Clear();
                foreach (var p in loaded.Pixels)
                    container.Pixels.Add(p);
                canvas.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки: " + ex.Message);
            }
        }
    }

    private void BtnContrast_Click(object? sender, EventArgs e)
    {
        var factor = 1.2; // например, +20% контраст
        var modifiedColors = PixelModificationService.ModifyContrast(container, factor);

        // можно просто визуализировать модифицированные пиксели поверх
        var g = canvas.CreateGraphics();
        var center = new PointF(canvas.Width / 2f, canvas.Height / 2f);

        for (int i = 0; i < container.Pixels.Count; i++)
        {
            var px = container.Pixels[i];
            var color = modifiedColors[i];
            var screen = new PointF(center.X + px.X * radius, center.Y + px.Y * radius);
            using var brush = new SolidBrush(color);
            g.FillEllipse(brush, screen.X - 4, screen.Y - 4, 8, 8);
        }

        MessageBox.Show($"Контрастность изменена (x{factor:F1})");
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        using var sfd = new SaveFileDialog { Filter = "Hex container (*.hexc)|*.hexc|All files|*.*", FileName = "container.hexc" };
        if (sfd.ShowDialog() == DialogResult.OK)
        {
            try
            {
                containerService.SaveContainer(sfd.FileName, container);
                MessageBox.Show("Сохранено");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message);
            }
        }
    }

    private void BtnEditPalette_Click(object? sender, EventArgs e)
    {
        using var dlg = new Form { Text = "Редактор палитры", Size = new Size(420, 360), StartPosition = FormStartPosition.CenterParent };
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true };
        for (int i = 0; i < container.Palette.Count; i++)
        {
            int idx = i;
            var v = container.Palette[i];
            var btn = new Button
            {
                Text = $"Vertex {i} ({v.Color.R},{v.Color.G},{v.Color.B})",
                Width = 380,
                Height = 40,
                BackColor = v.Color,
                ForeColor = Invert(v.Color)
            };
            btn.Click += (s, ea) =>
            {
                using var cd = new ColorDialog { Color = container.Palette[idx].Color };
                if (cd.ShowDialog() == DialogResult.OK)
                {
                    container.Palette[idx] = new ColorVertex(cd.Color, container.Palette[idx].Position);
                    btn.BackColor = cd.Color;
                    btn.ForeColor = Invert(cd.Color);
                    canvas.Invalidate();
                }
            };
            panel.Controls.Add(btn);
        }

        dlg.Controls.Add(panel);
        dlg.ShowDialog();
    }

    private Color Invert(Color c) => (c.R + c.G + c.B) / 3 > 128 ? Color.Black : Color.White;

    private void CreateDefaultPalette()
    {
        container.Palette.Clear();
        for (int i = 0; i < 6; i++)
        {
            double angle = Math.PI / 3.0 * i;
            var pos = new PointF((float)Math.Cos(angle), (float)Math.Sin(angle));
            var color = RandomColor();
            container.Palette.Add(new ColorVertex(color, pos));
        }
        container.Pixels.Clear();
    }

    private Color RandomColor()
    {
        return Color.FromArgb(rnd.Next(64, 256), rnd.Next(64, 256), rnd.Next(64, 256));
    }

    private void Canvas_MouseClick(object? sender, MouseEventArgs e)
    {
        var center = new PointF(canvas.Width / 2f, canvas.Height / 2f);
        float nx = (e.X - center.X) / radius;
        float ny = (e.Y - center.Y) / radius;

        if (e.Button == MouseButtons.Left)
        {
            container.AddPixel(new Pixel(nx, ny));
            canvas.Invalidate();
        }
        else if (e.Button == MouseButtons.Right)
        {
            int toRemove = -1;
            double bestDist = double.MaxValue;
            for (int i = 0; i < container.Pixels.Count; i++)
            {
                var p = container.Pixels[i];
                double dx = p.X - nx;
                double dy = p.Y - ny;
                double d = Math.Sqrt(dx * dx + dy * dy);
                if (d < bestDist)
                {
                    bestDist = d;
                    toRemove = i;
                }
            }

            if (toRemove >= 0 && bestDist < 0.05)
            {
                container.Pixels.RemoveAt(toRemove);
                canvas.Invalidate();
            }
        }
    }

    private void Canvas_Paint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        var center = new PointF(canvas.Width / 2f, canvas.Height / 2f);

        g.Clear(canvas.BackColor);

        // Используем предпросмотр или основной контейнер
        var displayContainer = isPreviewActive ? previewContainer : container;

        // draw hex outline
        var hexPts = new PointF[6];
        for (int i = 0; i < 6; i++)
        {
            double angle = Math.PI / 3.0 * i;
            float px = center.X + (float)Math.Cos(angle) * radius;
            float py = center.Y + (float)Math.Sin(angle) * radius;
            hexPts[i] = new PointF(px, py);
        }
        using var pen = new Pen(Color.Gray, 2);
        g.DrawPolygon(pen, hexPts);

        // draw vertices
        foreach (var v in displayContainer.Palette)
        {
            var screenPos = new PointF(center.X + v.Position.X * radius, center.Y + v.Position.Y * radius);
            using var brush = new SolidBrush(v.Color);
            g.FillEllipse(brush, screenPos.X - 10, screenPos.Y - 10, 20, 20);
            using var p = new Pen(Color.Black, 1);
            g.DrawEllipse(p, screenPos.X - 10, screenPos.Y - 10, 20, 20);
        }

        // draw connecting lines
        for (int i = 0; i < displayContainer.Palette.Count; i++)
        {
            var a = displayContainer.Palette[i];
            var b = displayContainer.Palette[(i + 1) % displayContainer.Palette.Count];
            var sa = new PointF(center.X + a.Position.X * radius, center.Y + a.Position.Y * radius);
            var sb = new PointF(center.X + b.Position.X * radius, center.Y + b.Position.Y * radius);
            using var p = new Pen(Color.DarkGray, 1);
            g.DrawLine(p, sa, sb);
        }

        // draw pixels using application logic
        foreach (var px in displayContainer.Pixels)
        {
            var color = PixelProcessingService.GetColorFromPixel(px, displayContainer);
            var screen = new PointF(center.X + px.X * radius, center.Y + px.Y * radius);
            using var brush = new SolidBrush(color);
            g.FillEllipse(brush, screen.X - 4, screen.Y - 4, 8, 8);
            using var p = new Pen(Color.Black, 1);
            g.DrawEllipse(p, screen.X - 4, screen.Y - 4, 8, 8);
        }

        // center (black)
        g.FillEllipse(Brushes.Black, center.X - 6, center.Y - 6, 12, 12);

        // Показываем подпись если активен предпросмотр
        if (isPreviewActive)
        {
            using var previewFont = new Font("Arial", 10, FontStyle.Bold);
            using var previewBrush = new SolidBrush(Color.Red);
            g.DrawString("ПРЕДПРОСМОТР", previewFont, previewBrush, 10, 10);
        }
    }
}