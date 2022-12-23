using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace cours_m2G
{
    class Scene
    {
        DrawVisitor Drawer;
        ReadVisitor Reader;
        IModel model;
        Camera camera;

        Thread renderThread;
        public static CancellationTokenSource cancelTokenSource;
        PictureBox picture;
        
        public Label l2;
        public Label l3;

        public Scene(PictureBox picture)
        {
            camera = new Camera(new PointComponent(0, 0, 100), new MatrixCoord3D(0, 0, 0), new MatrixCoord3D(0, 1, 0), new MatrixPerspectiveProjection(90, picture.Size.Width / (double)picture.Size.Height, 1, 1000));
          //  camera = new Camera(new PointComponent(0, 0, 300), new MatrixCoord3D(0, 0, 0), new MatrixCoord3D(0, 1, 0), new MatrixOrtoProjection(0, picture.Size.Width, 0, picture.Height, 0, 1000));
            Drawer = new DrawVisitorCamera(picture.Size, 1, camera);
            Reader = new ReadVisitorCamera(camera, picture.Size, 1);
           model = new PolygonHash(new PointComponent(0, 0, 0), 20);
            // model = new PyramideHash(new PointComponent(0, 0, 0), 20);
              model = new CubHash(new PointComponent(0, 0, 0), 20);
            // model = new Cub(new PointComponent(0, 0, 0), 20);
            this.picture = picture;
            cancelTokenSource = new CancellationTokenSource();
            renderThread = new Thread(new ParameterizedThreadStart(RenderLoop));
            renderThread.Name = "drawing";
            renderThread.IsBackground = true;
            renderThread.Start(cancelTokenSource.Token);

        
        }
        public Scene(PictureBox picture, ObjReader re)  
        {
            camera = new Camera(new PointComponent(0, 0, 300), new MatrixCoord3D(0, 0, 0), new MatrixCoord3D(0, 1, 0), new MatrixPerspectiveProjection(90, picture.Size.Width / (double)picture.Size.Height, 1, 1000));
            Drawer = new DrawVisitorCamera(picture.Size, 1, camera);
            Reader = new ReadVisitorCamera(camera, picture.Size, 1);
            model = re.ReadModel();

            this.picture = picture;
            cancelTokenSource = new CancellationTokenSource();
            renderThread = new Thread(new ParameterizedThreadStart(RenderLoop));
            renderThread.Name = "drawing";
            renderThread.IsBackground = true;
            renderThread.Start(cancelTokenSource.Token);

      
        }
     

        #region Render
        Stopwatch strender = new Stopwatch();
        Stopwatch strefresh = new Stopwatch();
        public void RenderLoop(object boxedToken)
        {
            var cancellationToken = (CancellationToken)boxedToken;
            while (!cancellationToken.IsCancellationRequested)
            {
                strender.Start();
                model.action(Drawer);
                strender.Stop();
           
                TimeSpan ts = strender.Elapsed;
                   
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds,
                    ts.Milliseconds / 10);
                if (l2 == null)
                    Console.WriteLine("RenderTime " + elapsedTime + " " + "Percent: ");
                else
                     if (l2.InvokeRequired && !cancellationToken.IsCancellationRequested)
                {
                    l2.Invoke(new MethodInvoker(delegate
                    {
                        l2.Text = elapsedTime;
                    }));
                }
             
                strender.Reset();

                strefresh.Start();
                Refresh();
                strefresh.Stop();
                ts = strefresh.Elapsed;

                 elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:0000}",
                    ts.Hours, ts.Minutes, ts.Seconds,
                    ts.Milliseconds / 10);
                //Console.WriteLine("RefreshTime " + elapsedTime);
                strefresh.Reset();
      
            }
           

        }
        public void Refresh()
        {
            Bitmap LocalBMP = new Bitmap(picture.Size.Width,picture.Size.Height);
            if (PictureBuff.Filled)
                LocalBMP = Drawer.Bmp;
          //  int coloredp = 0;
            if (picture.InvokeRequired)
            {
                picture.Invoke(new MethodInvoker(delegate
                {
                    picture.Image = LocalBMP;
                    Bitmap p = (Bitmap)picture.Image;

                    //for (int i = 0; i < p.Size.Width; i++)
                    //    for (int j = 0; j < p.Size.Height; j++)
                    //    {
                    //        Color r = p.GetPixel(i, j);

                    //        if (r != Color.FromArgb(255, 255, 255, 255) && r != Color.FromArgb(0, 0, 0, 0))
                    //            coloredp++;
                    //    }
                }));
            }
          //  double perc = coloredp / (double)(LocalBMP.Width * LocalBMP.Height) * 100;
           // Console.WriteLine(perc);
            Thread.Sleep(10);
        }
        /// <summary>
        /// Принимает значение от 0 до 5
        /// </summary>
        /// <param name="rt"></param>
        public void ChangeRender(int rt)
        {
            StopThread();
            switch (rt) {
                case 0:
                Drawer.SetRaster = 0;
                    break;
                case 1:
                Drawer.SetRaster = 1;
                    break;
                case 2:
                Drawer.SetRaster = 2;
                    break;
                case 4:
                Drawer = new DrawVisitorR(picture.Size, 1, camera);
                    break;
                case 5:
                Drawer = new DrawVisitorCamera(picture.Size, 1, camera);
                    break;
            }
            StartThread();
        }

        public void StartThread()
        {
            if (cancelTokenSource.IsCancellationRequested)
            {
                cancelTokenSource = new CancellationTokenSource();
                renderThread = new Thread(new ParameterizedThreadStart(RenderLoop));
                renderThread.Name = "drawing";
                renderThread.IsBackground = true;
                renderThread.Start(cancelTokenSource.Token);
            }
        }
        public void StopThread()
        {
         cancelTokenSource.Cancel();
        }
        public void Resize(Size s)
        {
            StopThread();
            Thread.Sleep(100);
            PictureBuff.Init(s);
            Drawer = new DrawVisitorCamera(picture.Size, 1, camera);
            Reader = new ReadVisitorCamera(camera, picture.Size, 1);
            camera.Aspect = s.Width / (double)s.Height;
            StartThread();
           
        }
       public Tuple<List<Id>, Id, PointComponent> Read(Point point, int what)
        {
            Reader.InPoint = point;
            model.action(Reader);
            PolygonComponent io = Reader.Find;
            PointComponent p = Reader.Findpoint;
            Tuple<List<Id>, Id, PointComponent> r = new Tuple<List<Id>, Id, PointComponent>(null, null, null);
            ModelComponent m = io;
            double dest = double.MaxValue;
            if (io != null)
            {
                if (l3 != null)
                    l3.Text = p.ToString();
                switch (what)
                {
                    case 1:
                        foreach (PointComponent po in io.Points)
                            if (po.Desctination(p) < dest)
                            {
                                dest = po.Desctination(p);
                                m = po;
                            }
                        Console.WriteLine(((PointComponent)m).Coords.ToString());
                        r = new Tuple<List<Id>, Id, PointComponent>(model.GetConnectedElements(m.Id), m.Id, p);
                        break;
                    case 2:
                        foreach (LineComponent po in io.Lines)
                            if (po.Desctination(p) < dest)
                            {
                                dest = po.Desctination(p);
                                m = po;
                            }
                        r = new Tuple<List<Id>, Id, PointComponent>(model.GetConnectedElements(m.Id), m.Id, p);
                        break;
                    case 3:
                        r = new Tuple<List<Id>, Id, PointComponent>(model.GetConnectedElements(io.Id), io.Id, p);
                        break;
                }   
            }
            return r;
        }
        #endregion

        #region model

        public void newCoords(Id id, MatrixCoord3D coords)
        {
            model.SetPointsCoord(id, coords);
        }
        public void NewP(Point point)
        {
            Reader.InPoint = point;
            model.action(Reader);
            PolygonComponent io = Reader.Find;
            PointComponent p = Reader.Findpoint;
            
            ModelComponent m = io;
            double dest = double.MaxValue;
            if (io != null)
            {
                foreach (LineComponent po in io.Lines)
                    if (po.Desctination(p) < dest)
                    {
                        dest = po.Desctination(p);
                        m = po;
                    }
                AddComponent(m.Id,p);
            }
        }

        public void SaveModel(ObjWriter w)
        {
            w.WriteModel(model);
        }
        List<PointComponent> pp = new List<PointComponent>();
        public Id NewPolygon(Point point)
        {
            Reader.InPoint = point;
            model.action(Reader);
            PolygonComponent io = Reader.Find;
            PointComponent p = Reader.Findpoint;

            ModelComponent m = io;
            double dest = double.MaxValue;
            if (io != null)
            {
                foreach (PointComponent po in io.Points)
                    if (po.Desctination(p) < dest)
                    {
                        dest = po.Desctination(p);
                        m = po;
                    }
                pp.Add((PointComponent)m);

                //if(pp.Count == 3)
                //{
                //    PolygonComponent pol = new PolygonComponent(pp[0], pp[1], pp[2]);
                //    pp.Clear();
                //    model.AddComponent(pol);
                //}
                return m.Id;
            }
            return null;
        }
        public void NewPolygon(PointComponent p)
        {
            model.LastPoint++;
            p.Id = new Id("Point", Convert.ToString(model.LastPoint));
            pp.Add(p);
           
        }

        public bool NewPolygon()
        {
            if (pp.Count == 3)
            {
                PolygonComponent pol = new PolygonComponent(pp[0], pp[1], pp[2]);
                pp.Clear();
                model.AddComponent(pol);
                return true;
            }
            
            return false;
        }

        public void RebildFigure()
        {
            model = new CubHash(new PointComponent(0, 0, 0), 20);
        }
        public void RebildFigure(ObjReader er)
        {
            model = er.ReadModel();
        }


        public void AddComponent(IObjects objects)
        {
        
            model.AddComponent(objects);
        }
        public void AddComponent(Id LineId, PointComponent point)
        {    
            model.AddPointToLine(LineId, point);
        }
        public void RemoveComponent(Id id)
        {
            StopThread();
           
            RemoveActiveComponent(id);
            model.RemovebyId(id);
            StartThread();
        }

        public bool AddComponentToActive(Id id)
        {
           return model.AddActiveComponent(id);
        }

        public void RemoveActiveComponent(Id id)
        {
            model.DeliteActive(id);
        }
        public void RemoveActiveComponent()
        {
            model.DeliteActive();
        }

        public void ModelAction(IVisitor action)
        {
          
            model.action(action);
        }

        public void ActiveMovement(string direction)
        {
            EasyTransformVisitor tr;
            MatrixTransformation3D trans = new MatrixTransformationTransfer3D(0, 0, 0);
            switch (direction)
            {
                case "south":
                    trans = new MatrixTransformationTransfer3D(-camera.Up.X, -camera.Up.Y, -camera.Up.Z);
                    break;
                case "north":
                    trans = new MatrixTransformationTransfer3D(camera.Up.X, camera.Up.Y, camera.Up.Z);
                    break;
                case "east":
                    trans = new MatrixTransformationTransfer3D(camera.Right.X, camera.Right.Y, camera.Right.Z);
                    break;
                case "west":
                    trans = new MatrixTransformationTransfer3D(-camera.Right.X, -camera.Right.Y, -camera.Right.Z);
                    break;
            }
            tr = new EasyTransformVisitor(trans);
            ModelAction(tr);
        }

        public void FromStack()
        {
         
        }
        #endregion

        #region camera
        public void Scale(int delta)
        {
            if (delta > 0)
            { Drawer.Scale++; Reader.Scale++; }
            else
            { Drawer.Scale--; Reader.Scale--; }
        }


        public void DropCam()
        {
            camera = new Camera(new PointComponent(0, 0, 300), new MatrixCoord3D(0, 0, 0), new MatrixCoord3D(0, 1, 0), new MatrixPerspectiveProjection(90, picture.Size.Width / (double)picture.Size.Height, 1, 1000));
            Drawer = new DrawVisitorCamera(picture.Size, 1, camera);
            Reader = new ReadVisitorCamera(camera, picture.Size, 1);
        }

        public void Move(int dir)
        {
            switch(dir)
            {
                case 'W':
                    camera.Move(CameraDirection.FORWARD, 5);
                    break;
                case 'S':
                    camera.Move(CameraDirection.BACKWARD, 5);
                    break;
                case 'A':
                    camera.Move(CameraDirection.LEFT, 5);
                    break;
                case 'D':
                    camera.Move(CameraDirection.RIGHT, 5);
                    break;
                case 'Q':
                    camera.Move(CameraDirection.UP, 5);
                    break;
                case 'E':
                    camera.Move(CameraDirection.DOWN, 5);
                    break;
                case 'C':
                    camera.Move(CameraDirection.YAW, 1);
                    break;
                case 'V':
                    camera.Move(CameraDirection.YAW, -1);
                    break;
                case 'G':
                    camera.Move(CameraDirection.PICH, 1);
                    break;
                case 'F':
                    camera.Move(CameraDirection.PICH, -1);
                    break;
                case 'Z':
                    camera.Move(CameraDirection.ROTATIONY, 2);
                    break;
                case 'X':
                    camera.Move(CameraDirection.ROTATIONY, -2);
                    break;

            }
        }

        #endregion


    }
}
