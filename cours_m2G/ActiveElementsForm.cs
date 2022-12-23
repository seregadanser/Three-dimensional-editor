using cours_m2G;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;
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
    delegate void Action(Id id);
    delegate void ActionEmpty();
    delegate void calback(Id id);

    struct CallBackDelegates
    {
       public Action remove_active, remove_object, newcoords, invnormal;
        public ActionEmpty close;
    }

    partial class ActiveElementsForm : Form
    {
        List<Elem> elements;
        List<Id> elem_id;
        CallBackDelegates delegates;
          public ActiveElementsForm(CallBackDelegates delegates)
        {
            InitializeComponent();
            elements = new List<Elem>();
            elem_id = new List<Id>();
            this.delegates = delegates;
        }

        private void ActiveElementsForm_Load(object sender, EventArgs e)
        {

        }

        private void Update1(Id id)
        {
            for (int i = 0; i < elements.Count; i++)
                if (elements[i].id == id)
                {
                    TableLayoutHelper.RemoveArbitraryRow(tableLayoutPanel1, i);
                    elements.RemoveAt(i);
                    elem_id.RemoveAt(i);
                    return;
                }
        }

        public void Update()
        {
            while (tableLayoutPanel1.Controls.Count > 0)
            {
                tableLayoutPanel1.Controls[0].Dispose();
            }
            elements.Clear();
            foreach(Id i in elem_id)
            elements.Add(new Elem(i, elements.Count, tableLayoutPanel1, delegates, new calback(Update1)));
        }

        private void ActiveElementsForm_FormClosed(object sender, FormClosedEventArgs e)
        {
      
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        public bool AddActive(Id id)
        {
            tableLayoutPanel1.RowCount++;
            elem_id.Add(id);
            elements.Add(new Elem(id, elements.Count, tableLayoutPanel1,delegates,new calback(Update1)));
            return true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            delegates.close.Invoke();
        }
    }
    class Elem
    {
        CallBackDelegates delegates;
        calback dele;
        Label name;
        Button rem;
        Button del;
        Button add;
        Button inv;
        public Id id;

        public Elem(Id id, int num, TableLayoutPanel main, CallBackDelegates dell, calback dell1)
        {
            this.id = id;
            dele = dell1;
            delegates = dell;
            name = new Label();
            name.AutoSize = true;
            name.Text = id.ToString();
            rem = new Button();
            rem.Click += new EventHandler(RemClic);
            rem.AutoSize = true;
            rem.Text = "Remove from Model";
            del = new Button();
            del.AutoSize = true;
            del.Text = "Delit from Active";
            del.Click += new EventHandler(DelClic);
            main.Controls.Add(name, 0, num);
            main.Controls.Add(rem, 1, num);
            main.Controls.Add(del, 2, num);
        }

        private void RemClic(object sender, EventArgs e)
        {
            delegates.remove_object.Invoke(id);
            dele.Invoke(id);
        }
        private void DelClic(object sender, EventArgs e)
        {
            delegates.remove_active.Invoke(id);
            dele.Invoke(id);
        }
        private void NewPointClick(object sender, EventArgs e)
        {
         delegates.newcoords.Invoke(id);
        }

    }


    static class TableLayoutHelper
    {
        public static void RemoveArbitraryRow(TableLayoutPanel panel, int rowIndex)
        {
            if (rowIndex >= panel.RowCount)
            {
                return;
            }

            // delete all controls of row that we want to delete
            for (int i = 0; i < panel.ColumnCount; i++)
            {
                var control = panel.GetControlFromPosition(i, rowIndex);
                panel.Controls.Remove(control);
            }

            // move up row controls that comes after row we want to remove
            for (int i = rowIndex + 1; i < panel.RowCount; i++)
            {
                for (int j = 0; j < panel.ColumnCount; j++)
                {
                    var control = panel.GetControlFromPosition(j, i);
                    if (control != null)
                    {
                        panel.SetRow(control, i - 1);
                    }
                }
            }

            var removeStyle = panel.RowCount - 1;

            if (panel.RowStyles.Count > removeStyle)
                panel.RowStyles.RemoveAt(removeStyle);

            panel.RowCount--;
        }
    }
}