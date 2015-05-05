using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SampleApplication {
    public partial class Form1 : Form {
        private DragAndDockable.DragAndDockable m_listControl;

        public Form1() {
            InitializeComponent();
            ListBox listBox = new ListBox();
            m_listControl = new DragAndDockable.DragAndDockable(listBox, dragAndDockBase1) {Size = new Size(200, 400)};
            m_listControl.Show();
        }
    }
}
