using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Collections.Generic;

using System.Drawing.Imaging;
using System.Linq;
using System.Diagnostics;
using cours_m2G;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Reflection;

namespace cours_m2G
{
  
    class VertexShader
    {
        protected Size screen;
        public Size Screen { get { return screen; } set { screen = value;} }
        protected double scale;
        public double Scale { get { return scale; } set { if (value > 0) scale = value; } }
        public virtual Camera up { get; set; }
        public VertexShader(Size screen, double scale)
        {
            this.screen = screen;
            this.scale = scale;
        }

        public virtual MatrixCoord3D VertexTransform(PointComponent point)
        {
            MatrixCoord3D? p1 = point.Coords;
            MatrixTransformation3D sc = new MatrixTransformationScale3D(scale, scale, scale);

            p1 *= sc;
            p1.X += screen.Width / 2;
            p1.Y = -(p1.Y - screen.Height / 2);
            return p1;
        }
    }
    class VertexShaderProjection : VertexShader
    {
        Camera cam;
        public override Camera up { get { return cam; } set { cam = ObjectCopier.Clone(value); } }
        public VertexShaderProjection(Size screen, double scale, Camera cam) : base(screen, scale)
        {
            this.screen = screen;
            this.scale = scale;
            this.cam =ObjectCopier.Clone(cam);
        }

        public override MatrixCoord3D VertexTransform(PointComponent point)
        {
            MatrixCoord3D? p1 = point.Coords;
            MatrixTransformation3D sc = new MatrixTransformationScale3D(scale, scale, scale);
            p1 = p1 * cam.LookAt;
            p1 = p1 * cam.Projection;
            if (Math.Abs( p1.X) < p1.W && Math.Abs(p1.Y) < p1.W && p1.Z < p1.W)
            {
                p1.X = p1.X / p1.W;
                p1.Y = p1.Y / p1.W;
                p1.Z = p1.W;
            }
            else
            {
                return null;
            }

            p1.X *= screen.Width / 2;
            p1.Y *= screen.Height / 2;
            p1 *= sc;
            p1.X += screen.Width / 2;
            p1.Y = -(p1.Y - screen.Height / 2);

            return p1;
        }
    }

    abstract class Rasterizator
    {
    //    protected Camera cam;
        public Camera up { set { shader.up = value; } }
        protected double scale;
        public double Scale { get { return scale; } set { if (value > 0) { scale = value; shader.Scale = value; } } }
        protected Size screen;
        public Size Screen { get { return screen; } set { screen = value; shader.Screen = value; } }
        protected VertexShader shader;
        protected RenderType type;
        public RenderType Type { get { return type; } }
        public virtual Bitmap Bmp { get { return PictureBuff.GetBitmap(); } }
        public abstract void DrawPoint(PointComponent point);
        public abstract void DrawLine(LineComponent line);
        public abstract void DrawPolygon(PolygonComponent polygon);

    }
    class RasterizatorNoCutterG : Rasterizator
    {
        protected Graphics g;
        public RasterizatorNoCutterG(double scale, Size screen)
        {
            //this.cam = cam;
            this.scale = scale;
            this.screen = screen;
            //  g = Graphics.FromImage(bmp);
            shader = new VertexShader(screen, scale);
            type = RenderType.NOCUTTER;
        }

        public RasterizatorNoCutterG(Camera cam, double scale, Size screen) : this(scale, screen)
        {
            shader = new VertexShaderProjection(screen, scale, cam);
        }

        public override void DrawPoint(PointComponent point)
        {
            Pen pen = new Pen(point.Color);
            MatrixCoord3D p1 = shader.VertexTransform(point);
            if (p1 != null)
            {
                // string s = "{" + Convert.ToString(point.X) + " " + Convert.ToString(point.Y) + " " + Convert.ToString(point.Z) + "}";
                string s = "";
                string s1 = "[ " + point.Id.Description + " ]";
                PictureBuff.SetText(p1, s, s1);
                PictureBuff.SetPoint(p1, (int)point.HitRadius, point.Color);
            }

        }

        public override void DrawLine(LineComponent line)
        {

            MatrixCoord3D p1 = shader.VertexTransform(line.Point1);
            MatrixCoord3D p2 = shader.VertexTransform(line.Point2);
            if (p1 != null && p2 != null)

                PictureBuff.SetLine((int)p1.X, (int)p1.Y, (int)p2.X, (int)p2.Y, line.Color);

        }
        public override void DrawPolygon(PolygonComponent polygon)
        {
            //MatrixCoord3D p1 = shader.VertexTransform(polygon.Points[0]);
            //MatrixCoord3D p2 = shader.VertexTransform(polygon.Points[1]);
            //MatrixCoord3D p3 = shader.VertexTransform(polygon.Points[2]);
        }
    }

