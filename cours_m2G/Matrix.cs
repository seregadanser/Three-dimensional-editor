using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cours_m2G
{
    [Serializable]
    abstract class Matrix
    {
        protected int size;
        public int Size
        {
            get
            {
                return size;
            }
        }
    }
    [Serializable]
    class MatrixCoord3D : Matrix
    {
        readonly private double[] coords;
        public double X { get { return coords[0]; } set { coords[0] = value; } }
        public double Y { get { return coords[1]; } set { coords[1] = value; } }
        public double Z { get { return coords[2]; } set { coords[2] = value; } }
        public double W { get { return coords[3]; } set { coords[3] = value; } }
        public MatrixCoord3D()
        {
            coords = new double[4];
            coords[3] = 1;
            size = 4;
        }
        public MatrixCoord3D(double x, double y, double z)
        {
            coords = new double[4];
            coords[0] = x;
            coords[1] = y;
            coords[2] = z;
            coords[3] = 1;
            size = 4;
        }
        public MatrixCoord3D(int nulll)
        {
            coords = new double[4];
            coords[3] = 0;
            size = 4;
        }
        public static MatrixCoord3D operator *(MatrixCoord3D first, MatrixTransformation3D second)
        {
            MatrixCoord3D result = new MatrixCoord3D(0);
            for (int i = 0; i < second.Size; i++)
                for (int j = 0; j < second.Size; j++)
                {
                    result.coords[i] += first.coords[j] * second.Coeff[j, i];
                }
            return result;
        }
        public static MatrixCoord3D operator *(MatrixCoord3D first, MatrixCoord3D second)
        {
            MatrixCoord3D result = new MatrixCoord3D();
            result.coords[0] = first.Y * second.Z - first.Z * second.Y;
            result.coords[1] = first.Z * second.X - first.X * second.Z;
            result.coords[2] = first.X * second.Y - first.Y * second.X;
            return result;
        }

        public static MatrixCoord3D operator *(MatrixCoord3D first, double second)
        {
            MatrixCoord3D result = new MatrixCoord3D();
            result.coords[0] = first.X * second;
            result.coords[1] = first.Y * second;
            result.coords[2] = first.Z * second;
            return result;
        }

        public static MatrixCoord3D operator -(MatrixCoord3D first, MatrixCoord3D second)
        {
            MatrixCoord3D result = new MatrixCoord3D();
            result.X = first.X - second.X;
            result.Y = first.Y - second.Y;
            result.Z = first.Z - second.Z;
            return result;
        }
        public static MatrixCoord3D operator +(MatrixCoord3D first, MatrixCoord3D second)
        {
            MatrixCoord3D result = new MatrixCoord3D();
            result.X = first.X + second.X;
            result.Y = first.Y + second.Y;
            result.Z = first.Z + second.Z;
            return result;
        }

        public static bool operator ==(MatrixCoord3D first, double second)
        {
            if(double.IsNaN(first.X) || double.IsNaN(first.Y) || double.IsNaN(first.Z))
                return true;
            return false;
        }
        public static bool operator !=(MatrixCoord3D first, double second)
        {
            if (first == second)
                return false;
            return true;
        }

        public static double scalar(MatrixCoord3D first, MatrixCoord3D second)
        {
            return first.X * second.X + first.Y * second.Y + first.Z * second.Z;
        }

        public double Len()
        {
            return Math.Sqrt( X * X + Y * Y + Z * Z);
        }

        public void Normalise()
        {
            double L = Len();
            for (int i = 0; i < this.size-1; i++)
            {
                coords[i] /= L;
            }
        }

        public static MatrixCoord3D Inverse(MatrixCoord3D coord)
        {
            return new MatrixCoord3D(-1 * coord.X, -1 * coord.Y, -1 * coord.Z);
        }

        public override string ToString()
        {
            return "{"+Convert.ToString(X) + " " + Convert.ToString(Y) + " " + Convert.ToString(Z) + "}";
        }
    }


    [Serializable]
    class MatrixTransformation3D : Matrix
    {
        protected double[,] coeff;
        protected MatrixType type;
        public MatrixType Type { get { return type; } set { type = value; } }

        public double[,] Coeff
        {
            get
            {
                return coeff;
            }
            set
            {
                coeff = value;
            }
        }
        public MatrixTransformation3D()
        {
            coeff = new double[4, 4];
            size = 4;
        }

        public static MatrixTransformation3D operator *(MatrixTransformation3D first, MatrixTransformation3D second)
        {
            MatrixTransformation3D result = new MatrixTransformation3D();
            for (int i = 0; i < first.Size; i++)
            {
                for (int j = 0; j < first.Size; j++)
                {
                    for (int k = 0; k < first.Size; k++)
                    {
                        result.coeff[i, j] += first.coeff[i, k] * second.coeff[k, j];
                    }
                }
            }
            return result;
        }
        private double[,] One1(double[,] hhh, int h)
        {
            double[,] ggg = new double[h, h];

            for (int i = 0; i < h;)
            {
                for (int j = 0; j < h;)
                {
                    if (i == j)
                    { ggg[i, j] = 1; }
                    else
                    { ggg[i, j] = 0; }
                    j++;
                }
                i++;
            }

            double arg;
            int i1;


            for (int j = 0; j < h;)
            {
                for (int i = 0; i < h;)
                {
                    if (i == j)
                    { goto k; }
                    arg = hhh[i, j] / (double)hhh[j, j];
                    for (i1 = 0; i1 < h;)
                    {

                        hhh[i, i1] = hhh[i, i1] - hhh[j, i1] * arg;
                        ggg[i, i1] = ggg[i, i1] - ggg[j, i1] * arg;
                        i1++;
                    }
                k:
                    i++;
                }
                j++;
            }

            for (int j = 0; j < h;)
            {
                for (int i = 0; i < h;)
                {
                    double arg_2;
                    if (i == j)
                    {
                        arg_2 = hhh[i, j];
                        for (i1 = 0; i1 < h;)
                        {
                            hhh[i, i1] = hhh[i, i1] / (double)arg_2;
                            ggg[i, i1] = ggg[i, i1] / (double)arg_2;
                            i1++;
                        }


                    }
                    i++;
                }
                j++;
            }
            return ggg;
        }
        public MatrixTransformation3D InversedMatrix()
        {
            MatrixTransformation3D m = new MatrixTransformation3D();
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    m.Coeff[i, j] = Coeff[i, j];

            inversion(m.Coeff, 4);

            return m;
        }
        private void inversion(double[,] A, int N)
        {
            double temp;

            double[,] E = new double[4, 4];

            for (int i = 0; i < N; i++)
                for (int j = 0; j < N; j++)
                {
                    E[i, j] = 0.0;
                    if (i == j)
                        E[i, j] = 1.0;
                }

            for (int k = 0; k < N; k++)
            {
                temp = A[k, k];

                for (int j = 0; j < N; j++)
                {
                    A[k, j] /= temp;
                    E[k, j] /= temp;
                }

                for (int i = k + 1; i < N; i++)
                {
                    temp = A[i, k];

                    for (int j = 0; j < N; j++)
                    {
                        A[i, j] -= A[k, j] * temp;
                        E[i, j] -= E[k, j] * temp;
                    }
                }
            }

            for (int k = N - 1; k > 0; k--)
            {
                for (int i = k - 1; i >= 0; i--)
                {
                    temp = A[i, k];

                    for (int j = 0; j < N; j++)
                    {
                        A[i, j] -= A[k, j] * temp;
                        E[i, j] -= E[k, j] * temp;
                    }
                }
            }

            for (int i = 0; i < N; i++)
                for (int j = 0; j < N; j++)
                    A[i, j] = E[i, j];
        }



    }
    [Serializable]
    class MatrixTransformationTransfer3D : MatrixTransformation3D
    {
        public MatrixTransformationTransfer3D(double dx, double dy, double dz)
        {
            coeff = new double[4, 4];
            size = 4;
            coeff[0, 0] = 1;
            coeff[1, 1] = 1;
            coeff[2, 2] = 1;
            coeff[3, 3] = 1;
            coeff[3, 0] = dx;
            coeff[3, 1] = dy;
            coeff[3, 2] = dz;
        }
    }
    [Serializable]
    class MatrixTransformationScale3D : MatrixTransformation3D
    {
        public MatrixTransformationScale3D(double kx, double ky, double kz)
        {
            coeff = new double[4, 4];
            size = 4;
            coeff[0, 0] = kx;
            coeff[1, 1] = ky;
            coeff[2, 2] = kz;
            coeff[3, 3] = 1;
        }
    }
    [Serializable]
    class MatrixTransformationRotateX3D : MatrixTransformation3D
    {
        public MatrixTransformationRotateX3D(int angle)
        {
            coeff = new double[4, 4];
            size = 4;
            coeff[0, 0] = 1;
            coeff[1, 1] = Math.Cos(angle * Math.PI / 180);
            coeff[2, 2] = Math.Cos(angle * Math.PI / 180);
            coeff[3, 3] = 1;
            coeff[1, 2] = Math.Sin(angle * Math.PI / 180);
            coeff[2, 1] = -Math.Sin(angle * Math.PI / 180);
        }
    }
    [Serializable]
    class MatrixTransformationRotateY3D : MatrixTransformation3D
    {
        public MatrixTransformationRotateY3D(int angle)
        {
            coeff = new double[4, 4];
            size = 4;
            coeff[0, 0] = Math.Cos(angle * Math.PI / 180);
            coeff[1, 1] = 1;
            coeff[2, 2] = Math.Cos(angle * Math.PI / 180);
            coeff[3, 3] = 1;
            coeff[2, 0] = Math.Sin(angle * Math.PI / 180);
            coeff[0, 2] = -Math.Sin(angle * Math.PI / 180);
        }
    }
    [Serializable]
    class MatrixTransformationRotateZ3D : MatrixTransformation3D
    {
        public MatrixTransformationRotateZ3D(int angle)
        {
            coeff = new double[4, 4];
            size = 4;
            coeff[0, 0] = Math.Cos(angle * Math.PI / 180);
            coeff[1, 1] = Math.Cos(angle * Math.PI / 180);
            coeff[2, 2] = 1;
            coeff[3, 3] = 1;
            coeff[0, 1] = Math.Sin(angle * Math.PI / 180);
            coeff[1, 0] = -Math.Sin(angle * Math.PI / 180);
        }
    }
    [Serializable]
    class MatrixTransformationRotateVec3D : MatrixTransformation3D
    {
        public MatrixTransformationRotateVec3D(MatrixCoord3D vec,int angle)
        {
            coeff = new double[4, 4];
            size = 4;
            double radians = angle * Math.PI / 180;
            double cos = Math.Cos(radians), sin = Math.Sin(radians);
            double VV = 1 - cos;
            coeff[0, 0] = cos + VV* vec.X * vec.X; 
            coeff[0, 1] = VV * vec.X * vec.Y - sin * vec.Z; 
            coeff[0, 2] = VV * vec.X*vec.Z + sin*vec.Y;
            coeff[1, 0] = vec.X * vec.Y * VV + sin * vec.Z;
            coeff[1, 1] = cos + vec.Y * vec.Y * VV;
            coeff[1, 2] = vec.Z * vec.Y * VV-vec.X*sin;
            coeff[2, 0] = vec.Z * vec.X * VV - sin *vec.Y;
            coeff[2, 1] = sin * vec.X + vec.Z * vec.Y * VV;
            coeff[2, 2] = cos + vec.Z * vec.Z * VV; ;
            coeff[3, 3] = 1;
      
        }
    }
    [Serializable]
    class MatrixAuxiliary : MatrixTransformation3D
    {
        public MatrixAuxiliary(MatrixCoord3D c1, MatrixCoord3D c2, MatrixCoord3D c3)
        {
            coeff = new double[4, 4];
            size = 4;
            coeff[3, 3] = 1;
            coeff[0, 0] = c1.X;
            coeff[1, 0] = c1.Y;
            coeff[2, 0] = c1.Z;
            coeff[0, 1] = c2.X;
            coeff[1, 1] = c2.Y;
            coeff[2, 1] = c2.Z;
            coeff[0, 2] = c3.X;
            coeff[1, 2] = c3.Y;
            coeff[2, 2] = c3.Z;
        }
        public MatrixAuxiliary(MatrixCoord3D c0, MatrixCoord3D c1, MatrixCoord3D c2, MatrixCoord3D c3)
        {
            coeff = new double[4, 4];
            size = 4;
            coeff[3, 3] = 1;
            coeff[2, 2] = 1;
            coeff[1, 1] = 1;
            coeff[0, 0] = 1;
            coeff[3, 0] = -MatrixCoord3D.scalar(c0, c1);
            coeff[3, 1] = -MatrixCoord3D.scalar(c0, c2);
            coeff[3, 2] = -MatrixCoord3D.scalar(c0, c3);
        }
        public MatrixAuxiliary(MatrixCoord3D c0)
        {
            coeff = new double[4, 4];
            size = 4;
            coeff[3, 3] = 1;
            coeff[2, 2] = 1;
            coeff[1, 1] = 1;
            coeff[0, 0] = 1;
            coeff[3, 0] = -c0.X;
            coeff[3, 1] = -c0.Y;
            coeff[3, 2] = -c0.Z;
        }
    }
    [Serializable]
    abstract class MatrixProjection : MatrixTransformation3D
    {
        protected double fovy, aspect, n, f;
        public double Fovy { get { return fovy; } set { fovy = value; } }
        public double Aspect { get { return aspect; } set { aspect = value; } }
        public double N { get { return n; } set { n = value; } }
        public double F { get { return f; } set { f = value; } }


        protected double xmin, xmax, ymin, ymax, zmin, zmax;

        public double Xmin { get { return xmin; } set { xmin = value; } }
        public double Xmax { get { return xmax; } set { xmax = value; } }
        public double Ymin { get { return ymin; } set { ymin = value; } }
        public double Ymax { get { return ymax; } set { ymax = value; } }
        public double Zmax { get { return zmax; } set { zmax = value; } }
        public double Zmin { get { return zmin; } set { zmin = value; } }

    }



    [Serializable]
    class MatrixPerspectiveProjection : MatrixProjection
    {
        public MatrixPerspectiveProjection(double fovy, double aspect, double n, double f)
        {
            this.fovy = fovy;
            this.aspect = aspect;
            this.n = n;
            this.f =f ;
            coeff = new double[4, 4];
            size = 4;
            type = MatrixType.Perspective;

            double radians = Math.PI / 180 * fovy;
            double sx = (1 / Math.Tan(radians / 2)) / aspect;
            double sy = (1 / Math.Tan(radians / 2));
            double sz = (f + n) / (f - n);
            double dz = (-2 * f * n) / (f - n);

            coeff[0, 0] = sx;
            coeff[1, 1] = sy;
            coeff[2, 2] = sz;
            coeff[2, 3] = -1;
            coeff[3, 2] = dz;
        }

    }
    [Serializable]
    class MatrixOrtoProjection : MatrixProjection
    {
        public MatrixOrtoProjection(double xmin, double xmax, double ymin, double ymax, double zmin, double zmax)
        {
            coeff = new double[4, 4];
            size = 4;
            type = MatrixType.Orto;


            coeff[0, 0] = 2/(xmax - xmin);
            coeff[1, 1] = 2/(ymax - ymin);
            coeff[2, 2] = -2/(zmax - zmin);
            coeff[3, 3] = 1;
            coeff[3, 0] = -(xmax+xmin)/(xmax-xmin);
            coeff[3, 1] = -(ymax+ymin)/(ymax-ymin);
            coeff[3, 2] = -(zmax+zmin)/(zmax-zmin);
        }
    }
}
