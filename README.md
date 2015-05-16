# DragAndDockable
.NET winform control for easy drag and drop of a form which can dock at different locations in a parent form. The dock location will highlight when a suitable control is dragged over it.

###Use
* Assign a DragAndDockableBase from the toolbox and size it (or dock it through Visual Studio) to a form.
* Under the Drag and Dock catagory you specify the containing form. If left blank the parent is automatically assumed.
* You can then add DockingPositions. Select the side and the percentage from that side which should trigger the dock.
For example, if you choose to dock on the right with a 0.25 percentage on a 100 pixel width form, then when the mouse is 25 pixels from the right (75 from the left) it will dock.
* Programmatically assign controls.
```
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

```
