using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DragAndDockable {
    [System.ComponentModel.TypeConverter(typeof(ExpandableObjectConverter))]
    public class DockingInformation {
        public DockingInformation() {
            Percentage = 0.25f;
        }

        /// <summary>
        /// Checks if the specified point for the given form is within this dock's bounds.
        /// </summary>
        /// <param name="p">The point to check (Mouse cursor relative to form.)</param>
        /// <param name="c">The control which is being checked with the point.</param>
        /// <param name="form">The form whose size will be used.</param>
        /// <returns>True if in the specified dock and percentage.</returns>
        public bool PointInDock(Point p, Control c, Control form) {
            if (form == null)
                return false;

            int minX = 0;
            int minY = 0;
            int maxX = form.Size.Width;
            int maxY = form.Size.Height;

            switch (DockStyle) {
                case DockStyle.Left:
                    maxX = (int)(form.Size.Width * Percentage);
                    break;
                case DockStyle.Right:
                    minX = form.Size.Width - (int)(form.Size.Width * Percentage);
                    break;
                case DockStyle.Top:
                    maxY = (int)(form.Size.Height * Percentage);
                    break;
                case DockStyle.Bottom:
                    minY = form.Size.Height - (int)(form.Size.Height * Percentage);
                    break;
            }

            return p.X >= minX && p.Y >= minY && p.X <= maxX && p.Y <= maxY;
        }

        public override string ToString() {
            return DockStyle.ToString() + " " + Percentage;
        }

        public DockStyle DockStyle { get; set; }
        public float Percentage { get; set; }
    }

    public partial class DragAndDockBase : UserControl {
        public const string CATEGORY_DRAG_DOCK = "Drag and Dock";

        [Category(CATEGORY_DRAG_DOCK)]
        [DisplayName("Containing Form")]
        [Description("The form which docking will occur against. Menustrips for this form will be adjusted.")]
        public Control ContainingForm { get; set; }
        [Category(CATEGORY_DRAG_DOCK)]
        public DockingInformation[] DockingPositions { get; set; }

        public DragAndDockBase() {
            InitializeComponent();

            if (ContainingForm == null)
                ContainingForm = this.Parent;
        }

        /// <summary>
        /// Docks a window to the specified side of the parent form.
        /// </summary>
        /// <param name="window"></param>
        /// <param name="style"></param>
        private void DockWindow(Form window, DockStyle style) {
            window.TopLevel = false;
            ContainingForm.Controls.Add(window);
            window.Dock = style;
            BringMenuStripsToFront();
        }

        /// <summary>
        /// Undocks a window and places it top border centered on the cursor.
        /// </summary>
        /// <param name="window"></param>
        public void UndockWindow(Form window) {
            if (window.Dock != DockStyle.None) {
                ContainingForm.Controls.Remove(window);
                window.TopLevel = true;
                window.Dock = DockStyle.None;

                // Calculate the border size.
                int borderWidth = (window.Width - window.ClientSize.Width) /2 ;
                int titlebarHeight = window.Height - window.ClientSize.Height - 2 * borderWidth;

                // Place the window with the cursor at the center of the top border.
                window.Location = new Point(Cursor.Position.X - window.Size.Width / 2, Cursor.Position.Y - titlebarHeight / 2);
            }
        }

        public void CheckDockingPositions(DragAndDockable dockableForm) {
            if (DockingPositions != null) {
                foreach (DockingInformation docking in DockingPositions) {
                    if (docking.PointInDock(PointToClient(Cursor.Position), dockableForm, ContainingForm)) {
                        DockWindow(dockableForm, docking.DockStyle);
                    }
                }
            }
        }

        public void BringParentToFront() {
            if (ContainingForm != null)
                ContainingForm.BringToFront();
        }

        /// <summary>
        /// Finds all menu strips, removes them, then adds them again so they will dock correctly.
        /// </summary>
        protected void BringMenuStripsToFront() {
            List<Control> menuControls = new List<Control>();

            // Build list of menu controls and remove from containing form.
            for (int i = 0; i < ContainingForm.Controls.Count; ) {
                Control c = ContainingForm.Controls[i];
                Type t = c.GetType();
                if (t == typeof(MenuStrip) || t.IsSubclassOf(typeof(MenuStrip))) {
                    menuControls.Add(c);
                    ContainingForm.Controls.RemoveAt(i);
                } else {
                    i++;
                }
            }

            // Add the controls back so they will be correctly formatted.
            foreach (Control c in menuControls) {
                ContainingForm.Controls.Add(c);
            }
        }
    }
}
