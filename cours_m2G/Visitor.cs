using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Linq;
using System.Drawing.Imaging;
using System.Diagnostics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace cours_m2G
{
   
     interface IVisitor
    {
        public TypeVisitor type { get; }
        abstract public void visit(PointComponent point);
        abstract public void visit(LineComponent line);
        abstract public void visit(PolygonComponent polygon);
        abstract public void visit(IModel model);
    
    }

    abstract class ScreenVisitor : IVisitor
    {
         protected int scale;
         protected Size screen;
        
        public virtual int Scale { get { return scale; } set { if(value>0) scale = value; } }
        public virtual Size Screen { get { return screen; } set { screen = value; } }

        public abstract TypeVisitor type { get; }

        public abstract void visit(PointComponent p);
        public abstract void visit(LineComponent l);
        public abstract void visit(PolygonComponent polygon);
        abstract public void visit(IModel model);


    }

    class DrawVisitor : ScreenVisitor
    {
        public override TypeVisitor type { get; } = TypeVisitor.Drawer;
        public virtual Bitmap Bmp { get { return raster.Bmp; } }
        public override int Scale { get { return scale; } set { if (value > 0) { scale = value; raster.Scale = value; } } }
        public override Size Screen { get { return screen; } set { screen = value; raster.Screen = value; } }
        public bool PrintText { get; set; } = true;
        protected Rasterizator raster;
       // protected R2 raster1;
       /// <summary>
       /// 0-2
       /// </summary>
        public virtual int SetRaster
        {
            set
            {
                if(value==0)
                    raster = new RasterizatorNoCutter(scale, screen);
                if (value == 1)
                    raster = new RasterizatorNoText(scale, screen);
                if(value == 2)
                    raster = new RasterizatorNoPoints(scale, screen);
                if(value == 3)
                    raster = new RasterizatorCutter(scale, screen);
            }
        }


        public DrawVisitor(Size screen, int scale)
        {
            this.screen = screen;
            this.scale = scale;
            raster = new RasterizatorNoCutter(scale,screen);
        }

        public override void visit(PointComponent point)
        {
            raster.DrawPoint(point);
        }
        public override void visit(LineComponent line)
        {
            raster.DrawLine(line);
            line.Point1.action(this);
            line.Point2.action(this);
        }
        int k = 0;
        public override void visit(PolygonComponent polygon)
        {
           
            //raster1.drawTriangleFill(polygon.Points, polygon.ColorF);
            raster.DrawPolygon(polygon);
            foreach (LineComponent l in polygon.Lines)
            { if(l!=null)
                l.action(this);
         }
        }

        public override void visit(IModel model)
        {
            PictureBuff.Filled = false;
            PictureBuff.Creator = raster.Type;
            PictureBuff.Clear();
    
            foreach (PolygonComponent l in model.Polygons)
            {
                if (Scene.cancelTokenSource.IsCancellationRequested)
                    return;
                l.action(this);
            }
     
          
            PictureBuff.Filled = true;
        }
     
    }

    class DrawVisitorCamera : DrawVisitor
    {
        Camera cam;
        public Camera Cam { get { return cam; } set { cam = value; } }
        /// <summary>
        /// 0-2
        /// </summary>
        public override int SetRaster
        {
            set
            {
                if (value == 0)
                    raster = new RasterizatorNoCutter(cam, scale, screen);
                if (value == 1)
                    raster = new RasterizatorCutter(cam,scale, screen);
                if (value == 2)
                    raster = new RasterizatorCutterBarri(cam,scale, screen);
                //if (value == 3)
                //    raster = new RasterizatorCutter(cam, scale, screen);
            }
        }
        public DrawVisitorCamera(Size screen, int scale, Camera cam) : base(screen, scale)
        {
            this.cam = cam;
            raster = new RasterizatorCutter(cam, scale, screen);
        }
        int k = 0;
       
        public override void visit(IModel model)
        {
            PictureBuff.Creator = raster.Type;
            PictureBuff.Clear();
            PictureBuff.Filled = false;
            raster.up = cam;
            foreach (PolygonComponent l in model.Polygons)
            {
                if (Scene.cancelTokenSource.IsCancellationRequested)
                    return;
                // if(MatrixCoord3D.scalar(l.Normal,cam.Direction)>0)
                if (l!=null)
                l.action(this);
            }
            PictureBuff.Filled = true;
        }
       
    }

    class DrawVisitorR : DrawVisitor
    {
        Camera cam;
        RayTraiser rayt;
        public override Size Screen { get { return screen; } set { screen = value; rayt.Screen = value; } }
        public override int Scale { get { return scale; } set { if (value > 0) { scale = value; rayt.Scale = value; } } }
        public override int SetRaster
        {
            set
            {
                if (value == 0)
                    rayt = new RayTraiserPool(screen, cam, scale);
                if (value == 1)
                  rayt =  new RayTraiser(screen, cam, scale);
            }
        }

        public override Bitmap Bmp { get { return PictureBuff.GetBitmap(); } }
        
        public DrawVisitorR(Size screen, int scale, Camera cam) : base(screen, scale)
        {
            this.cam = cam;
            rayt = new RayTraiserPool(screen, cam, scale);
        }
        
    public override void visit(IModel model)
        {
            PictureBuff.Filled = false;
            PictureBuff.Creator = RenderType.RAY;
            
            rayt.RayTrasing(model);
            PictureBuff.Filled = true;
        }

      
    }
    class DrawVisitorR1 : DrawVisitor
    {
        Camera cam;
        RayTraiser rayt;


        public override Bitmap Bmp { get { return PictureBuff.GetBitmap(); } }

        public DrawVisitorR1(Size screen, int scale, Camera cam) : base(screen, scale)
        {
            this.cam = cam;
            rayt = new RayTraiserPool(screen, cam, scale);
        }

        public override void visit(IModel model)
        {
            PictureBuff.Filled = false;
            PictureBuff.Creator = RenderType.RAY;

            rayt.RayTrasing(model);
            PictureBuff.Filled = true;
        }


    }

    class ReadVisitor : ScreenVisitor
    {
        public override TypeVisitor type { get; } = TypeVisitor.Reader;
        public Point InPoint { get; set; } = new Point(0, 0);
        protected PolygonComponent find;
        public PolygonComponent Find { get { return find; } }

        protected PointComponent findpoint;
        public PointComponent Findpoint { get { return findpoint; } }
        public override int Scale { get { return scale; } set { if (value > 0) { scale = value; } } }
        public ReadVisitor(Size screen, int scale)
        {
            this.screen = screen;
            this.scale = scale;
        }

        public override void visit(PointComponent p)
        {
 
        }
        public override void visit(LineComponent l)
        {
     
        }

        public override void visit(PolygonComponent polygon)
        {

        }

        public override void visit(IModel model)
        {

        }
   
    }

    class ReadVisitorCamera : ReadVisitor
    {
        readonly Camera cam;
   
        public ReadVisitorCamera(Camera cam, Size screen, int scale) : base(screen, scale)
        {
            this.cam = cam;
        }

        public async override void visit(IModel model)
        {   
            MatrixCoord3D CamPosition = cam.Position.Coords;
            MatrixTransformation3D RotateMatrix = cam.RotateMatrix;
       

            MatrixCoord3D D = CanvasToVieport(InPoint.X, InPoint.Y) * RotateMatrix.InversedMatrix();// new MatrixCoord3D(cam.RotateMatrix.Coeff[0,2], cam.RotateMatrix.Coeff[1, 2], cam.RotateMatrix.Coeff[2, 2]);
            D.Normalise();
            find = RayT(model, D, CamPosition);
        }
    

        private PolygonComponent RayT(IModel model, MatrixCoord3D D, MatrixCoord3D position)
        {
            PolygonComponent closest = null;
            double closest_t = double.MaxValue-1;
            //  Parallel.ForEach<PolygonComponent>(model.Polygons, p=> { 
            foreach (PolygonComponent p in model.Polygons)
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
        
            findpoint = new PointComponent(GetTrilinearCoordinateOfTheHit(closest_t, position, D));
            
            return closest;
        }
        private PolygonComponent RayT(ModelHash model, MatrixCoord3D D, MatrixCoord3D position)
        {
            PolygonComponent closest = null;
            double closest_t = double.MaxValue - 1;
            //  Parallel.ForEach<PolygonComponent>(model.Polygons, p=> { 
            foreach (PolygonComponent p in model.Polygons)
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
           
            findpoint = new PointComponent(GetTrilinearCoordinateOfTheHit(closest_t, position, D));

            return closest;
        }

        private const double Epsilon = 0.000001d;

        double RaySphereIntersection(MatrixCoord3D rayOrigin, MatrixCoord3D rayDirection, MatrixCoord3D spos, double r)
        {
            double t = Double.MaxValue;
            
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

        public static MatrixCoord3D? GetTimeAndUvCoord(MatrixCoord3D rayOrigin, MatrixCoord3D rayDirection, MatrixCoord3D vert0, MatrixCoord3D vert1, MatrixCoord3D vert2)
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


        public static MatrixCoord3D GetTrilinearCoordinateOfTheHit(double t, MatrixCoord3D rayOrigin, MatrixCoord3D rayDirection)
        {
            return rayDirection * t + rayOrigin;
        }

        private MatrixCoord3D CanvasToVieport(int x, int y)
        {
            double aspectRatio = screen.Width / (double)screen.Height;
            double fieldOfView = Math.Tan(cam.Fovy / 2 * Math.PI / 180.0f);

            double fx = aspectRatio * fieldOfView * (2 * ((x + 0.5f) / screen.Width) - 1);
            double fy = (1 - (2 * (y + 0.5f) / screen.Height)) * fieldOfView;

            return new MatrixCoord3D(fx / scale, fy / scale, -1);
        }
    }

    class EasyTransformVisitor : IVisitor
    {
        public TypeVisitor type { get; } = TypeVisitor.Transform;
        MatrixTransformation3D TransformMatrix;
        public EasyTransformVisitor(MatrixTransformation3D transformMatrix)
        {
            TransformMatrix = transformMatrix;
        }

        public void visit(PointComponent point)
        {
            point.Coords = point.Coords * TransformMatrix;
        }
        public void visit(LineComponent line)
        {
            line.Point1 = line.Point1 * TransformMatrix;
            line.Point2 = line.Point2 * TransformMatrix;
        }

        public void visit(PolygonComponent polygon)
        {
            foreach(PointComponent p in polygon.Points)
            {
                p.action(this);
            }
        }

        public void visit(IModel model)
        {
            foreach (PointComponent p in model.Points)
            {
                p.action(this);
            }
        }

    }
    class HardTransformVisitor : IVisitor
    {
        public TypeVisitor type { get; } = TypeVisitor.Transform;
        MatrixTransformation3D TransformMatrix;
        PointComponent pointp;
        public HardTransformVisitor(MatrixTransformation3D transformMatrix, PointComponent point)
        {
            TransformMatrix = transformMatrix;
            this.pointp = point;
        }

        public void visit(PointComponent point)
        {
            MatrixTransformation3D transfer = new MatrixTransformationTransfer3D(-pointp.X, -pointp.Y, -pointp.Z);
            point.Coords = point.Coords * transfer;
            point.Coords = point.Coords * TransformMatrix;
            transfer = new MatrixTransformationTransfer3D(pointp.X, pointp.Y, pointp.Z);
            point.Coords = point.Coords * transfer;
        }
        public void visit(LineComponent line)
        {
            line.Point1.action(this);
            line.Point2.action(this);
        }

        public void visit(PolygonComponent polygon)
        {
            foreach (PointComponent p in polygon.Points)
            {
                p.action(this);
            }
        }

        public void visit(IModel model)
        {
            foreach (PointComponent p in model.Points)
            {
                p.action(this);
            }
        }
      
    }

  
}
