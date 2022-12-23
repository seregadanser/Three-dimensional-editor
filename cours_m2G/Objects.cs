using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.LinkLabel;

namespace cours_m2G
{
    interface IObjects
    {
        public abstract Color Color { get; set; }
        public Id Id { get; set; }
        public abstract void action(IVisitor visitor);
        public abstract IObjects Clone();
    }
    [Serializable]
    class ActiveElements : IObjects
    {
        Color color = Color.Black;
        public Id Id { get; set; }
        public Color Color
        {
            get
            { return color; }
            set
            {
                color = value;
            }
        }
        public int Count { get { return activeComponents.Count; } }

        List<IObjects> activeComponents;
        Container<PointComponent> activepoints;
        List<Id> activeComponentsId;
        List<List<Id>> parents, childeren;

        public List<Id> ActiveComponentsId { get { return activeComponentsId; } }

        public ActiveElements()
        {
            activeComponents = new List<IObjects>();
            activepoints = new ContainerList<PointComponent>();
            activeComponentsId = new List<Id>();
            parents = new List<List<Id>>();
            childeren = new List<List<Id>>();
        }

        public void action(IVisitor visitor)
        {
            foreach (IObjects i in activepoints)
            {
                i.action(visitor);
            }
                
        }

        public bool AddElement(IObjects element, List<Id> parent, List<Id> child, params PointComponent[] points)
        {
            Id eid = element.Id;
            foreach (Id i in activeComponentsId)
                if (eid == i)
                    return false;
            for (int i = 0; i < activeComponents.Count; i++)
                foreach (Id id in childeren[i])
                    if (id == eid)
                        return false;
            for (int i = activeComponents.Count -1; i> -1; i--)
                foreach (Id id in parents[i])
                    if (id == eid)
                    {
                        RemoveElement(activeComponentsId[i]);
                    }
            element.Color = Color.Red;
            activeComponents.Add(element);
            activeComponentsId.Add(element.Id);
            parents.Add(parent);
            childeren.Add(child);
            foreach (PointComponent i in points)
                activepoints.Add(i, i.Id, element.Id);
            return true;
        }
        public bool RemoveElement(Id id)
        {
            int pos = -1;
            for (int i = 0; i < activeComponents.Count; i++)
                if (activeComponentsId[i] == id)
                { pos = i; break; }
            if(pos!=-1)
            {
                activeComponents[pos].Color = Color.Black;
              List<Id> l = activepoints.RemoveParent(id);
                foreach (Id i in l)
                    activepoints.Remove(i);
                activeComponents.RemoveAt(pos);
                activeComponentsId.RemoveAt(pos);
                parents.RemoveAt(pos);
                childeren.RemoveAt(pos);
                return true;
            }
            return false;
        }
        public void ClearElements()
        {
            foreach (IObjects i in activeComponents)
                i.Color = Color.Black;
            activeComponents.Clear();
            activeComponentsId.Clear();
            parents.Clear();
            childeren.Clear();
        }

        public IObjects Clone()
        {
            ActiveElements activeElements = new ActiveElements();
            
            for(int i = 0; i<activeComponents.Count;i++)
            {
                activeElements.activeComponents.Add(activeComponents[i].Clone());
                activeElements.activeComponentsId.Add(activeComponentsId[i].Clone());
                activeElements.parents.Add(new List<Id>());
                for (int j = 0; j < parents[i].Count; j++)
                    activeElements.parents[i].Add(parents[i][j].Clone());
                activeElements.childeren.Add(new List<Id>());
                for (int j = 0; j < childeren[i].Count; j++)
                    activeElements.childeren[i].Add(childeren[i][j].Clone());
            }

            return activeElements;
        }
    }
    interface IModel : IObjects
    {
        public Container<PointComponent> Points { get; }
        public Container<LineComponent> Lines { get;  }
        public Container<PolygonComponent> Polygons { get; }
        public int LastPoint { get; set; }
        public bool AddActiveComponent(Id id);
        public void DeliteActive(); public void DeliteActive(Id id); public List<Id> GetConnectedElements(Id id);
        public void AddPointToLine(Id LineId, PointComponent point);
        public void RemovebyId(Id id);
        public void RemovePoint(Id id);
        public void RemoveLine(Id id);
        public void RemovePolygon(Id id);
        public void AddComponent(IObjects component);
        public void AddPoint(PointComponent point);
        public void AddLine(LineComponent line);
        public void AddPolygons(PolygonComponent polygon);
        public void SetPointsCoord(Id pointid, MatrixCoord3D coords);
        public void InversePolygonsNormal(Id id);


    }
    [Serializable]
    class Model : IModel
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