    class RasterizatorNoText : RasterizatorNoCutterG
    {
        public RasterizatorNoText(Camera cam, double scale, Size screen) : base(cam, scale, screen)
        {

        }
        public RasterizatorNoText(double scale, Size screen) : base(scale, screen)
        {

        }

        public override void DrawPoint(PointComponent point)
        {

            MatrixCoord3D p1 = shader.VertexTransform(point);
            if (p1 != null)
                PictureBuff.SetPoint(p1, (int)point.HitRadius, point.Color);

        }
    }
    class RasterizatorNoPoints : RasterizatorNoCutterG
    {
        public RasterizatorNoPoints(Camera cam, double scale, Size screen) : base(cam, scale, screen)
        {

        }
        public RasterizatorNoPoints(double scale, Size screen) : base(scale, screen)
        {

        }

        public override void DrawPoint(PointComponent point)
        {

        }
    }
    class RasterizatorNoCutter : Rasterizator
    {
        public override Bitmap Bmp { get {return PictureBuff.GetBitmap(); } }
        public RasterizatorNoCutter(double scale, Size screen)
        {
           
            this.scale = scale;
            this.screen = screen;
            shader = new VertexShader(screen, scale);
            type = RenderType.ZBUFF;
        }

        public RasterizatorNoCutter(Camera cam, double scale, Size screen) : this(scale, screen)
        {
            shader = new VertexShaderProjection(screen, scale, cam);
        }

        public override void DrawPoint(PointComponent point)
        {
          
        }

        public override void DrawLine(LineComponent line)
        {
            MatrixCoord3D p1 = shader.VertexTransform(line.Point1);
            MatrixCoord3D p2 = shader.VertexTransform(line.Point2);

            if (p1 != null && p2 != null)
                if (p1 != Double.NaN && p2 != Double.NaN)
                {
                   drawLine(new List<PointComponent> { new PointComponent(p1), new PointComponent(p2) }, line.Color);
                }

        }
        private void drawLine(List<PointComponent> vertices, Color color)
        {
            List<PointComponent> pp = Line.GetPoints(vertices[0], vertices[1]);
            if(pp!=null)
            foreach (var p in pp)
            {
                if (Scene.cancelTokenSource.IsCancellationRequested)
                    return;
                drawPoint1(p, color);
            }
        }
        void drawPoint1(PointComponent point, Color color)
        {
            var p2D = point;

            if (color != Color.White)
                PictureBuff.SetPixel((int)p2D.X, (int)p2D.Y, color.ToArgb());
        }
        public override void DrawPolygon(PolygonComponent polygon)
        {
            
        }
    }
    class RasterizatorCutter : Rasterizator
    {
        ZBuffer zBuffer;
        public override Bitmap Bmp { get { zBuffer.Down(); return PictureBuff.GetBitmap(); } }
        public RasterizatorCutter(double scale, Size screen)
        {
            this.scale = scale;
            zBuffer = new ZBuffer(screen);
            this.screen = screen;
            type = RenderType.ZBUFF;
            Console.WriteLine("Inter");
        }
        public RasterizatorCutter(Camera cam, double scale, Size screen) : this(scale, screen)
        {
            shader = new VertexShaderProjection(screen, scale, cam);
        }
        public override void DrawPoint(PointComponent point)
        {
        }

        public override void DrawLine(LineComponent line)
        {
            MatrixCoord3D p1 = shader.VertexTransform(line.Point1);
            MatrixCoord3D p2 = shader.VertexTransform(line.Point2);

            if (p1 != null && p2 != null)
                if (p1 != Double.NaN && p2 != Double.NaN)
                {
                    if (zBuffer[p1.X, p1.Y] <= p1.Z - 1 && zBuffer[p2.X, p2.Y] <= p2.Z - 1)
                        return;
                    drawLine(new List<PointComponent> { new PointComponent(p1), new PointComponent(p2) }, line.Color);
                }
        }

        public override void DrawPolygon(PolygonComponent polygon)
        {
            MatrixCoord3D p1 = shader.VertexTransform(polygon.Points[0]);
            MatrixCoord3D p2 = shader.VertexTransform(polygon.Points[1]);
            MatrixCoord3D p3 = shader.VertexTransform(polygon.Points[2]);
            double cos = Math.Abs(MatrixCoord3D.scalar(polygon.Normal, shader.up.Direction));
            Color c = Color.White;
            try
            {
                c = Color.FromArgb(255, Convert.ToInt32(polygon.ColorF.R * cos), Convert.ToInt32(polygon.ColorF.G * cos), Convert.ToInt32(polygon.ColorF.B * cos));
            }
            catch { return; }
            if (p1 != null && p2 != null && p3 != null)
         //       if (p1 != Double.NaN && p2 != Double.NaN && p3 != Double.NaN)
                    drawTriangleFill(new List<PointComponent> { new PointComponent(p1), new PointComponent(p2), new PointComponent(p3) }, c);
        }

