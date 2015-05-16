using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DragAndDockable {
    /// <summary>
    /// A special container for a control which can be moved or docked. Closing will only hide the form
    /// so it can be shown again from a view menu.
    /// </summary>
    public partial class DragAndDockControl : Form {
        private readonly DragAndDockBase m_dockBase;
        private bool m_mouseDown;
        private bool m_moving;
        private bool m_sizing;
        private bool m_docked;
        private const int WM_NCLBUTTONUP = 0xA2;
        private const int WM_ENTERSIZEMOVE = 0x0231;
        private const int WM_NCLBUTTONDWN = 0x00A1;
        private const int WM_NCLBUTTONDBLCLK = 0x00A3;
        private const int WM_EXITSIZEMOVE = 0x0232;
        private const int WM_MOVING = 0x0216;
        private const int WM_SIZING = 0x0214;

        public DragAndDockControl(Control control, DragAndDockBase dockBase) {
            InitializeComponent();
            m_dockBase = dockBase;
            control.Dock = DockStyle.Fill;
            this.Closing += DragAndDockable_Closing;
            this.Controls.Add(control);
        }

        protected void FormIsMoving() {
            m_moving = true;
            m_dockBase.CheckDockingPositions(this, false, true);
        }

        /// <summary>
        /// Done moving, checks for docking position.
        /// </summary>
        protected void DoneMoving() {
            bool dock = m_moving && !m_sizing;

            m_mouseDown = false;
            m_moving = false;
            m_sizing = false;
            if (dock) {
                m_docked = m_dockBase.CheckDockingPositions(this, true);
            }
        }

        /// <summary>
        /// Non-client mouse down. Undocks if cursor in correct position.
        /// </summary>
        protected void NCMouseDown() {
            if (m_docked && !m_sizing) {
                // Calculate the border size.
                //int borderWidth = (Width - ClientSize.Width) / 2;
                //int titlebarHeight = Height - ClientSize.Height - 2 * borderWidth;

                Point pos = PointToClient(Cursor.Position);

                if (pos.X > 0 && pos.X < ClientSize.Width && pos.Y < 0) {
                    m_docked = false;
                    m_dockBase.UndockWindow(this);
                }
            }

            // Make sure the parent form is visible.
            if (!m_mouseDown) {
                m_dockBase.BringParentToFront();
                this.BringToFront();
                m_mouseDown = true;
            }
        }

        protected override void WndProc(ref Message m) {
            try {
                switch (m.Msg) {
                    case WM_MOVING:
                        FormIsMoving();
                        break;
                    case WM_SIZING:
                        m_sizing = true;
                        break;
                    case WM_EXITSIZEMOVE:
                        DoneMoving();
                        break;
                    case WM_NCLBUTTONDWN:
                        NCMouseDown();
                        break;
                }

                base.WndProc(ref m);
            } catch (Exception ex) {
                // Error handling.
            }
        }

        void DragAndDockable_Closing(object sender, CancelEventArgs e) {
            try {
                e.Cancel = true;
                this.Hide();
            } catch (Exception ex) {
                //ErrorHandling.HandleException(ex);
            }
        }
    }
}
