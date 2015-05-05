using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DragAndDockable {
    /// <summary>
    /// A special container for a control which can be moved or docked. Closing will only hide the form
    /// so it can be shown again from a view menu.
    /// </summary>
    public partial class DragAndDockable : Form {
        private readonly DragAndDockBase m_dockBase;
        private Point m_lastPosition;
        private bool m_mouseDown;
        private const int WM_NCLBUTTONUP = 0xA2;
        private const int WM_NCLBUTTONDWN = 0xA1;
        private const int WM_EXITSIZEMOVE = 0x0232;

        public DragAndDockable(Control control, DragAndDockBase dockBase) {
            InitializeComponent();
            m_dockBase = dockBase;
            control.Dock = DockStyle.Fill;
            this.Closing += DragAndDockable_Closing;
            this.Controls.Add(control);
        }

        private bool HasMoved() {
            if (!m_lastPosition.Equals(this.Location)) {
                m_lastPosition = this.Location;
                return true;
            }

            return false;
        }

        protected override void WndProc(ref Message m) {
            try {
                switch (m.Msg) {
                    case WM_EXITSIZEMOVE:
                        m_dockBase.CheckDockingPositions(this);
                        m_mouseDown = false;
                        break;
                    case WM_NCLBUTTONDWN:
                        m_dockBase.UndockWindow(this);
                        // Make sure the parent form is visible.
                        if (!m_mouseDown) {
                            m_dockBase.BringParentToFront();
                            this.BringToFront();
                            m_mouseDown = true;
                        }

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