        private void drawTriangleFill(List<PointComponent> vertices, Color color)
        {
            var points = new List<PointComponent> { vertices[0], vertices[1], vertices[2] };



            foreach (var p in Fill.FillTriangle(points))
            {
                if (Scene.cancelTokenSource.IsCancellationRequested)
                    return;
                drawPoint(p, color);
            }
        }

        private void drawLine(List<PointComponent> vertices, Color color)
        {
            List<PointComponent> pp = Line.GetPoints(vertices[0], vertices[1]);
            if(pp!=null)
            foreach (var p in pp)
            {
                if (Scene.cancelTokenSource.IsCancellationRequested)
                    return;
                drawPoint1(p, color);
            }
               
        }

        void drawPoint(PointComponent point, Color color)
        {
            var p2D = point;

            if (zBuffer[point.X, point.Y] <= point.Z)
                return;

            zBuffer[point.X, point.Y] = point.Z;
            if (color != Color.White)
                PictureBuff.SetPixel((int)p2D.X, (int)p2D.Y, color.ToArgb());
        }
        void drawPoint1(PointComponent point, Color color)
        {
            var p2D = point;

            if (zBuffer[point.X, point.Y] <= point.Z-1)
                return;

            zBuffer[point.X, point.Y] = point.Z;
            if (color != Color.White)
                PictureBuff.SetPixel((int)p2D.X, (int)p2D.Y, color.ToArgb());
        }
    }

    class R1
    {
        ZBuffer zBuffer;
        public  Bitmap Bmp { get { zBuffer.Down(); return PictureBuff.GetBitmap(); } }
        Bitmap tex;
        public Camera up { set { shader.up = value; } }
        protected double scale;
        public double Scale { get { return scale; } set { if (value > 0) { scale = value; shader.Scale = value; } } }
        protected Size screen;
        protected VertexShader shader;
        protected RenderType type;
        public RenderType Type { get { return type; } }

        public R1(Camera cam, double scale, Size screen)
        {
            this.scale = scale;
            zBuffer = new ZBuffer(screen);
            this.screen = screen;
            type = RenderType.ZBUFF;
            tex = new Bitmap(Image.FromFile(@"D:\2.png"));
            shader = new VertexShaderProjection(screen, scale, cam);
        }

        public void drawTriangleFill(PointComponent[] vertices, PointComponent[] texture)
        {
            MatrixCoord3D p1 = shader.VertexTransform(vertices[0]);
            MatrixCoord3D p2 = shader.VertexTransform(vertices[1]);
            MatrixCoord3D p3 = shader.VertexTransform(vertices[2]);

         //   FillTexture.FillTriangle(new List<Point3DTexture> { new Point3DTexture(new PointComponent(p1), texture[0]), new Point3DTexture(new PointComponent(p2), texture[1]), new Point3DTexture(new PointComponent(p3), texture[2]) })
            foreach (var p in ShadeBackgroundPixel(new Point3DTexture(new PointComponent(p1), texture[0]), new Point3DTexture(new PointComponent(p2), texture[1]), new Point3DTexture(new PointComponent(p3), texture[2])))
                drawPoint(p);
        }

        public void drawLine(List<PointComponent> vertices, Color color)
        {
            //List<PointComponent> pp = Line.GetPoints(vertices[0], vertices[1]);
            // foreach (var p in pp)
            //      drawPoint(p, color);
        }


