using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class DrawingGraphics : UserControl
{
    private readonly int width;
    private readonly int height;

    private List<List<Place>> travelers;

    public DrawingGraphics(int w, int h, List<List<Place>> t)
    {
        width = w;
        height = h;
        travelers = t;
    }

    private RectangleF Point(Place place)
    {
        float x = (float)(place.X - 308899.316497516 + 2) / 1000 * 3.5f;
        float y = (float)(place.Y - 5986842.71600167 + 2) / 1000 * 3.5f;
        return new RectangleF(x - 2, y - 2, 5, 5);
    }

    private PointF[] Line(float x1, float y1, float x2, float y2)
    {
        return new PointF[] { new PointF(x1, y1), new PointF(x2, y2) };
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        Graphics g = e.Graphics;
        Graphics2D g2d = new Graphics2D(g);

        g2d.SmoothingMode = SmoothingMode.AntiAlias;

        List<Color> colors = new List<Color>();
        colors.Add(Color.FromArgb(100, 0, 0, 255));
        colors.Add(Color.FromArgb(100, 0, 255, 0));
        colors.Add(Color.FromArgb(100, 255, 0, 0));

        for (int i = 0; i < travelers.Count; i++)
        {
            List<Place> path = travelers[i];

            // Drawing all points
            foreach (var place in path)
            {
                g2d.FillEllipse(new SolidBrush(colors[i]), Point(place));
            }

            // Drawing first point in different color
            g2d.FillEllipse(new SolidBrush(colors[i].Brighter()), Point(path[0]));

            // Drawing all lines
            for (int j = 0; j < path.Count - 1; j++)
            {
                float x1 = (float)(path[j].X - 308899.316497516 + 2) / 1000 * 3.5f;
                float y1 = (float)(path[j].Y - 5986842.71600167 + 2) / 1000 * 3.5f;

                float x2 = (float)(path[j + 1].X - 308899.316497516 + 2) / 1000 * 3.5f;
                float y2 = (float)(path[j + 1].Y - 5986842.71600167 + 2) / 1000 * 3.5f;

                g2d.DrawLines(new Pen(colors[i]), Line(x1, y1, x2, y2));
            }
        }
    }
}
