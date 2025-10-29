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


        lblInfo = new Label { Text = "ЛКМ: добавить пиксель\nПКМ: удалить ближайший\nРедактировать палитру — изменить вершины", Location = new Point(620, 230), Width = 160, Height = 80 };

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
        foreach (var v in container.Palette)
        {
            var screenPos = new PointF(center.X + v.Position.X * radius, center.Y + v.Position.Y * radius);
            using var brush = new SolidBrush(v.Color);
            g.FillEllipse(brush, screenPos.X - 10, screenPos.Y - 10, 20, 20);
            using var p = new Pen(Color.Black, 1);
            g.DrawEllipse(p, screenPos.X - 10, screenPos.Y - 10, 20, 20);
        }

        // draw connecting lines
        for (int i = 0; i < container.Palette.Count; i++)
        {
            var a = container.Palette[i];
            var b = container.Palette[(i + 1) % container.Palette.Count];
            var sa = new PointF(center.X + a.Position.X * radius, center.Y + a.Position.Y * radius);
            var sb = new PointF(center.X + b.Position.X * radius, center.Y + b.Position.Y * radius);
            using var p = new Pen(Color.DarkGray, 1);
            g.DrawLine(p, sa, sb);
        }

        // draw pixels using application logic
        foreach (var px in container.Pixels)
        {
            var color = PixelProcessingService.GetColorFromPixel(px, container);
            var screen = new PointF(center.X + px.X * radius, center.Y + px.Y * radius);
            using var brush = new SolidBrush(color);
            g.FillEllipse(brush, screen.X - 4, screen.Y - 4, 8, 8);
            using var p = new Pen(Color.Black, 1);
            g.DrawEllipse(p, screen.X - 4, screen.Y - 4, 8, 8);
        }

        // center (black)
        g.FillEllipse(Brushes.Black, center.X - 6, center.Y - 6, 12, 12);
    }
}