        void drawPoint(Point3DTexture point)
        {
            var p2D = point;

            if (zBuffer[point.X, point.Y] <= point.Z)
                return;

            zBuffer[point.X, point.Y] = point.Z;
            PictureBuff.SetPixel((int)p2D.X, (int)p2D.Y, tex.GetPixel(Ut.F(p2D.U * tex.Size.Width), Ut.F(p2D.V * tex.Size.Height)).ToArgb());
        
        }
        public IEnumerable<Point3DTexture> ShadeBackgroundPixel(Point3DTexture p1, Point3DTexture p2, Point3DTexture p3)
        {

            //  List<PointComponent> points = new List<PointComponent>();

            double x_min, x_max, y_min, y_max;
            x_min = Math.Min(p1.X, Math.Min(p2.X, p3.X));
            y_min = Math.Min(p1.Y, Math.Min(p2.Y, p3.Y));
            x_max = Math.Max(p1.X, Math.Max(p2.X, p3.X));
            y_max = Math.Max(p1.Y, Math.Max(p2.Y, p3.Y));
            //if (x_min < 0)
            //    x_min = 0;
            //if (x_max > screen.Width)
            //    x_max = screen.Width;
            //if (y_min < 0)
            //    y_min = 0;
            //if(y_max > screen.Height)
            //    y_max = screen.Height;
            double det = ((p2.Y - p3.Y) * (p1.X - p3.X) + (p3.X - p2.X) * (p1.Y - p3.Y));

            double l1, l2, l3;
            double dy23 = (p2.Y - p3.Y), dy31 = (p3.Y - p1.Y), dx32 = (p3.X - p2.X), dx13 = (p1.X - p3.X);
            int k = 0;
            for (double sx = x_min - k; sx <= x_max + k; sx += 0.5)
                for (double sy = y_min - k; sy <= y_max + k; sy += 0.5)
                {
                    l1 = (dy23 * ((sx) - p3.X) + dx32 * ((sy) - p3.Y)) / det;
                    l2 = (dy31 * ((sx) - p3.X) + dx13 * ((sy) - p3.Y)) / det;
                    l3 = 1 - l1 - l2;
                    if (l1 >= 0 && l1 <= 1 && l2 >= 0 && l2 <= 1 && l3 >= 0 && l3 <= 1)
                    {
                        double z = l1 * p1.Z + l2 * p2.Z + l3 * p3.Z;
                        double vz = l1 * p1.V/z + l2 * p2.V/z + l3 * p3.V/z ;
                        double uz =l1* p1.U / z + l2 * p2.U / z + l3 * p3.U / z; 
                        double zz = l1 * 1/p1.Z + l2 * 1/p2.Z + l3 * 1/p3.Z;

                        yield return new Point3DTexture(new PointComponent(sx, sy, z), new PointComponent(uz/zz, vz/zz, 0));
                        //points.Add(new PointComponent(sx, sy, z));
                    }
                }
            //return points;
        }
    }

    class ZBuffer
    {
        // minVal is at the top of the stack
        double[,] zBufferMap;
        Size w;

        public ZBuffer(Size windowSize)
        {
            w = windowSize;
            zBufferMap = new double[windowSize.Width, windowSize.Height];
            for (int i = 0; i < windowSize.Width * windowSize.Height; i++)
                zBufferMap[i % windowSize.Width, Ut.F(i / windowSize.Width)] = double.MaxValue;
        }
        public void Down()
        {
            for (int i = 0; i < w.Width * w.Height; i++)
                zBufferMap[i % w.Width, Ut.F(i / w.Width)] = double.MaxValue;

        }
        // get: boolean if z is higher or equal to point (=> true = can draw)
        // set: z at point
        public double this[double x, double y]
        {
            get
            {
                x = Ut.F(x);
                y = Ut.F(y);
                if (x > w.Width - 1 || x < 0 || y < 0 || y > w.Height - 1) return double.MinValue;
                return zBufferMap[Ut.F(x), Ut.F(y)];
            }
            set
            {
                zBufferMap[Ut.F(x), Ut.F(y)] = (double)value;
            }
        }

        //public override string ToString()
        //{
        //    var sb = new StringBuilder();

        //    for (int i = 0; i < w.Width; i++)
        //    {
        //        for (int j = 0; j < w.Height; j++)
        //        {
        //            if (zBufferMap[i, j] == double.MinValue)
        //                sb.Append(",");
        //            else
        //                sb.Append(zBufferMap[i, j]);
        //            sb.Append(" ");
        //        }
        //        sb.AppendLine();
        //    }

        //    return sb.ToString();
        //}
    }

    static class Line
    {
 
        public static List<PointComponent> GetPoints(PointComponent p1, PointComponent p2)
        {
            double dE = 1;

            List<PointComponent> points = new List<PointComponent>() { p1 };

            double dx = Math.Abs(p2.X - p1.X);
            double dy = Math.Abs(p2.Y - p1.Y);
            double dz = Math.Abs(p2.Z - p1.Z);

            double xs = p2.X > p1.X ? 1 : -1;
            double ys = p2.Y > p1.Y ? 1 : -1;
            double zs = p2.Z > p1.Z ? 1 : -1;

            double
                d1,
                d2;
            double
                x1 = p1.X,
                y1 = p1.Y,
                z1 = p1.Z;


            if (dx >= dy && dx >= dz)
            {
                d1 = 2 * dy - dx;
                d2 = 2 * dz - dx;
                while (Math.Abs(Ut.F(x1) - Ut.F(p2.X)) > dE)
                {
                    if (Scene.cancelTokenSource.IsCancellationRequested)
                        return null;
                    x1 += xs;
                    if (d1 >= 0)
                    {
                        y1 += ys;
                        d1 -= 2 * dx;
                    }
                    if (d2 >= 0)
                    {
                        z1 += zs;
                        d2 -= 2 * dx;
                    }
                    d1 += 2 * dy;
                    d2 += 2 * dz;
                    points.Add(new PointComponent(x1, y1, z1));
                }
            }

            else if (dy >= dx && dy >= dz)
            {
                d1 = 2 * dx - dy;
                d2 = 2 * dz - dy;
                while (Math.Abs(Ut.F(y1) - Ut.F(p2.Y)) > dE)
                {
                    if (Scene.cancelTokenSource.IsCancellationRequested)
                        return null;
                    y1 += ys;
                    if (d1 >= 0)
                    {
                        x1 += xs;
                        d1 -= 2 * dy;
                    }
                    if (d2 >= 0)
                    {
                        z1 += zs;
                        d2 -= 2 * dy;
                    }
                    d1 += 2 * dx;
                    d2 += 2 * dz;
                    points.Add(new PointComponent(x1, y1, z1));
                }
            }

            else
            {
                d1 = 2 * dy - dz;
                d2 = 2 * dz - dz;
                while (Math.Abs(Ut.F(z1) - Ut.F(p2.Z)) > dE)
                {
                    if (Scene.cancelTokenSource.IsCancellationRequested)
                        return null;
                    z1 += zs;
                    if (d1 >= 0)
                    {
                        y1 += ys;
                        d1 -= 2 * dz;
                    }
                    if (d2 >= 0)
                    {
                        x1 += xs;
                        d2 -= 2 * dz;
                    }
                    d1 += 2 * dy;
                    d2 += 2 * dx;
                    points.Add(new PointComponent(x1, y1, z1));
                }
            }

            points.Add(p2);

            return points;
        }

