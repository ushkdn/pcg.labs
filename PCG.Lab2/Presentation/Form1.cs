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
    private readonly Button btnLoad;
    private readonly Label lblInfo;
    private readonly Random rnd = new Random();

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

        lblInfo = new Label { Text = "ЛКМ: добавить пиксель\nПКМ: удалить ближайший\nРедактировать палитру — изменить вершины", Location = new Point(620, 230), Width = 160, Height = 80 };

        canvas = new Panel
        {
            Location = new Point(20, 20),
            Size = new Size(560, 560),
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.White
        };

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
            float px = center.X + (float)Math.Cos(angle) * _radius;
            float py = center.Y + (float)Math.Sin(angle) * _radius;
            hexPts[i] = new PointF(px, py);
        }
        using var pen = new Pen(Color.Gray, 2);
        g.DrawPolygon(pen, hexPts);

        // draw vertices
        foreach (var v in container.Palette)
        {
            var screenPos = new PointF(center.X + v.Position.X * _radius, center.Y + v.Position.Y * _radius);
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
            var sa = new PointF(center.X + a.Position.X * _radius, center.Y + a.Position.Y * _radius);
            var sb = new PointF(center.X + b.Position.X * _radius, center.Y + b.Position.Y * _radius);
            using var p = new Pen(Color.DarkGray, 1);
            g.DrawLine(p, sa, sb);
        }

        // draw pixels using application logic
        foreach (var px in container.Pixels)
        {
            var color = PixelProcessingService.GetColorFromPixel(px, container);
            var screen = new PointF(center.X + px.X * _radius, center.Y + px.Y * _radius);
            using var brush = new SolidBrush(color);
            g.FillEllipse(brush, screen.X - 4, screen.Y - 4, 8, 8);
            using var p = new Pen(Color.Black, 1);
            g.DrawEllipse(p, screen.X - 4, screen.Y - 4, 8, 8);
        }

        // center (black)
        g.FillEllipse(Brushes.Black, center.X - 6, center.Y - 6, 12, 12);
    }
}
