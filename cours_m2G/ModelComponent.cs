using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cours_m2G
{
    [Serializable]
    abstract class ModelComponent : IObjects
    {
        public abstract Color Color { get; set; }
        protected Color color = Color.Black;
        protected Id id;
        public abstract Id Id { get; set; }
        public abstract void action(IVisitor visitor);
        public abstract IObjects Clone();
    }
    [Serializable]
    class PointComponent : ModelComponent
    {
        public override Id Id { get { return id; } set { id = value; } }
        private MatrixCoord3D coords;
        private double hit_radius = 10;
        public double HitRadius { get { return hit_radius; } set { hit_radius = value; } }
        public MatrixCoord3D Coords { get { return coords; } set { coords = value; } }
        public double X { get { return coords.X; } }
        public double Y { get { return coords.Y; } }
        public double Z { get { return coords.Z; } }

        public override Color Color { get { return color; } set { color = value; } } 

        public PointComponent(MatrixCoord3D coords)
        {
            this.coords = coords;
            id = new Id("Point", "-1");
        }
        public PointComponent(double x, double y, double z)
        {
            coords = new MatrixCoord3D(x, y, z);
            id = new Id("Point", "-1");
        }
        public PointComponent(double x, double y, double z, Id id ) : this(x, y, z)
        {
            this.id = id;
        }
        public PointComponent(MatrixCoord3D coords, Id id) : this(coords)
        {
            this.id = id;
        }
        public override void action(IVisitor visitor)
        {
            visitor.visit(this);
        }

        public static PointComponent operator *(PointComponent point, MatrixTransformation3D transform)
        {
            return new PointComponent(point.Coords * transform);
        }

        public static bool operator ==(PointComponent p1, PointComponent p2)
        {
            if ((Object)p1 == null && (Object)p2 == null)
                return true;
            if ((Object)p1 == null && (Object)p2 != null)
                return false;
            if ((Object)p1 != null && (Object)p2 == null)
                return false;
            if (p1.X == p2.X && p1.Y == p2.Y && p1.Z == p2.Z)
                return true;
            return false;
        }

        public static bool operator !=(PointComponent p1, PointComponent p2)
        {
            if (p1==p2)
                return false;
            return true;
        }
        public override bool Equals(object obj)
        {
            PointComponent p = (PointComponent)obj;
            if (p == this)
                return true;
            return false;
        }

        public override PointComponent Clone()
        {
            PointComponent p = new PointComponent(X, Y, Z, Id.Clone());
            return p;
        }

        public double Desctination(PointComponent p)
        {
            return Math.Sqrt(Math.Pow(this.X - p.X, 2) + Math.Pow(this.Y - p.Y, 2) + Math.Pow(this.Z - p.Z, 2));
        }

        public override string ToString()
        {
            return Convert.ToString(X) + "\n" + Convert.ToString(Y) + "\n" + Convert.ToString(Z);
        }
    }
    [Serializable]
    class LineComponent : ModelComponent
    {
        public override Id Id { get { return id; } set { id = value; } }
      
        public override Color Color
        {
            get
            { return color; }
            set
            {
                color = value;
                point1.Color = value;
                point2.Color = value;
            }
        }
        private PointComponent point1, point2;
        public PointComponent Point1 { get { return point1; } set { point1.Coords = value.Coords; } }
        public PointComponent Point2 { get { return point2; } set { point2.Coords = value.Coords; } }

        public LineComponent(PointComponent point1, PointComponent point2)
        {
            this.point1 = point1;
            this.point2 = point2;
            string iddesc = "";
            if (Convert.ToInt32(point1.Id.Description) < Convert.ToInt32(point2.Id.Description))
                iddesc = point1.Id.Description + point2.Id.Description;
            else
                iddesc = point2.Id.Description + point1.Id.Description;
            id = new Id("Line", iddesc);
        }
        public LineComponent(PointComponent point1, PointComponent point2, Id id) : this(point1, point2)
        {
            this.id = id;
        }

        public override void action(IVisitor visitor)
        {
            //point1.action(visitor);
            //point2.action(visitor);
            visitor.visit(this);
        }
        public double Len()
        {
            MatrixCoord3D diff = point1.Coords - point2.Coords;
            return Math.Sqrt(diff.X*diff.X + diff.Y*diff.Y + diff.Z*diff.Z);
        }
    
        public void ReplacePoint(PointComponent whatline, PointComponent whichline)
        {
            if (point1 == whatline)
                point1 = whichline;
            if (point2 == whatline)
                point2 = whichline;
        }


        public static LineComponent operator *(LineComponent line, MatrixTransformation3D transform)
        {
            return new LineComponent(line.point1 * transform, line.point2*transform);
        }

        public static bool operator ==(LineComponent p1, LineComponent p2)
        {
            if ((Object)p1 == null && (Object)p2 == null)
                return true;
            if ((Object)p1 == null && (Object)p2 != null)
                return false;
            if ((Object)p1 != null && (Object)p2 == null)
                return false;
            if (p1.id == p2.id)
                return true;
            return false;
        }

        public static bool operator !=(LineComponent p1, LineComponent p2)
        {
            if (p1 == p2)
                return false;
            return true;
        }
        public override bool Equals(object obj)
        {
            LineComponent p = (LineComponent)obj;
            if (p == this)
                return true;
            return false;
        }

        public double Desctination(PointComponent p)
        {
            MatrixCoord3D ab = new MatrixCoord3D(point1.X - point2.X, point1.Y - point2.Y, point1.Z - point2.Z );
            MatrixCoord3D pa = new MatrixCoord3D(p.X - point1.X, p.Y - point1.Y, p.Z - point1.Z);
            MatrixCoord3D pab = ab * pa;
            return Math.Sqrt(MatrixCoord3D.scalar(pab, pab)/ MatrixCoord3D.scalar(ab,ab));
            // return Math.Abs((point2.X - point1.X)*(point1.Y - p.Y)-(point1.X - p.X)*(point2.Y - point1.Y)- (point1.Z - p.Z) * (point2.Z - point1.Z))
            //        /Math.Sqrt(Math.Pow(point1.X - point2.X, 2) + Math.Pow(point1.Y - point2.Y, 2) + Math.Pow(point1.Z - point2.Z, 2));
        }

        public override LineComponent Clone()
        {
            LineComponent p = new LineComponent(point1.Clone(), point2.Clone(), Id.Clone());
            return p;
        }
    }
    [Serializable]
    class PolygonComponent : ModelComponent
    {
        public override Id Id { get { return id; } set { id = value; } }
       
        public override Color Color
        {
            get
            { return lines[0].Color; }
            set
            {
                foreach (LineComponent l in lines)
                {
                    l.Color = value;
                }
           //     color = value;
            }
        }

        public Color ColorF
        {
            get
            { return color; }
            set
            {
                color = value;
            }
        }

        public bool Inversed { get; set; } = false;

        private PointComponent[] points;
        private LineComponent[] lines;
        private PointComponent[] texture;
        public PointComponent[] Points { get { return points; } set { points = value; } }
        public LineComponent[] Lines { get { return lines; } set { lines = value; } }
        public PointComponent[] Text { get { return texture; } set { texture = value; } }

        public PointComponent norm;
        private MatrixCoord3D normal;
        public MatrixCoord3D Normal { get { if (Inversed) return MatrixCoord3D.Inverse(normal); return normal; } }

        public PolygonComponent(PointComponent p1, PointComponent p2, PointComponent p3)
        {
            color = Color.LightYellow;
            points = new PointComponent[3];
            lines = new LineComponent[3];
            points[0] = p1;
            points[1] = p2;
            points[2] = p3;

            lines[0] = new LineComponent(p1, p2);
            lines[1] = new LineComponent(p2, p3);
            lines[2] = new LineComponent(p3, p1);

            string iddesc = "";

            if (Convert.ToInt32(p1.Id.Description) < Convert.ToInt32(p2.Id.Description) && Convert.ToInt32(p1.Id.Description) < Convert.ToInt32(p3.Id.Description))
            {
                if (Convert.ToInt32(p2.Id.Description) < Convert.ToInt32(p3.Id.Description))
                    iddesc = p1.Id.Description + p2.Id.Description + p3.Id.Description;
                else
                    iddesc = p1.Id.Description + p3.Id.Description + p2.Id.Description;
            }
            if (Convert.ToInt32(p2.Id.Description) < Convert.ToInt32(p1.Id.Description) && Convert.ToInt32(p2.Id.Description) < Convert.ToInt32(p3.Id.Description))
            {
                if (Convert.ToInt32(p1.Id.Description) < Convert.ToInt32(p3.Id.Description))
                    iddesc = p2.Id.Description + p1.Id.Description + p3.Id.Description;
                else
                    iddesc = p2.Id.Description + p3.Id.Description + p1.Id.Description;
            }
            if (Convert.ToInt32(p3.Id.Description) < Convert.ToInt32(p1.Id.Description) && Convert.ToInt32(p3.Id.Description) < Convert.ToInt32(p2.Id.Description))
            {
                if (Convert.ToInt32(p1.Id.Description) < Convert.ToInt32(p2.Id.Description))
                    iddesc = p3.Id.Description + p1.Id.Description + p2.Id.Description;
                else
                    iddesc = p3.Id.Description + p2.Id.Description + p1.Id.Description;
            }
            id = new Id("Polygon", iddesc);
            normal = (points[2].Coords - points[1].Coords)*(points[0].Coords - points[1].Coords);
            normal.Normalise();
            norm = new PointComponent(normal);
        }
        public PolygonComponent(PointComponent p1, PointComponent p2, PointComponent p3, Id id) : this(p1, p2, p3)
        {
            this.id = id;
        }
        public PolygonComponent(PointComponent p1, PointComponent p2, PointComponent p3, PointComponent p4, PointComponent p5, PointComponent p6) : this(p1, p2, p3)
        {
            texture = new PointComponent[3];
            texture[0] = p4;
            texture[1] = p5;
            texture[2] = p6;
        }

        public void ReplaceLine(LineComponent whatline, LineComponent whichline)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i] == whatline)
                {
                    lines[i] = whichline;
                  //  ReplacePoint();
                    break;
                }
            }
        }

        public void ReplacePoint(PointComponent whatline, PointComponent whichline)
        {
            for (int i = 0; i < points.Length; i++)
            {
                if (points[i] == whatline)
                {
                    points[i] = whichline;
                    break;
                }
            }
        }

        public void ReCalcNormal()
        {
            normal = (points[2].Coords - points[1].Coords) * (points[0].Coords - points[1].Coords);
            normal.Normalise();
            norm = new PointComponent(normal);
        }

        public override void action(IVisitor visitor)
        {
            visitor.visit(this);
        }
      

        public static bool operator ==(PolygonComponent p1, PolygonComponent p2)
        {
            if ((Object)p1 == null && (Object)p2 == null)
                return true;
            if ((Object)p1 == null && (Object)p2 != null)
                return false;
            if ((Object)p1 != null && (Object)p2 == null)
                return false;
            if (p1.id == p2.id)
                return true;
            return false;
        }

        public static bool operator !=(PolygonComponent p1, PolygonComponent p2)
        {
            if (p1 == p2)
                return false;
            return true;
        }

        public override bool Equals(object obj)
        {
            PolygonComponent p = (PolygonComponent)obj;
            if (p == this)
                return true;
            return false;
        }
        public override PolygonComponent Clone()
        {
            return new PolygonComponent(points[0].Clone(), points[1].Clone(), points[2].Clone(), Id.Clone());
        }
    }

    class PolygonComponentLines : ModelComponent
    {
        public override IObjects Clone() { return null; }
        public override Id Id { get { return id; } set { id = value; } }
        Color color = Color.Black;
        public override Color Color
        {
            get
            { return color; }
            set
            {
                foreach (LineComponent l in lines)
                {
                    l.Color = value;
                }
            }

        }

        private List<PointComponent> points;
        private List<LineComponent> lines;

        public int NumberOfLines { get { return lines.Count; } }
       
        public List<PointComponent> Points { get { return points; } set { points = value; }}
        public List<LineComponent> Lines { get { return lines; } set { lines = value; } }
        //public PolygonComponent ()
        //{
        //    points = new List<PointComponent>();
        //    lines = new List<LineComponent>();
        //}
        //public PolygonComponent(params LineComponent[] inlines)
        //{
        //    points = new List<PointComponent>();
        //    lines = new List<LineComponent>();
      
        //    for (int i = 0; i < inlines.Length; i++)
        //        AddLine(inlines[i]);
        //}

        public void AddLine(LineComponent line)
        {
            bool inlist = true;
            foreach(LineComponent l in lines)
            {
                if (l == line)
                    inlist = false;
            }
            if(inlist)
            {
                lines.Add(line);

                bool pi1 = false, pi2 = false;
                foreach (PointComponent p in points)
                {
                    if (p == line.Point1)
                        pi1 = true;
                    if (p == line.Point2)
                        pi2 = true;
                }
                if (!pi1)
                    points.Add(line.Point1);
                if (!pi2)
                    points.Add(line.Point2);
            }
        }

        public void DelitLine(LineComponent line)
        {
            points.Remove(line.Point1);
            points.Remove(line.Point2);
            lines.Remove(line);

        }

        public override void action(IVisitor visitor)
        {
          // visitor.visit(this);
        }

    
    }
}
