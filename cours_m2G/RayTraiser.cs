using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace cours_m2G
{
    class RayTraiser
    {
        Size screen;
        Camera cam;
        double scale;
        public Size Screen { get { return screen; } set { screen = value; } }
        public double Scale { get { return scale; } set { if (value > 0) scale = value; } }
        public int NumberofThreads { get; set; } = Environment.ProcessorCount;
       
        public RayTraiser(Size screen, Camera cam, double scale)
        {
            this.screen = screen;
            this.cam = cam;
            this.scale = scale;
        }

        public virtual void RayTrasing(IModel model)
        {
            Tuple<MatrixCoord3D, double> sphere = FoundCenter(model.Points);
            MatrixCoord3D CamPosition = cam.Position.Coords;
            MatrixTransformation3D RotateMatrix = cam.RotateMatrix.InversedMatrix();
            double aspect = screen.Width / (double)screen.Height;
            double field = Math.Tan(cam.Fovy / 2 * Math.PI / 180.0f);
            for (int x = 0; x < screen.Width; x++)
            {
                for (int y = 0; y < screen.Height; y++)
                {
                    if (Scene.cancelTokenSource.IsCancellationRequested)
                        return;
                    MatrixCoord3D D = CanvasToVieport(x, y, aspect, field) * RotateMatrix;// new MatrixCoord3D(cam.RotateMatrix.Coeff[0,2], cam.RotateMatrix.Coeff[1, 2], cam.RotateMatrix.Coeff[2, 2]);
                    D.Normalise();
                    Color c = Color.White;
                    if (RaySphereIntersection(CamPosition, D, sphere.Item1, sphere.Item2) != double.MaxValue)
                    {
                        c = RayT(model, D, CamPosition);
                    }
                    PictureBuff.SetPixel(x, y, c.ToArgb());
                }
            }
            //Parallel.For(0, screen.Width, x =>
            //{
            //    for (int y = 0; y < screen.Height; y++)
            //    {
            //        MatrixCoord3D D = CanvasToVieport(x, y, aspect, field) * RotateMatrix;// new MatrixCoord3D(cam.RotateMatrix.Coeff[0,2], cam.RotateMatrix.Coeff[1, 2], cam.RotateMatrix.Coeff[2, 2]);
            //        D.Normalise();
            //        Color c = Color.White;
            //        if (RaySphereIntersection(CamPosition, D, sphere.Item1, sphere.Item2) != double.MaxValue)
            //        {
            //            c = RayT(model, D, CamPosition);
            //        }
            //        PictureBuff.SetPixel(x, y, c.ToArgb());
            //    }
            //});
        }

        protected Color RayT(IModel model, MatrixCoord3D D, MatrixCoord3D position)
        {

            PolygonComponent closest = null;
            double closest_t = double.MaxValue;
            //  Parallel.ForEach<PolygonComponent>(model.Polygons, p=> { 
            foreach (PolygonComponent p in model.Polygons)
            {
                if(p!=null)
                //   if (MatrixCoord3D.scalar(p.Normal, cam.Direction) > 0)
                {
                    MatrixCoord3D tt = GetTimeAndUvCoord(position, D, p.Points[0].Coords, p.Points[1].Coords, p.Points[2].Coords);
                    if (tt != null)
                    {
                        if (tt.X < closest_t && tt.X > 1)
                        {
                            closest_t = tt.X;
                            closest = p;
                        }
                    }
                }
            }

            if (closest == null)
                return Color.White;
        
             double cos = Math.Abs(MatrixCoord3D.scalar(closest.Normal, cam.Direction));
             Color c = Color.FromArgb(255, Convert.ToInt32(closest.ColorF.R * cos), Convert.ToInt32(closest.ColorF.G * cos), Convert.ToInt32(closest.ColorF.B * cos));
       
            return c;
        }
        protected Tuple<MatrixCoord3D, double> FoundCenter(Container<PointComponent> points)
        {
            double minX, maxX, minY, maxY, minZ, maxZ;
            PointComponent poi = points.GetFirstElem();
            minX = poi.X;
            maxX = poi.X;
            minY = poi.Y;
            maxY = poi.Y;
            minZ = poi.Z;
            maxZ = poi.Z;

            foreach (PointComponent p in points)
            {
                if (p.X < minX)
                    minX = p.X;
                if (p.X > maxX)
                    maxX = p.X;
                if (p.Y < minY)
                    minY = p.Y;
                if (p.Y > maxY)
                    maxY = p.Y;
                if (p.Z < minZ)
                    minZ = p.Z;
                if (p.Z > maxZ)
                    maxZ = p.Z;
            }
            MatrixCoord3D position = new MatrixCoord3D((minX + maxX) / 2f, (minY + maxY) / 2f, (minZ + maxZ) / 2f);
            double r = 0;
            foreach (PointComponent p in points)
            {
                LineComponent minx = new LineComponent(new PointComponent(position), p);
                r = Math.Max(minx.Len(), r);
            }

            return new Tuple<MatrixCoord3D, double>(position, r);
        }
        protected double RaySphereIntersection(MatrixCoord3D rayOrigin, MatrixCoord3D rayDirection, MatrixCoord3D spos, double r)
        {
            double t = Double.MaxValue;
            //a == 1; // because rdir must be normalized
            MatrixCoord3D k = rayOrigin - spos;
            double b = MatrixCoord3D.scalar(k, rayDirection);
            double c = MatrixCoord3D.scalar(k, k) - r * r;
            double d = b * b - c;
            if (d >= 0)
            {
                double sqrtfd = Math.Sqrt(d);
                // t, a == 1
                double t1 = -b + sqrtfd;
                double t2 = -b - sqrtfd;
                double min_t = Math.Min(t1, t2);
                double max_t = Math.Max(t1, t2);
                t = (min_t >= 0) ? min_t : max_t;
            }
            return t;
        }

        private const double Epsilon = 0.000001d;

        protected MatrixCoord3D? GetTimeAndUvCoord(MatrixCoord3D rayOrigin, MatrixCoord3D rayDirection, MatrixCoord3D vert0, MatrixCoord3D vert1, MatrixCoord3D vert2)
        {
            var edge1 = vert1 - vert0;
            var edge2 = vert2 - vert0;

            var pvec = (rayDirection * edge2);

            var det = MatrixCoord3D.scalar(edge1, pvec);

            if (det > -Epsilon && det < Epsilon)
            {
                return null;
            }

            var invDet = 1d / det;

            var tvec = rayOrigin - vert0;

            var u = MatrixCoord3D.scalar(tvec, pvec) * invDet;

            if (u < 0 || u > 1)
            {
                return null;
            }

            var qvec = (tvec * edge1);

            var v = MatrixCoord3D.scalar(rayDirection, qvec) * invDet;

            if (v < 0 || u + v > 1)
            {
                return null;
            }

            var t = MatrixCoord3D.scalar(edge2, qvec) * invDet;

            return new MatrixCoord3D(t, u, v);
        }

        protected MatrixCoord3D GetTrilinearCoordinateOfTheHit(double t, MatrixCoord3D rayOrigin, MatrixCoord3D rayDirection)
        {
            return rayDirection * t + rayOrigin;
        }

        protected MatrixCoord3D CanvasToVieport(int x, int y, double aspect, double fov)
        {

            double fx = aspect * fov * (2 * ((x + 0.5f) / screen.Width) - 1);
            double fy = (1 - (2 * (y + 0.5f) / screen.Height)) * fov;

            return new MatrixCoord3D(fx / scale, fy / scale, -1);
        }
    }

    class RayTraiserPool : RayTraiser
    {
        Size screen;
        public Size Screen { get { return screen; } set { screen = value;} }
        Camera cam;
        double scale;

      

        public RayTraiserPool(Size screen, Camera cam, double scale) : base(screen, cam, scale)
        {
            this.screen = screen;
            this.cam = cam;
            this.scale = scale;
        }

        public override void RayTrasing(IModel model)
        {

            Tuple<MatrixCoord3D, double> sphere = FoundCenter(model.Points);
            List<Thread> threads = new List<Thread>();

            MatrixCoord3D CamPosition = cam.Position.Coords;
            MatrixTransformation3D RotateMatrix = cam.RotateMatrix;

            int x = 0;
            for (int h = 0; h < NumberofThreads; h++)
            {
                threads.Add(new Thread(new ParameterizedThreadStart(ByVertecal)));
                threads[threads.Count - 1].Start(new Limit(x, x + screen.Width / NumberofThreads,model,CamPosition, RotateMatrix,sphere ));
                x += screen.Width / NumberofThreads;
            }
            foreach (var elem in threads)
            {
                elem.Join();
            }
        }

        private void ByVertecal(object obj)
        {
            Limit limit = (Limit)obj;
            Color c;
            double aspect = screen.Width / (double)screen.Height;
            double field = Math.Tan(cam.Fovy / 2 * Math.PI / 180.0f);
            for (int x = limit.begin; x < limit.end; x++)
                for (int y = 0; y < screen.Height; y++)
                {
                    if (Scene.cancelTokenSource.IsCancellationRequested)
                        return;
                    MatrixCoord3D D = CanvasToVieport(x, y,aspect,field) * limit.RotateMatrix;// new MatrixCoord3D(cam.RotateMatrix.Coeff[0,2], cam.RotateMatrix.Coeff[1, 2], cam.RotateMatrix.Coeff[2, 2]);
                    D.Normalise();
                    c = Color.White;
                    if (RaySphereIntersection(limit.CamPosition, D, limit.sphere.Item1, limit.sphere.Item2) != double.MaxValue)
                    {
                        c = RayT(limit.model, D, limit.CamPosition);
                    }
                    PictureBuff.SetPixel(x, y, c.ToArgb());
                }
        }

        class Limit
        {
            public int begin;
            public int end;
            readonly public IModel model;
            readonly public MatrixCoord3D CamPosition;
            readonly public MatrixTransformation3D RotateMatrix;
            readonly public Tuple<MatrixCoord3D, double> sphere;
            public Limit(int begin, int end, IModel model, MatrixCoord3D CamPosition, MatrixTransformation3D RotateMatrix, Tuple<MatrixCoord3D, double> sphere)
            {
                this.begin = begin; this.end = end; this.model = model; 
                this.CamPosition = CamPosition; this.RotateMatrix = RotateMatrix.InversedMatrix();
                this.sphere = sphere;
            }
        }
    }
}
