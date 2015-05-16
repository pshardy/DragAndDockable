using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DragAndDockable {
    public partial class DragAndDockBase : UserControl {
        public const string CATEGORY_DRAG_DOCK = "Drag and Dock";

        [Category(CATEGORY_DRAG_DOCK)]
        [DisplayName("Containing Form")]
        [Description("The form which docking will occur against. Menustrips for this form will be adjusted.")]
        public Control ContainingForm { get; set; }
        [Category(CATEGORY_DRAG_DOCK)]
        public DockingInformation[] DockingPositions { get; set; }

        private DockingInformation m_highlightPosition;

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
            m_highlightPosition = null;
            this.Refresh();
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

        /// <summary>
        /// Checks if the form is in a position to dock.
        /// </summary>
        /// <param name="dockableForm">The form to dock.</param>
        /// <param name="dock">Docks the form if true.</param>
        /// <returns></returns>
        public bool CheckDockingPositions(DragAndDockControl dockableForm, bool dock = false, bool highlightPosition = false) {
            if (DockingPositions != null) {
                foreach (DockingInformation docking in DockingPositions) {
                    if (docking.PointInDock(PointToClient(Cursor.Position), ContainingForm)) {
                        if(highlightPosition)
                            HighlightPosition(docking);

                        if (dock)
                            DockWindow(dockableForm, docking.DockStyle);
                        return true;
                    }
                }
            }

            if (highlightPosition && m_highlightPosition != null) {
                m_highlightPosition = null;
                this.Refresh();
            }

            return false;
        }

        /// <summary>
        /// Signals the base to highlight the given docking position and signals a redraw of the control.
        /// </summary>
        /// <param name="information"></param>
        public void HighlightPosition(DockingInformation information) {
            information.BuildRect(this);
            if (m_highlightPosition == null || !m_highlightPosition.Equals(information)) {
                m_highlightPosition = information;
                this.Refresh();
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

        private void DragAndDockBase_Paint(object sender, PaintEventArgs e) {
            try {
                if (m_highlightPosition != null) {
                    e.Graphics.FillRectangle(SystemBrushes.Highlight, m_highlightPosition.Rectangle);
                }
            }
            catch (Exception ex) {
                // Handle exception.
            }
        }
    }

    [System.ComponentModel.TypeConverter(typeof(ExpandableObjectConverter))]
    public class DockingInformation {
        public Rectangle Rectangle;

        public DockingInformation() {
            Percentage = 0.25f;
        }

        /// <summary>
        /// Checks if the specified point for the given form is within this dock's bounds.
        /// This will also build the rectangle.
        /// </summary>
        /// <param name="p">The point to check (Mouse cursor relative to form.)</param>
        /// <param name="form">The form whose size will be used.</param>
        /// <returns>True if in the specified dock and percentage.</returns>
        public bool PointInDock(Point p, Control form) {
            if (form == null)
                return false;

            BuildRect(form);

            //return Rectangle.Contains(p);
            return p.X >= Rectangle.X && p.Y >= Rectangle.Y && p.X <= Rectangle.Width && p.Y <= Rectangle.Height;
        }

        public void BuildRect(Control form) {
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

            Rectangle = new Rectangle(minX, minY, maxX, maxY);
        }

        public override bool Equals(object obj) {
            if (obj == null) return false;
            if (obj.GetType() != typeof (DockingInformation))
                return false;

            return this == obj || Rectangle.Equals(((DockingInformation) obj).Rectangle);
        }

        public override int GetHashCode() {
            return Rectangle.GetHashCode();
        }

        public override string ToString() {
            return DockStyle.ToString() + " " + Percentage;
        }

        public DockStyle DockStyle { get; set; }
        public float Percentage { get; set; }
    }

}
