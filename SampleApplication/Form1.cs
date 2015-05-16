using System.Drawing;
using System.Windows.Forms;
using DragAndDockable;

namespace SampleApplication {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
            ListBox listBox = new ListBox();
            TextBox textbox = new TextBox();
            DragAndDockControl textControl = new DragAndDockControl(textbox, dragAndDockBase1) { Size = new Size(150, 200) };
            DragAndDockControl listControl = new DragAndDockControl(listBox, dragAndDockBase1) { Size = new Size(200, 400) };
            textControl.Show();
            listControl.Show();
        }
    }
}