        public Model()
        {
            numpoints = 0;
            points = new ContainerList<PointComponent>();
            lines = new ContainerList<LineComponent>();
            polygons = new ContainerList<PolygonComponent>();
            active = new ActiveElements();
        }

        public void action(IVisitor visitor)
        {
            if (visitor.type == TypeVisitor.Drawer || visitor.type == TypeVisitor.Reader)
            {
                visitor.visit(this);
                return;
            }

            if (active.Count > 0)
                active.action(visitor);
            else
            {
                visitor.visit(this);
            }
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
            List<Id> pa =  lines.GetParents(LineId);
            point.Id = new Id("Point", Convert.ToString(numpoints+1));
            numpoints++;
            List<PolygonComponent> poly = new List<PolygonComponent>();
            poly.Add(polygons[pa[0]]);
            if (pa.Count == 2)
                poly.Add(polygons[pa[1]]);
            LineComponent oldline = lines[LineId];
            List<PolygonComponent> new_polygons = new List<PolygonComponent>();
            for(int i =0; i<poly.Count;i++)
            {
                PointComponent thirdpoint = new PointComponent(0,0,0);
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
            int delindex = -1;
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i].Id == id)
                { delindex = i; break; }
            }
            if (delindex != -1)
            {
                List<Id> parents = points.Remove(delindex).Item1;
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
        }

        public void RemoveLine(Id id)
        {
            int delindex = -1;
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Id == id)
                { delindex = i; break; }
            }
            if (delindex != -1)
            {
                lines[delindex].Color = Color.Black;
                lines.Remove(delindex);
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
        }

        public void RemovePolygon(Id id)
        {
            int delindex = -1;
            for(int i= 0; i < polygons.Count; i++)
            {
                if (polygons[i].Id == id)
                { delindex = i;break; }
            }
            if (delindex != -1)
            {
                polygons[delindex].Color = Color.Black;
                List<Id> children = polygons.Remove(delindex).Item2;
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

        public void AddPoint(PointComponent point)
        {
            points.Add(point, point.Id);
            numpoints++;
        }

        public void AddLine(LineComponent line)
        {
           int k = lines.Add(line, line.Id,0 ,line.Point1.Id, line.Point2.Id);
            if(k ==-1)
            {
               k= points.Add(line.Point1, line.Point1.Id, line.Id);
                numpoints++;
                if (k!=-1)
                {
                    line.ReplacePoint(line.Point1, points[k]);
                }
                k= points.Add(line.Point2, line.Point2.Id, line.Id);
                numpoints++;
                if (k != -1)
                {
                    line.ReplacePoint(line.Point2, points[k]);
                }
            }
        }

        public void AddPolygons(PolygonComponent polygon)
        {
            int k = polygons.Add(polygon,polygon.Id ,0, polygon.Points[0].Id, polygon.Points[1].Id, polygon.Points[2].Id, polygon.Lines[0].Id, polygon.Lines[1].Id, polygon.Lines[2].Id);
            if(k == -1)
            {
                for(int i = 0; i < polygon.Lines.Length;i++)
                {
                    k = lines.Add(polygon.Lines[i], polygon.Lines[i].Id, polygon.Id, 0,polygon.Lines[i].Point1.Id, polygon.Lines[i].Point2.Id);
                    if(k!=-1)
                    { 
                        polygon.ReplaceLine(polygon.Lines[i], lines[k]);
                    }
                    k = points.Add(polygon.Lines[i].Point1, polygon.Lines[i].Point1.Id, polygon.Id, polygon.Lines[i].Id);
                    numpoints++;
                    if (k != -1)
                        polygon.ReplacePoint(polygon.Lines[i].Point1, points[k]);
                    k= points.Add(polygon.Lines[i].Point2, polygon.Lines[i].Point2.Id, polygon.Id, polygon.Lines[i].Id);
                    numpoints++;
                    if (k != -1)
                        polygon.ReplacePoint(polygon.Lines[i].Point2, points[k]);
                }
            }
        }

        public IObjects Clone()
        {
            Model m = new Model();

          

            return m;
        }

        public void SetPointsCoord(Id pointid, MatrixCoord3D coords)
        {
            throw new NotImplementedException();
        }

        public void InversePolygonsNormal(Id id)
        {
            throw new NotImplementedException();
        }
    }
    [Serializable]
    class Cub : Model
    {
        public Cub(PointComponent center, int side) : base()
        {
            PointComponent p1 = new PointComponent(center.X - side / 2, center.Y + side / 2, center.Z + side / 2, new Id("Point", "1"));
            PointComponent p2 = new PointComponent(center.X - side / 2, center.Y + side / 2, center.Z - side / 2, new Id("Point", "2"));
            PointComponent p3 = new PointComponent(center.X + side / 2, center.Y + side / 2, center.Z - side / 2, new Id("Point", "3"));
            PointComponent p4 = new PointComponent(center.X + side / 2, center.Y + side / 2, center.Z + side / 2, new Id("Point", "4"));
            PointComponent p5 = new PointComponent(center.X - side / 2, center.Y - side / 2, center.Z + side / 2, new Id("Point", "5"));
            PointComponent p6 = new PointComponent(center.X - side / 2, center.Y - side / 2, center.Z - side / 2, new Id("Point", "6"));
            PointComponent p7 = new PointComponent(center.X + side / 2, center.Y - side / 2, center.Z - side / 2, new Id("Point", "7"));
            PointComponent p8 = new PointComponent(center.X + side / 2, center.Y - side / 2, center.Z + side / 2, new Id("Point", "8"));

            AddPoint(p1);
            AddPoint(p2);
            AddPoint(p3);
            AddPoint(p4);
            AddPoint(p5);
            AddPoint(p6);
            AddPoint(p7);
            AddPoint(p8);

            AddLine(new LineComponent(p1, p2));
            AddLine(new LineComponent(p2, p3));
            AddLine(new LineComponent(p3, p4));
            AddLine(new LineComponent(p4, p1));
            AddLine(new LineComponent(p5, p6));
            AddLine(new LineComponent(p6, p7));
            AddLine(new LineComponent(p7, p8));
            AddLine(new LineComponent(p8, p5));
            AddLine(new LineComponent(p1, p5));
            AddLine(new LineComponent(p2, p6));
            AddLine(new LineComponent(p3, p7));
            AddLine(new LineComponent(p4, p8));

            AddPolygons(new PolygonComponent(p1,p2,p6));
            AddPolygons(new PolygonComponent(p1,p6,p5));
            AddPolygons(new PolygonComponent(p3,p4,p7));
            AddPolygons(new PolygonComponent(p8,p7,p4));
            AddPolygons(new PolygonComponent(p4,p1,p8));
            AddPolygons(new PolygonComponent(p1,p5,p8));
            AddPolygons(new PolygonComponent(p1,p3,p2));
            AddPolygons(new PolygonComponent(p1,p4,p3));
            AddPolygons(new PolygonComponent(p5,p7,p8));
            AddPolygons(new PolygonComponent(p5,p6,p7));
            AddPolygons(new PolygonComponent(p2,p3,p7));
            AddPolygons(new PolygonComponent(p2,p7,p6));

            polygons[0].ColorF = Color.Green;
            polygons[1].ColorF = Color.Aqua;
            polygons[2].ColorF = Color.Beige;
            polygons[3].ColorF = Color.Crimson;
            polygons[4].ColorF = Color.DarkOrange;
            polygons[5].ColorF = Color.Indigo;
            polygons[6].ColorF = Color.Ivory;
            polygons[7].ColorF = Color.Lime;
            polygons[8].ColorF = Color.Navy;
            polygons[9].ColorF = Color.Snow;
            polygons[10].ColorF = Color.Olive;
            polygons[11].ColorF = Color.Orange;
        }
    }
    class Pyramide1 : Model
    {
        public Pyramide1(PointComponent center,double height, double radius) : base()
        {

        }
    }
}
