using System.Diagnostics;

namespace SpoofDPI_GUI
{
    public partial class mainWindow : Form
    {
        WindowHighlight highlight;
        bool validProc;

        public mainWindow()
        {
            InitializeComponent();
            highlight = new WindowHighlight();
            procListUpdateTimer.Start();
        }

        private void SetScaleFactorSlider(int ScaleFactor = 100)
        {
            scaleFactorGroupBox.Enabled = validProc;
            int index = Spoof.scaleFactorArray.ToList().IndexOf(ScaleFactor);
            scaleFactorTrackBar.Value = (index != -1) ? index : 0;
            UpdateScaleFactorSliderLabel();
        }

        private void UpdateScaleFactorSliderLabel()
        {
            Spoof.CurrScaleFactor = Spoof.scaleFactorArray[scaleFactorTrackBar.Value];
            scaleFactorLabel.Text = Spoof.CurrScaleFactor.ToString();
        }

        private void UpdateToggleButton(bool enable)
        {
            toggleButton.Text = enable ? "Enable" : "Disable";
        }

        private void DisplayInfoByCurrProc()
        {
            selectDragImage.Image = Spoof.CurrProcInfo.Icon;
            procNameLabel.Text = Spoof.CurrProcInfo.Name;
            highlight.Location = Spoof.CurrProcInfo.WindowPos;
            highlight.Size = Spoof.CurrProcInfo.WindowSize;
        }

        private void UpdateProcInfo(Spoof.ProcInfo procInfo)
        {
            const int enabledGroupIndex = 0;
            const int disabledGroupIndex = 1;

            const int scaleFactorIndex = 2;

            ListViewGroup group = (procInfo.HookEnabled) ? procListView.Groups[enabledGroupIndex] : procListView.Groups[disabledGroupIndex];

            if (procListView.Items.ContainsKey(procInfo.Id.ToString()))
            {
                ListViewItem item = procListView.Items[procInfo.Id.ToString()];
                item.SubItems[scaleFactorIndex].Text = procInfo.ScaleFactor.ToString();
                item.Group = group;
            }
            else
            {
                string[] row = { procInfo.Name, procInfo.Id.ToString(), procInfo.ScaleFactor.ToString() };
                ListViewItem item = new ListViewItem(row, group);
                item.Name = procInfo.Id.ToString();
                procListView.Items.Add(item);
            }
        }

        public void selectDragImage_MouseDown(object sender, EventArgs e)
        {
            selectDragImage.BackColor = SystemColors.ControlDarkDark;
            Cursor.Current = Cursors.Cross;

            dragTimer.Start();

            highlight.Show();
        }

        public void selectDragImage_MouseUp(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.Default;
            highlight.Hide();

            dragTimer.Stop();

            selectDragImage.BackColor = validProc ? Color.Green : SystemColors.Control;
            SetScaleFactorSlider();
        }

        public void selectDragImage_MouseEnter(object sender, EventArgs e)
        {
            selectDragImage.BackColor = SystemColors.ControlDark;
        }

        public void selectDragImage_MouseLeave(object sender, EventArgs e)
        {
            if (selectDragImage.BackColor != Color.Green)
            {
                selectDragImage.BackColor = SystemColors.Control;
            }
        }

        public void dragTimer_Tick(object sender, EventArgs e)
        {
            validProc = Spoof.SetCurrProcFromCursorPoint();
            DisplayInfoByCurrProc();
        }

        private void scaleFactorTrackBar_Scroll(object sender, EventArgs e)
        {
            UpdateScaleFactorSliderLabel();
        }

        private void scaleFactorSetButton_Click(object sender, EventArgs e)
        {
            if (Spoof.injectedProcs.ContainsKey(Spoof.CurrProcInfo.Id))
            {
                Spoof.SetScaleFactor();
                UpdateProcInfo(Spoof.CurrProcInfo);
            }
            else
            {
                Spoof.Install();
            }
            Spoof.RefreshCurrProcWindow(this);
        }

        private void procListUpdateTimer_Tick(object sender, EventArgs e)
        {
            foreach (Spoof.ProcInfo procInfo in Spoof.injectedProcs.Values)
            {
                if (!procListView.Items.ContainsKey(procInfo.Id.ToString()))
                {
                    UpdateProcInfo(procInfo);
                }
            }

            foreach (ListViewItem item in procListView.Items)
            {
                uint procId = uint.Parse(item.Name);
                if (!Spoof.injectedProcs.ContainsKey(procId))
                {
                    procListView.Items.RemoveByKey(item.Name);
                }
            }
        }

        private void procListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectDragImage.BackColor = SystemColors.Control;
            if (procListView.SelectedItems.Count != 0)
            {
                uint procId = uint.Parse(procListView.SelectedItems[0].Name);
                Spoof.ProcInfo procInfo;
                if (Spoof.injectedProcs.TryGetValue(procId, out procInfo))
                {
                    validProc = Spoof.SetCurrProcFromProcId(procInfo.Id, procInfo.TopHwnd);
                    DisplayInfoByCurrProc();

                    SetScaleFactorSlider(procInfo.ScaleFactor);
                    toggleButton.Enabled = true;
                    UpdateToggleButton(!procInfo.HookEnabled);
                }
            }
            else
            {
                validProc = Spoof.SetCurrProcFromProcId(0);
                DisplayInfoByCurrProc();
                SetScaleFactorSlider();
                toggleButton.Enabled = false;
            }
        }

        private void toggleButton_Click(object sender, EventArgs e)
        {
            Spoof.Toggle();
            UpdateToggleButton(!Spoof.CurrProcInfo.HookEnabled);
            UpdateProcInfo(Spoof.CurrProcInfo);
            Spoof.RefreshCurrProcWindow(this);
        }
    }
}
