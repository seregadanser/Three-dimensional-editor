using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace cours_m2G
{
    class ObjReader
    {
        protected StreamReader f;
        protected string filename;
        List<PointComponent> pointsObj;
        List<PolygonComponent> polygonsObj;
        List<PointComponent> textureObj;
        public ObjReader(string s)
        {
            pointsObj = new List<PointComponent>();
            polygonsObj = new List<PolygonComponent>();
            textureObj = new List<PointComponent>();
            filename = s;
        }

        public virtual IModel ReadModel()
        {
           f = new StreamReader(filename);

            IModel M = new ModelHash();
            while (true)
            {
                string temp = f.ReadLine();

                if (temp == null) break;

                string[] str = temp.Split(" ", StringSplitOptions.RemoveEmptyEntries);

                if (str.Length != 0)
                {
                    if (str[0] == "v")
                    {
                        NewPoint(str);
                    }
                    if (str[0] == "f")
                    {
                        NewPolygon(str);
                    }
                    if (str[0] == "vt")
                    {
                        NewTexture(str);
                    }
                }
            }

            //foreach (PointComponent p in pointsObj)
            //    M.AddComponent(p);
            foreach (PolygonComponent p in polygonsObj)
                M.AddComponent(p);
            f.Close();
            pointsObj.Clear();
            polygonsObj.Clear();
            textureObj.Clear();
            return M;
        }

        void NewPoint(string[] str)
        {
            string g = Convert.ToString(pointsObj.Count + 1);
            Id i = new Id("Point", g);
            double a, b, c;
            double.TryParse(str[1], NumberStyles.Any, CultureInfo.InvariantCulture, out a);
            double.TryParse(str[2], NumberStyles.Any, CultureInfo.InvariantCulture, out b);
            double.TryParse(str[3], NumberStyles.Any, CultureInfo.InvariantCulture, out c);
            pointsObj.Add(new PointComponent(a, b, c, i));
        }
        void NewTexture(string[] str)
        {
            string g = Convert.ToString(pointsObj.Count + 1);
            Id i = new Id("Texture", g);
            double a, b, c;
            double.TryParse(str[1], NumberStyles.Any, CultureInfo.InvariantCulture, out a);
            double.TryParse(str[2], NumberStyles.Any, CultureInfo.InvariantCulture, out b);
            double.TryParse(str[3], NumberStyles.Any, CultureInfo.InvariantCulture, out c);
            textureObj.Add(new PointComponent(a, b, c, i));
        }

        void NewPolygon(string[] str)
        {
            List<int> p = new List<int>();
            List<int> pt = new List<int>();
            for (int i = 1; i < str.Length; i++)
            {
                string[] tmp = str[i].Split("/", StringSplitOptions.RemoveEmptyEntries);
                p.Add(Convert.ToInt32(tmp[0]));
                pt.Add(Convert.ToInt32(tmp[1]));
            }

            if (p.Count == 3)
            {
                PolygonComponent pp = new PolygonComponent(pointsObj[p[0] - 1], pointsObj[p[1] - 1], pointsObj[p[2] - 1], textureObj[pt[0] - 1], textureObj[pt[1] - 1], textureObj[pt[2] - 1]);

                PointComponent[] tex = new PointComponent[3];
                tex[0] = textureObj[pt[0] - 1];
                tex[1] = textureObj[pt[1] - 1];
                tex[2] = textureObj[pt[2] - 1];
                // pp.Text = tex;
                polygonsObj.Add(pp);
            }
            if (p.Count == 4)
            {
                PolygonComponent pp = new PolygonComponent(pointsObj[p[0] - 1], pointsObj[p[1] - 1], pointsObj[p[2] - 1]);// textureObj[pt[0] - 1], textureObj[pt[1] - 1], textureObj[pt[2] - 1]);
                PolygonComponent pp1 = new PolygonComponent(pointsObj[p[2] - 1], pointsObj[p[3] - 1], pointsObj[p[0] - 1]);//, textureObj[pt[2] - 1], textureObj[pt[3] - 1], textureObj[pt[0] - 1]);

                //PointComponent[] tex = new PointComponent[3];
                //tex[0] = textureObj[pt[0] - 1];
                //tex[1] = textureObj[pt[1] - 1];
                //tex[2] = textureObj[pt[2] - 1];

                //PointComponent[] tex1 = new PointComponent[3];
                //tex[0] = textureObj[pt[2] - 1];
                //tex[1] = textureObj[pt[3] - 1];
                //tex[2] = textureObj[pt[0] - 1];

                //pp.Text = tex;
                //pp1.Text = tex1;

                polygonsObj.Add(pp);
                polygonsObj.Add(pp1);
            }
        }
    }

    class ObjReaderJson:ObjReader
    {
        public ObjReaderJson(string s) :base(s)
        {
            
        }

        public override IModel ReadModel()
        {
            IModel model = null;
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate))
            {
                model = (IModel)formatter.Deserialize(fs);
            }
            return model;
        }
    }

    class ObjWriter
    {
        StreamReader f;
        protected string filename;
        public ObjWriter(string s)
        {
            filename = s; 
        }

        public void WriteModel(IModel model)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate))
            {
                formatter.Serialize(fs, model);
                Console.WriteLine("Data has been saved to file");
            }
        }



    }


}