        public static List<PointComponent> OutlineTriangle(List<PointComponent> vertices)
        {
            var points = new List<PointComponent>();

            var p1 = vertices[0];
            var p2 = vertices[1];
            var p3 = vertices[2];

            if (p1.Y > p2.Y)
            {
                var temp = p2;
                p2 = p1;
                p1 = temp;
            }

            if (p2.Y > p3.Y)
            {
                var temp = p2;
                p2 = p3;
                p3 = temp;
            }

            if (p1.Y > p2.Y)
            {
                var temp = p2;
                p2 = p1;
                p1 = temp;
            }

            double dP1P2, dP1P3;

            if (p2.Y - p1.Y > 0)
                dP1P2 = (p2.X - p1.X) / (p2.Y - p1.Y);
            else
                dP1P2 = 0;

            if (p3.Y - p1.Y > 0)
                dP1P3 = (p3.X - p1.X) / (p3.Y - p1.Y);
            else
                dP1P3 = 0;

 
            if (dP1P2 > dP1P3)
            {
                for (var y = (int)p1.Y; y <= (int)p3.Y; y++)
                {
                    if (Scene.cancelTokenSource.IsCancellationRequested)
                        return null;
                    if (y < p2.Y)
                    {
                        points.AddRange(ProcessScanLine(y, p1, p3, p1, p2));
                    }
                    else
                    {
                        points.AddRange(ProcessScanLine(y, p1, p3, p2, p3));
                    }
                }
            }

            else
            {
                for (var y = (int)p1.Y; y <= (int)p3.Y; y++)
                {
                    if (Scene.cancelTokenSource.IsCancellationRequested)
                        return null;
                    if (y < p2.Y)
                    {
                        points.AddRange(ProcessScanLine(y, p1, p2, p1, p3));
                    }
                    else
                    {
                        points.AddRange(ProcessScanLine(y, p2, p3, p1, p3));
                    }
                }
            }

            return points;
        }

        static double Clamp(double value, double min = 0, double max = 1)
        {
            return Math.Max(min, Math.Min(value, max));
        }

        static double Interpolate(double min, double max, double gradient)
        {
            return min + (max - min) * Clamp(gradient);
        }

        static List<PointComponent> ProcessScanLine(int y, PointComponent pa, PointComponent pb, PointComponent pc, PointComponent pd)
        {
            var points = new List<PointComponent>();

            var gradient1 = pa.Y != pb.Y ? (y - pa.Y) / (pb.Y - pa.Y) : 1;
            var gradient2 = pc.Y != pd.Y ? (y - pc.Y) / (pd.Y - pc.Y) : 1;

            int sx = (int)Interpolate(pa.X, pb.X, gradient1);
            int ex = (int)Interpolate(pc.X, pd.X, gradient2);

            double z1 = Interpolate(pa.Z, pb.Z, gradient1);
            double z2 = Interpolate(pc.Z, pd.Z, gradient2);

            var x = sx;

            float gradient = (x - sx) / (float)(ex - sx);

            var z = Interpolate(z1, z2, gradient);
            points.Add(new PointComponent(x, y, z));

            for (x = sx + 1; x < ex; x++)
            {
                if (Scene.cancelTokenSource.IsCancellationRequested)
                    return null;
                gradient = (x - sx) / (float)(ex - sx);

                z = Interpolate(z1, z2, gradient);
            }

            points.Add(new PointComponent(x, y, z));

            return points;
        }
    }

    static class Fill
    
