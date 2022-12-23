using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace cours_m2G
{
    delegate void ActiveD(IVisitor I);
    partial class FormTransform : Form
    {
        ActiveD action;
        public IVisitor transformvisitor = new EasyTransformVisitor(new MatrixTransformationTransfer3D(0, 0, 0)); 
        public FormTransform(ActiveD action)
        {
            InitializeComponent();
            this.action = action;
        }

        private void FormTransform_Load(object sender, EventArgs e)
        {

        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            panel1.Visible = true;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            panel1.Visible = false;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            panel1.Visible = false;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            panel1.Visible = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MatrixTransformation3D transform = new MatrixTransformationTransfer3D(0,0,0);
            switch (tabControl1.SelectedIndex)
            {
                case 0:
                   transform = new MatrixTransformationTransfer3D(Convert.ToDouble(numericUpDown1.Value), Convert.ToDouble(numericUpDown2.Value), Convert.ToDouble(numericUpDown3.Value));
                    break;
                case 1:
                    transform = new MatrixTransformationScale3D(Convert.ToDouble(numericUpDown6.Value), Convert.ToDouble(numericUpDown5.Value), Convert.ToDouble(numericUpDown4.Value));
                    break;
                case 2:
                    if (radioButton1.Checked)
                    {
                        transform = new MatrixTransformationRotateX3D(Convert.ToInt32(numericUpDown7.Value));
                    }
                    if (radioButton2.Checked)
                    {
                        transform = new MatrixTransformationRotateY3D(Convert.ToInt32(numericUpDown7.Value));
                    }
                    if (radioButton3.Checked)
                    {
                        transform = new MatrixTransformationRotateZ3D(Convert.ToInt32(numericUpDown7.Value));
                    }
                    if (radioButton4.Checked)
                    {
                        transform = new MatrixTransformationRotateVec3D(new MatrixCoord3D(Convert.ToDouble(numericUpDown8.Value), Convert.ToDouble(numericUpDown9.Value), Convert.ToDouble(numericUpDown10.Value)), Convert.ToInt32(numericUpDown7.Value));
                    }
                    break;
            }
            if (checkBox1.Checked)
            {
             transformvisitor = new HardTransformVisitor(transform,new PointComponent(Convert.ToDouble(numericUpDown13.Value), Convert.ToDouble(numericUpDown11.Value), Convert.ToDouble(numericUpDown12.Value)));
            }
            else
            {
              transformvisitor =  new EasyTransformVisitor(transform);
            }
            action.Invoke(transformvisitor);
        }

        private void numericUpDown13_ValueChanged(object sender, EventArgs e)
        {

        }

        private void numericUpDown8_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
