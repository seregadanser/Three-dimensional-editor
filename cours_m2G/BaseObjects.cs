using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace cours_m2G
{
    interface IBaseObjects : IObjects
    {

    }



    class Axes : IBaseObjects
    {
        public Color Color { get; set; } = Color.Black;
        public Id Id { get; set; }
        readonly LineComponent XAxis, YAxis, ZAxis;
        PointComponent[] point;
        public Axes()
        {
            point = new PointComponent[6];

            point[0] = new PointComponent(0, 100, 0);
            point[1] = new PointComponent(0, -100, 0);
            point[2] = new PointComponent(100, 0, 0);
            point[3] = new PointComponent(-100, 0, 0);
            point[4] = new PointComponent(0, 0, 100);
            point[5] = new PointComponent(0, 0, -100);

            XAxis = new LineComponent(point[2], point[3]);
            YAxis = new LineComponent(point[0], point[1]);
            ZAxis = new LineComponent(point[4], point[5]);
            XAxis.Color = Color.Red;
            YAxis.Color = Color.Green;
            ZAxis.Color = Color.Blue;
        }
        public void action(IVisitor visitor)
        {
            XAxis.action(visitor);
            YAxis.action(visitor);
            ZAxis.action(visitor);
        }
        public IObjects Clone()
        {
            return this;
        }

    }
    [Serializable]
    class Camera : IBaseObjects
    {
        public Color Color { get; set; } = Color.Black;
        public Id Id { get; set; }
        public PointComponent Position;
        public MatrixCoord3D Target;
        public MatrixCoord3D Direction;
        protected MatrixCoord3D up;
        protected MatrixCoord3D right;
        public MatrixCoord3D Up { get { return up; } }
        public MatrixCoord3D Right { get { return right; } }

        private MatrixTransformation3D RotationMatrix;
        public MatrixTransformation3D LookAt { get; set; }

        private MatrixProjection projection;
        private double fovy;
        private double aspect;

        public MatrixProjection Projection { get { return projection; } set { projection = value; } }
        public MatrixTransformation3D RotateMatrix { get { return RotationMatrix; } }

        public double Fovy
        {
            get { return fovy; }
            set
            {
                fovy = value;
                if (projection.Type == MatrixType.Perspective)
                    projection = new MatrixPerspectiveProjection(fovy, projection.Aspect, projection.N, projection.F);
            }
        }
        public double Aspect
        {
            get { return aspect; }
            set
            {
                aspect = value;
                if (projection.Type == MatrixType.Perspective)
                    projection = new MatrixPerspectiveProjection(projection.Fovy, aspect, projection.N, projection.F);
            }
        }
      
        public Camera(PointComponent position, MatrixCoord3D Target, MatrixCoord3D Up, MatrixProjection projection)
        {
            this.Position = position;
            this.Target = Target;
            this.up = Up;

            Direction = Position.Coords - Target;
            Direction.Normalise();
            right = Up * Direction;
            right.Normalise();
            up = (Direction * right);
            up.Normalise();
            RotationMatrix = new MatrixAuxiliary(right, up, Direction);
            LookAt = RotationMatrix * new MatrixAuxiliary(Position.Coords, right, up, Direction);
            aspect = projection.Aspect;
            fovy = projection.Fovy;
            this.projection = projection;

        }

        public void action(IVisitor visitor)
        {
            Position.action(visitor);
            PointComponent D = new PointComponent(Position.Coords + Direction * 30);

            PointComponent R = new PointComponent(Position.Coords + Right * 30);

            PointComponent U = new PointComponent(Position.Coords + Up * 30);
            LineComponent LD = new LineComponent(Position, D);
            LD.Color = Color.Orange;
            LineComponent LR = new LineComponent(Position, R);
            LR.Color = Color.DarkGreen;
            LineComponent LU = new LineComponent(Position, U);
            LU.Color = Color.BlueViolet;
            LD.action(visitor);
            LR.action(visitor);
            LU.action(visitor);
            //CountParams();
        }

        public bool IsVieved(MatrixCoord3D point)
        {
            // if(point.Len()> Position.Coords.Len())
            {
                if (MatrixCoord3D.scalar(Position.Coords - point, Direction) < 0)
                    return false;
            }
            return true;
        }

        public void Move(CameraDirection dir, double speed)
        {
            MatrixTransformation3D rotation;
            switch (dir)
            {
             
                case CameraDirection.FORWARD:
                    Position.Coords -= (Direction * speed);
                    break;
                case CameraDirection.RIGHT:
                    Position.Coords += (Right * (speed));
                    break;
                case CameraDirection.UP:
                    Position.Coords += (Up * (speed));
                    break;
                case CameraDirection.BACKWARD:
                    Position.Coords += (Direction * speed);
                    break;
                case CameraDirection.LEFT:
                    Position.Coords -= (Right * (speed));
                    break;
                case CameraDirection.DOWN:
                    Position.Coords -= (Up * (speed));
                    break;
                case CameraDirection.YAW:
                    //вращение вокруг своей оси по Y
                    rotation = new MatrixTransformationRotateVec3D(Up,(int)speed);
                    Direction *= rotation;
                    right *= rotation;
                    Direction.Normalise();
                    right.Normalise();
                    RotationMatrix = new MatrixAuxiliary(Right, Up, Direction);
                    break;
                case CameraDirection.PICH:
                    //вращение вокруг своей оси по X
                    rotation = new MatrixTransformationRotateVec3D(Right,(int)speed);
                    Direction *= rotation;
                    up *= rotation;
                    Direction.Normalise();
                    up.Normalise();
                    RotationMatrix = new MatrixAuxiliary(Right, Up, Direction);
                    break;
        
        

                case CameraDirection.ROTATIONY:
                    ////вращение по вокруг y относительно  0 0 0
                    MatrixTransformation3D r = new MatrixTransformationRotateY3D((int)speed);
                    Position.Coords = Position.Coords * r;
                    Direction = Direction * r;
                    right = Right * r;
                    up *= r;
                    Direction.Normalise();
                    up.Normalise();
                    right.Normalise();
                    RotationMatrix = new MatrixAuxiliary(Right, Up, Direction);
                    break;

                    //  case 10:
                    ////вращение по вокруг y относительно  задаваемой точки
                    //MatrixTransformation3D transfer3 = new MatrixTransformationTransfer3D(-100, -0, -0);//vfnhbwf gthtyjcf
                    //MatrixTransformation3D transfer4 = new MatrixTransformationTransfer3D(100, 0, 0);//vfnhbwf gthtyjcf
                    //MatrixTransformation3D r1t = new MatrixTransformationRotateY3D(-(int)speed);
                    //Position.Coords = Position.Coords * transfer3;
                    //Position.Coords = Position.Coords * r1t;
                    //Position.Coords = Position.Coords * transfer4;

                    //Direction = Direction * r1t;
                    //Right = Right * r1t;

                    //RotationMatrix = new MatrixAuxiliary(Right, Up, Direction);



                    //   break;

            }
            CountLookAt();
        }
        private void CountLookAt()
        {

            LookAt = RotationMatrix * new MatrixAuxiliary(Position.Coords, Right, Up, Direction);
        }

        public IObjects Clone()
        {
            return this;
        }

    }


    class Pyramide : IBaseObjects
    {
        public Color Color { get; set; } = Color.Black;
        public Id Id { get; set; }
        LineComponent[] lines;
        public Pyramide()
        {
            PointComponent[] point = new PointComponent[8];
            lines = new LineComponent[12];
            point[0] = new PointComponent(-5, 10, 5);
            point[1] = new PointComponent(-5, 10, -5);
            point[2] = new PointComponent(5, 10, -5);
            point[3] = new PointComponent(5, 10, 5);
            point[4] = new PointComponent(-10, -10, 10);
            point[5] = new PointComponent(-10, -10, -10);
            point[6] = new PointComponent(10, -10, -10);
            point[7] = new PointComponent(10, -10, 10);

            lines[0] = new LineComponent(point[0], point[1]);
            lines[1] = new LineComponent(point[1], point[2]);
            lines[2] = new LineComponent(point[2], point[3]);
            lines[3] = new LineComponent(point[3], point[0]);

            lines[4] = new LineComponent(point[4], point[5]);
            lines[5] = new LineComponent(point[5], point[6]);
            lines[6] = new LineComponent(point[6], point[7]);
            lines[7] = new LineComponent(point[7], point[4]);

            lines[8] = new LineComponent(point[0], point[4]);
            lines[9] = new LineComponent(point[1], point[5]);
            lines[10] = new LineComponent(point[2], point[6]);
            lines[11] = new LineComponent(point[3], point[7]);

        }
        public void action(IVisitor visitor)
        {
            foreach (LineComponent line in lines)
            {
                line.action(visitor);
            }
        }
        public IObjects Clone()
        {
            return this;
        }
    }

    public static class ObjectCopier
    {
        public static T Clone<T>(T source)
        {
            if (ReferenceEquals(source, null)) return default;

            using Stream stream = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, source);
            stream.Seek(0, SeekOrigin.Begin);
            return (T)formatter.Deserialize(stream);
        }
    }
}