    {
        public static IEnumerable<PointComponent> FillTriangle(List<PointComponent> vertices)
        {
            vertices.Sort((x, y) => Ut.F(x.Y - y.Y));

            var p1 = vertices[0];
            var p2 = vertices[1];
            var p3 = vertices[2];

            double dP1P2, dP1P3;

            if (p2.Y - p1.Y > 0)
                dP1P2 = (p2.X - p1.X) / (p2.Y - p1.Y);
            else
                dP1P2 = 0;

            if (p3.Y - p1.Y > 0)
                dP1P3 = (p3.X - p1.X) / (p3.Y - p1.Y);
            else
                dP1P3 = 0;

            if (dP1P2 > dP1P3)
            {
                for (var y = (int)p1.Y; y <= (int)p3.Y; y++)
                {
                    if (y < p2.Y)
                        foreach (var p in ProcessScanLine(y, p1, p3, p1, p2))
                            yield return p;
                    else
                        foreach (var p in ProcessScanLine(y, p1, p3, p2, p3))
                            yield return p;
                }
            }
            else
            {
                for (var y = (int)p1.Y; y <= (int)p3.Y; y++)
                {
                    if (y < p2.Y)
                        foreach (var p in ProcessScanLine(y, p1, p2, p1, p3))
                            yield return p;
                    else
                        foreach (var p in ProcessScanLine(y, p2, p3, p1, p3))
                            yield return p;
                }
            }
        }

        static double Clamp(double value, double min = 0, double max = 1)
        {
            return Math.Max(min, Math.Min(value, max));
        }

        static double Interpolate(double min, double max, double gradient)
        {
            return min + (max - min) * Clamp(gradient);
        }

        static IEnumerable<PointComponent> ProcessScanLine(int y, PointComponent pa, PointComponent pb, PointComponent pc, PointComponent pd)
        {
            var gradient1 = pa.Y != pb.Y ? (y - pa.Y) / (pb.Y - pa.Y) : 1;
            var gradient2 = pc.Y != pd.Y ? (y - pc.Y) / (pd.Y - pc.Y) : 1;

            int sx = (int)Interpolate(pa.X, pb.X, gradient1);
            int ex = (int)Interpolate(pc.X, pd.X, gradient2);

            double z1 = Interpolate(pa.Z, pb.Z, gradient1);
            double z2 = Interpolate(pc.Z, pd.Z, gradient2);

            for (var x = sx; x < ex; x++)
            {
                float gradient = (x - sx) / (float)(ex - sx);

                var z = Interpolate(z1, z2, gradient);
                yield return new PointComponent(x, y, z);
            }

            for (var x = ex; x < sx; x++)
            {
                float gradient = (x - sx) / (float)(ex - sx);

                var z = Interpolate(z1, z2, gradient);
                yield return new PointComponent(x, y, z);
            }
        }
    }

    static class FillTexture
{
        public static IEnumerable<Point3DTexture> FillTriangle(List<Point3DTexture> vertices)
        {
            vertices.Sort((x, y) => Ut.F(x.Y - y.Y));

            var p1 = vertices[0];
            var p2 = vertices[1];
            var p3 = vertices[2];

            double dP1P2, dP1P3;

            if (p2.Y - p1.Y > 0)
                dP1P2 = (p2.X - p1.X) / (p2.Y - p1.Y);
            else
                dP1P2 = 0;

            if (p3.Y - p1.Y > 0)
                dP1P3 = (p3.X - p1.X) / (p3.Y - p1.Y);
            else
                dP1P3 = 0;

            if (dP1P2 > dP1P3)
            {
                for (var y = (int)p1.Y; y <= (int)p3.Y; y++)
                {
                    if (y < p2.Y)
                        foreach (var p in ProcessScanLine(y, p1, p3, p1, p2))
                            yield return p;
                    else
                        foreach (var p in ProcessScanLine(y, p1, p3, p2, p3))
                            yield return p;
                }
            }
            else
            {
                for (var y = (int)p1.Y; y <= (int)p3.Y; y++)
                {
                    if (y < p2.Y)
                        foreach (var p in ProcessScanLine(y, p1, p2, p1, p3))
                            yield return p;
                    else
                        foreach (var p in ProcessScanLine(y, p2, p3, p1, p3))
                            yield return p;
                }
            }
        }

        static double Clamp(double value, double min = 0, double max = 1)
        {
            return Math.Max(min, Math.Min(value, max));
        }

        static double Interpolate(double min, double max, double gradient)
        {
            return min + (max - min) * Clamp(gradient);
        }

