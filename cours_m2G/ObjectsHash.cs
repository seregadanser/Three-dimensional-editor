using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace cours_m2G
{
   
   
    [Serializable]
    class ModelHash : IModel
    {
        Color color = Color.Black;
        public Id Id { get; set; }
        public Color Color
        {
            get
            { return color; }
            set
            {
                foreach (PolygonComponent l in polygons)
                {
                    l.Color = value;
                }
            }
        }
        public int LastPoint { get { return numpoints; } set { numpoints = value; } }
        protected Container<PointComponent> points;
        protected Container<LineComponent> lines;
        protected Container<PolygonComponent> polygons;
        private int numpoints;
        public Container<PointComponent> Points { get { return points; } }
        public Container<LineComponent> Lines { get { return lines; } }
        public Container<PolygonComponent> Polygons { get { return polygons; } }
        public int NumberPoints { get { return points.Count; } }
        public int NumberLines { get { return lines.Count; } }
        public int NumberPolygons { get { return polygons.Count; } }

        ActiveElements active;
        public List<Id> ActiveComponentsId { get { return active.ActiveComponentsId; } }

        public ModelHash()
        {
            numpoints = 0;
            points = new ContainerHash<PointComponent>();
            lines = new ContainerHash<LineComponent>();
            polygons = new ContainerHash<PolygonComponent>();
            active = new ActiveElements();
        }
        int count = 0;
        public void action(IVisitor visitor)
        {
            if (visitor.type == TypeVisitor.Drawer || visitor.type == TypeVisitor.Reader)
            {
                visitor.visit(this);
                count++;
                if(count!=50)
                return;
                count = 0;
                foreach (IObjects i2 in polygons)
                    ((PolygonComponent)i2).ReCalcNormal();
                return;
            }

            if (active.Count > 0)
                active.action(visitor);
            else
            {
                visitor.visit(this);
            }
            foreach (IObjects i2 in polygons)
                    ((PolygonComponent)i2).ReCalcNormal();
        }

        public bool AddActiveComponent(Id id)
        {
            switch (id.Name)
            {
                case "Point":
                    foreach(IObjects i in points)
                        if(i.Id == id)
                        {
                         return  active.AddElement(i, points.GetParents(id), points.GetChildren(id), (PointComponent)i);
                        }
                    break;
                case "Line":
                    foreach (IObjects i in lines)
                        if (i.Id == id)
                        {
                          return  active.AddElement(i, lines.GetParents(id), lines.GetChildren(id), ((LineComponent)i).Point1, ((LineComponent)i).Point2);
                        }
                    break;
                case "Polygon":
                    foreach (IObjects i in polygons)
                        if (i.Id == id)
                        {
                           return active.AddElement(i, polygons.GetParents(id), polygons.GetChildren(id), ((PolygonComponent)i).Points[0], ((PolygonComponent)i).Points[1], ((PolygonComponent)i).Points[2]);
                        }
                    break;
            }
            return false;
        }

        public void DeliteActive()
        {
            active.ClearElements();
        }
       
        public void DeliteActive(Id id)
        {
            active.RemoveElement(id);
        }

        public List<Id> GetConnectedElements(Id id)
        {
            switch (id.Name)
            {
                case "Point":
                   return points.GetConnectionObjects(id);
                case "Line":
                    return lines.GetConnectionObjects(id);
                case "Polygon":
                   return polygons.GetConnectionObjects(id);
                default:
                    return null;
            }
        }

        public void AddPointToLine(Id LineId, PointComponent point)
        {
            List<Id> pa = lines.GetParents(LineId);
            numpoints++;
            point.Id = new Id("Point", Convert.ToString(numpoints));
            List<PolygonComponent> poly = new List<PolygonComponent>();
            List<PolygonComponent> new_polygons = new List<PolygonComponent>();
            poly.Add(polygons[pa[0]]);
            if (pa.Count == 2)
                poly.Add(polygons[pa[1]]);

            LineComponent oldline = lines[LineId];
            for (int i = 0; i < poly.Count; i++)
            {
                PointComponent thirdpoint = new PointComponent(0, 0, 0);
                for (int j = 0; j < poly[i].Points.Length; j++)
                    if (oldline.Point1 != poly[i].Points[j] && oldline.Point2 != poly[i].Points[j])
                        thirdpoint = poly[i].Points[j];
                new_polygons.Add(new PolygonComponent(thirdpoint, oldline.Point1, point));
                new_polygons.Add(new PolygonComponent(thirdpoint, oldline.Point2, point));
            }
            foreach (PolygonComponent p in new_polygons)
                AddPolygons(p);
            DeliteActive(LineId);
            RemoveLine(LineId);
        }

        public void RemovebyId(Id id)
        {
            switch(id.Name)
            {
                case "Point":
                    RemovePoint(id);
                    break;
                case "Line":
                    RemoveLine(id);
                    break;
                case "Polygon":
                    RemovePolygon(id);
                    break;
            }
        }

        public void RemovePoint(Id id)
        {
                points.Remove(id);
                List<Id> rrl = lines.RemoveChildren(id);
                for (int i = 0; i < rrl.Count; i++)
                {
                    RemoveLine(rrl[i]);
                }
                List<Id> rrp = polygons.RemoveChildren(id);
                for (int i = 0; i < rrp.Count; i++)
                {
                    RemovePolygon(rrp[i]);
                }
            
        }

        public void RemoveLine(Id id)
        {
                lines[id].Color = Color.Black;
                lines.Remove(id);
                List<Id> rrl = points.RemoveParent(id);
                for (int i = 0; i < rrl.Count; i++)
                {
                    RemovePoint(rrl[i]);
                }
                List<Id> rrp = polygons.RemoveChildren(id);
                for (int i = 0; i < rrp.Count; i++)
                {
                    RemovePolygon(rrp[i]);
                }
        }

        public void RemovePolygon(Id id)
        {
            polygons[id].Color = Color.Black;
            polygons.Remove(id);
            List<Id> rrl = lines.RemoveParent(id);
            for (int i = 0; i < rrl.Count; i++)
            {
                RemoveLine(rrl[i]);
            }
            List<Id> rrp = points.RemoveParent(id);
            for (int i = 0; i < rrp.Count; i++)
            {
                RemovePoint(rrp[i]);
            }
        }

        public void AddComponent(IObjects component)
        {
            switch (component.Id.Name)
            {
                case "Point":
                    AddPoint((PointComponent)component);
                    break;
                case "Line":
                    AddLine((LineComponent)component);
                    break;
                case "Polygon":
                    AddPolygons((PolygonComponent)component);
                    break;
            }
        }

        public void SetPointsCoord(Id pointid, MatrixCoord3D coords)
        {
            points[pointid].Coords = coords;
        }
        public void AddPoint(PointComponent point)
        {
            points.Add(new Dict<PointComponent>(point.Id, point));
            numpoints++;
        }

        public void AddLine(LineComponent line)
        {
           LineComponent k = lines.Add(new Dict<LineComponent>(line.Id ,line), 0 ,line.Point1.Id, line.Point2.Id);
            if(k == null)
            {
               PointComponent  k1= points.Add(new Dict<PointComponent>(line.Point1.Id,line.Point1), line.Id);
                if (k1!=null)
                    line.ReplacePoint(line.Point1, k1);
               else
                    numpoints++;
                PointComponent k3 = points.Add(new Dict<PointComponent>(line.Point2.Id, line.Point2), line.Id);
               
                if (k3 != null)
                    line.ReplacePoint(line.Point2, k3);
                else
                    numpoints++;
            }
        }

        public void AddPolygons(PolygonComponent polygon)
        {
            PolygonComponent k = polygons.Add(new Dict<PolygonComponent>(polygon.Id, polygon) ,0, polygon.Points[0].Id, polygon.Points[1].Id, polygon.Points[2].Id, polygon.Lines[0].Id, polygon.Lines[1].Id, polygon.Lines[2].Id);
            if(k == null)
            {
                for(int i = 0; i < polygon.Lines.Length;i++)
                {
                    LineComponent k1 = lines.Add(new Dict<LineComponent>(polygon.Lines[i].Id, polygon.Lines[i]), polygon.Id, 0,polygon.Lines[i].Point1.Id, polygon.Lines[i].Point2.Id);
                    if(k1 != null)
                    { 
                        polygon.ReplaceLine(polygon.Lines[i], k1);
                    }
                    PointComponent k3 = points.Add(new Dict<PointComponent>(polygon.Lines[i].Point1.Id, polygon.Lines[i].Point1), polygon.Id, polygon.Lines[i].Id);
                    
                    if (k3 != null)
                        polygon.ReplacePoint(polygon.Lines[i].Point1, k3);
                    else
                        numpoints++;
                    PointComponent k4 = points.Add(new Dict<PointComponent>(polygon.Lines[i].Point2.Id, polygon.Lines[i].Point2), polygon.Id, polygon.Lines[i].Id);
                    if (k4 != null)
                        polygon.ReplacePoint(polygon.Lines[i].Point2, k4);
                    else
                        numpoints++;
                }
            }
        }

        public IObjects Clone()
        {
            Model m = new Model();

          

            return m;
        }

        public void InversePolygonsNormal(Id id)
        {
            if (polygons[id].Inversed)
                polygons[id].Inversed = false;
            else
                polygons[id].Inversed = true;
        }
    }
    [Serializable]
    class CubHash : ModelHash
    {
        public CubHash(PointComponent center, int side) : base()
        {
            PointComponent p1 = new PointComponent(center.X - side / 2, center.Y + side / 2, center.Z + side / 2, new Id("Point", "1"));
            PointComponent p2 = new PointComponent(center.X - side / 2, center.Y + side / 2, center.Z - side / 2, new Id("Point", "2"));
            PointComponent p3 = new PointComponent(center.X + side / 2, center.Y + side / 2, center.Z - side / 2, new Id("Point", "3"));
            PointComponent p4 = new PointComponent(center.X + side / 2, center.Y + side / 2, center.Z + side / 2, new Id("Point", "4"));
            PointComponent p5 = new PointComponent(center.X - side / 2, center.Y - side / 2, center.Z + side / 2, new Id("Point", "5"));
            PointComponent p6 = new PointComponent(center.X - side / 2, center.Y - side / 2, center.Z - side / 2, new Id("Point", "6"));
            PointComponent p7 = new PointComponent(center.X + side / 2, center.Y - side / 2, center.Z - side / 2, new Id("Point", "7"));
            PointComponent p8 = new PointComponent(center.X + side / 2, center.Y - side / 2, center.Z + side / 2, new Id("Point", "8"));

            PolygonComponent po1 = new PolygonComponent(p1, p2, p6);
            PolygonComponent po2 = new PolygonComponent(p1, p6, p5);
            PolygonComponent po3 = new PolygonComponent(p3, p4, p7);
            PolygonComponent po4 = new PolygonComponent(p8, p7, p4);
            PolygonComponent po5 = new PolygonComponent(p4, p1, p8);
            PolygonComponent po6 = new PolygonComponent(p1, p5, p8);
            PolygonComponent po7 = new PolygonComponent(p1, p3, p2);
            PolygonComponent po8 = new PolygonComponent(p1, p4, p3);
            PolygonComponent po9 = new PolygonComponent(p5, p7, p8);
            PolygonComponent po10 = new PolygonComponent(p5, p6, p7);
            PolygonComponent po11 = new PolygonComponent(p2, p3, p7);
            PolygonComponent po12 = new PolygonComponent(p2, p7, p6);

            AddPolygons(po1);
            AddPolygons(po2);
            AddPolygons(po3);
            AddPolygons(po4);
            AddPolygons(po5);
            AddPolygons(po6);
            AddPolygons(po7);
            AddPolygons(po8);
            AddPolygons(po9);
            AddPolygons(po10);
            AddPolygons(po11);
            AddPolygons(po12);

            polygons[po1.Id].ColorF = Color.Green;
            polygons[po2.Id].ColorF = Color.Aqua;
            polygons[po3.Id].ColorF = Color.Beige;
            polygons[po4.Id].ColorF = Color.Crimson;
            polygons[po5.Id].ColorF = Color.DarkOrange;
            polygons[po6.Id].ColorF = Color.Indigo;
            polygons[po7.Id].ColorF = Color.Ivory;
            polygons[po8.Id].ColorF = Color.Lime;
            polygons[po9.Id].ColorF = Color.Navy;
            polygons[po10.Id].ColorF = Color.Snow;
            polygons[po11.Id].ColorF = Color.Olive;
            polygons[po12.Id].ColorF = Color.Orange;


        }
    }

    class PyramideHash : ModelHash
    {
        public PyramideHash(PointComponent center, int side) : base()
        {
            PointComponent p1 = new PointComponent(center.X - side / 2, center.Y, center.Z + side / 2, new Id("Point", "1"));
            PointComponent p2 = new PointComponent(center.X - side / 2, center.Y, center.Z - side / 2, new Id("Point", "2"));
            PointComponent p3 = new PointComponent(center.X + side / 2, center.Y, center.Z - side / 2, new Id("Point", "3"));
            PointComponent p4 = new PointComponent(center.X + side / 2, center.Y, center.Z + side / 2, new Id("Point", "4"));
            PointComponent p5 = new PointComponent(center.X , center.Y + side, center.Z , new Id("Point", "5"));


            PolygonComponent po1 = new PolygonComponent(p1, p2, p5);
            PolygonComponent po2 = new PolygonComponent(p1, p4, p5);
            PolygonComponent po3 = new PolygonComponent(p2, p3, p5);
            PolygonComponent po4 = new PolygonComponent(p3, p4, p5);
            
            PolygonComponent po5 = new PolygonComponent(p1, p3, p4);
            PolygonComponent po6 = new PolygonComponent(p1, p2, p3);
            AddPolygons(po1);
            AddPolygons(po2);
            AddPolygons(po3);
            AddPolygons(po4);
            AddPolygons(po5);
            AddPolygons(po6);

            //polygons[po1.Id].ColorF = Color.Green;
            //polygons[po2.Id].ColorF = Color.Aqua;
            //polygons[po3.Id].ColorF = Color.Beige;
            //polygons[po4.Id].ColorF = Color.Crimson;
            //polygons[po5.Id].ColorF = Color.DarkOrange;
            //polygons[po6.Id].ColorF = Color.Indigo;
            //polygons[po7.Id].ColorF = Color.Ivory;
            //polygons[po8.Id].ColorF = Color.Lime;
            //polygons[po9.Id].ColorF = Color.Navy;
            //polygons[po10.Id].ColorF = Color.Snow;
            //polygons[po11.Id].ColorF = Color.Olive;
            //polygons[po12.Id].ColorF = Color.Orange;


        }
    }
    class PolygonHash : ModelHash
    {
        public PolygonHash(PointComponent center, int side) : base()
        {
            PointComponent p1 = new PointComponent(center.X, center.Y+side, center.Z, new Id("Point", "1"));
            PointComponent p2 = new PointComponent(center.X+side, center.Y, center.Z, new Id("Point", "2"));
            PointComponent p3 = new PointComponent(center.X + side, center.Y+side, center.Z, new Id("Point", "3"));

            PolygonComponent po1 = new PolygonComponent(p1, p2, p3);
         
            AddPolygons(po1);
        }
    }


}