        static IEnumerable<Point3DTexture> ProcessScanLine(int y, Point3DTexture pa, Point3DTexture pb, Point3DTexture pc, Point3DTexture pd)
        {
            var gradient1 = pa.Y != pb.Y ? (y - pa.Y) / (pb.Y - pa.Y) : 1;
            var gradient2 = pc.Y != pd.Y ? (y - pc.Y) / (pd.Y - pc.Y) : 1;

            int sx = (int)Interpolate(pa.X, pb.X, gradient1);
            int ex = (int)Interpolate(pc.X, pd.X, gradient2);

            double z1 = Interpolate(pa.Z, pb.Z, gradient1);
            double z2 = Interpolate(pc.Z, pd.Z, gradient2);

            double u1 = Interpolate(pa.U, pb.U, gradient1);
            double u2 = Interpolate(pc.U, pd.U, gradient2);

            double v1 = Interpolate(pa.V, pb.V, gradient1);
            double v2 = Interpolate(pc.V, pd.V, gradient2);

            for (var x = sx; x < ex; x++)
            {
                float gradient = (x - sx) / (float)(ex - sx);

                var z = Interpolate(z1, z2, gradient);
                var u = Interpolate(u1, u2, gradient);
                var v = Interpolate(v1, v2, gradient);
                yield return new Point3DTexture(new PointComponent(x, y, z), new PointComponent(u, v, 0));
            }

            for (var x = ex; x < sx; x++)
            {
                float gradient = (x - sx) / (float)(ex - sx);

                var z = Interpolate(z1, z2, gradient);
                var u = Interpolate(u1, u2, gradient);
                var v = Interpolate(v1, v2, gradient);
                yield return new Point3DTexture(new PointComponent(x, y, z), new PointComponent(u, v, 0));
            }
        }
    }

    class Point3DTexture
    {
        public double X { get { return coords.X; } }
        public double Y { get { return coords.Y; } }
        public double Z { get { return coords.Z; } }
        public double U { get { return coordsTexture.X; } }
        public double V { get { return coordsTexture.Y; } }
        private MatrixCoord3D coordsTexture;
        private MatrixCoord3D coords;
        public Point3DTexture(PointComponent p, PointComponent texture)
        {
            coords = p.Coords;
            coordsTexture = texture.Coords;
        }
    }

    static class Ut
    {
        public static int F(double a)
        {
            return (int)Math.Floor(a);
        }
    }

    public static class IEnumerableExtensions
    {
        public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> self)
            => self.Select((item, index) => (item, index));
    }

    public static class BitmapExtension
    {
        public static void SetPixelFast(this Bitmap bmp, int x, int y, Color color)
        {
            var newValues = new byte[] { color.B, color.G, color.R, 255 };

            BitmapData data = bmp.LockBits(
                new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb
                );

            if (
                data.Stride * y + 4 * x < data.Stride * data.Height
                && data.Stride * y + 4 * x >= 0
                && x * 4 < data.Stride
                && y < data.Height
                && x > 0
                )
                unsafe
                {
                    byte* ptr = (byte*)data.Scan0;

                    for (int i = 0; i < 4; i++)
                        ptr[data.Stride * y + 4 * x + i] = newValues[i];
                }

            bmp.UnlockBits(data);
        }
    }



    class RasterizatorCutterBarri : Rasterizator
    {
        ZBuffer zBuffer;
        public override Bitmap Bmp { get { zBuffer.Down(); return PictureBuff.GetBitmap(); } }
        public RasterizatorCutterBarri(double scale, Size screen)
        {
            this.scale = scale;
            zBuffer = new ZBuffer(screen);
            this.screen = screen;
            type = RenderType.ZBUFF;
            Console.WriteLine("Barri");
        }
        public RasterizatorCutterBarri(Camera cam, double scale, Size screen) : this(scale, screen)
        {
            shader = new VertexShaderProjection(screen, scale, cam);
        }
        public override void DrawPoint(PointComponent point)
        {
        }

        public override void DrawLine(LineComponent line)
        {

            MatrixCoord3D p1 = shader.VertexTransform(line.Point1);
            MatrixCoord3D p2 = shader.VertexTransform(line.Point2);

            if (p1 != null && p2 != null)
                if (p1 != Double.NaN && p2 != Double.NaN)
                {
                    if (zBuffer[p1.X, p1.Y] <= p1.Z - 1 && zBuffer[p2.X, p2.Y] <= p2.Z - 1)
                        return;
                   drawLine(new List<PointComponent> { new PointComponent(p1), new PointComponent(p2) }, line.Color);
                }
        }

        public override void DrawPolygon(PolygonComponent polygon)
        {
            MatrixCoord3D p1 = shader.VertexTransform(polygon.Points[0]);
            MatrixCoord3D p2 = shader.VertexTransform(polygon.Points[1]);
            MatrixCoord3D p3 = shader.VertexTransform(polygon.Points[2]);
            double cos = Math.Abs(MatrixCoord3D.scalar(polygon.Normal, shader.up.Direction));
            Color c = Color.White;
            try
            {
                c = Color.FromArgb(255, Convert.ToInt32(polygon.ColorF.R * cos), Convert.ToInt32(polygon.ColorF.G * cos), Convert.ToInt32(polygon.ColorF.B * cos));
            }
            catch { return; }
            if (p1 != null && p2 != null && p3 != null)
            //    if (p1 != Double.NaN && p2 != Double.NaN && p3 != Double.NaN)
            {

                foreach (var p in ShadeBackgroundPixel(new PointComponent(p1), new PointComponent(p2), new PointComponent(p3)))
                {
                    if (p == null)
                        return;
                    drawPoint(p, c);
                }
            }
        }

    

        private void drawLine(List<PointComponent> vertices, Color color)
        {
            List<PointComponent> pp = Line.GetPoints(vertices[0], vertices[1]);
            if(pp!=null)
            foreach (var p in pp)
                drawPoint1(p, color);
        }

        void drawPoint(PointComponent point, Color color)
        {
            var p2D = point;

            if (zBuffer[point.X, point.Y] <= point.Z)
                return;

            zBuffer[point.X, point.Y] = point.Z;
            if (color != Color.White)
                PictureBuff.SetPixel((int)p2D.X, (int)p2D.Y, color.ToArgb());
        }

        void drawPoint1(PointComponent point, Color color)
        {
            var p2D = point;

            if (zBuffer[point.X, point.Y] <= point.Z - 1)
                return;

            zBuffer[point.X, point.Y] = point.Z;
            if (color != Color.White)
                PictureBuff.SetPixel((int)p2D.X, (int)p2D.Y, color.ToArgb());
        }



        public IEnumerable<PointComponent> ShadeBackgroundPixel(PointComponent p1, PointComponent p2, PointComponent p3)
        {

              List<PointComponent> points = new List<PointComponent>();

            double x_min, x_max, y_min, y_max;
            x_min = Math.Min(p1.X, Math.Min(p2.X, p3.X));
            y_min = Math.Min(p1.Y, Math.Min(p2.Y, p3.Y));
            x_max = Math.Max(p1.X, Math.Max(p2.X, p3.X));
            y_max = Math.Max(p1.Y, Math.Max(p2.Y, p3.Y));


            double det = ((p2.Y - p3.Y) * (p1.X - p3.X) + (p3.X - p2.X) * (p1.Y - p3.Y));

            double l1, l2, l3;
            double dy23 = (p2.Y - p3.Y), dy31 = (p3.Y - p1.Y), dx32 = (p3.X - p2.X), dx13 = (p1.X - p3.X);
            int k = 1;
            bool flag_in = false, flag_out = false;
            double direction = 1;
            double start = x_min-1, stop = x_max+1;
            for (double sy = y_min - k; sy <= y_max + k; sy += 0.25)
            {
                if (Scene.cancelTokenSource.IsCancellationRequested)
                    yield break;
                flag_out = false;
                double speed = 0.25;
                if (direction == 1)
                    for (double sx = start; sx <= stop; sx += speed)
                    {
                   
                        l1 = (dy23 * ((sx) - p3.X) + dx32 * ((sy) - p3.Y)) / det;
                        l2 = (dy31 * ((sx) - p3.X) + dx13 * ((sy) - p3.Y)) / det;
                        l3 = 1 - l1 - l2;
                        if ((l1 >= 0 && l1 <= 1) && (l2 >= 0 && l2 <= 1) && (l3 >= 0 && l3 <= 1))
                        {
                            flag_in = true;
                            double z = l1 * p1.Z + l2 * p2.Z + l3 * p3.Z;
                            yield return new PointComponent(sx, sy, z);
                           // points.Add(new PointComponent(sx, sy, z));
                        }
                        else
                        {
                            if (flag_in)
                            {
                                start = sx+1;
                                stop = x_min - 1;
                                flag_in = false;
                                flag_out = true;
                                direction = -1;
                            }
                        }
                        if (flag_out)
                            break;
                    }
                else
                    for (double sx = start; sx >= stop; sx -= speed)
                    {

                        l1 = (dy23 * ((sx) - p3.X) + dx32 * ((sy) - p3.Y)) / det;
                        l2 = (dy31 * ((sx) - p3.X) + dx13 * ((sy) - p3.Y)) / det;
                        l3 = 1 - l1 - l2;
                        if ((l1 >= 0 && l1 <= 1) && (l2 >= 0 && l2 <= 1) && (l3 >= 0 && l3 <= 1))
                        {
                            flag_in = true;
                            double z = l1 * p1.Z + l2 * p2.Z + l3 * p3.Z;
                            yield return new PointComponent(sx, sy, z);
                            //points.Add(new PointComponent(sx, sy, z));
                        }
                        else
                        {
                            if (flag_in)
                            {
                                start = sx-1;
                                stop = x_max + 1;
                                flag_in = false;
                                flag_out = true;
                                direction = 1;
                            }
                        }
                        if (flag_out)
                            break;
                    }
            }
            
          //  return points;
        }




        struct Point2D
        {
           public Point2D(double x, double y)
            {
                this.x = x;
                this.y = y;
            }
            public double x, y;
        };
    }
   
}