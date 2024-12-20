namespace SpoofDPI_GUI
{
    partial class mainWindow
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            ListViewGroup listViewGroup1 = new ListViewGroup("Enabled", HorizontalAlignment.Left);
            ListViewGroup listViewGroup2 = new ListViewGroup("Disabled", HorizontalAlignment.Left);
            selectDragImage = new PictureBox();
            dragTimer = new System.Windows.Forms.Timer(components);
            dragLabel = new Label();
            windowSelectorGroupBox = new GroupBox();
            procNameLabel = new Label();
            scaleFactorTrackBar = new TrackBar();
            scaleFactorGroupBox = new GroupBox();
            scaleFactorLabel = new Label();
            scaleFactorSetButton = new Button();
            procListView = new ListView();
            name = new ColumnHeader();
            id = new ColumnHeader();
            scaleFactor = new ColumnHeader();
            separator = new Label();
            toggleButton = new Button();
            procListUpdateTimer = new System.Windows.Forms.Timer(components);
            ((System.ComponentModel.ISupportInitialize)selectDragImage).BeginInit();
            windowSelectorGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)scaleFactorTrackBar).BeginInit();
            scaleFactorGroupBox.SuspendLayout();
            SuspendLayout();
            // 
            // selectDragImage
            // 
            selectDragImage.BackColor = SystemColors.Control;
            selectDragImage.BorderStyle = BorderStyle.Fixed3D;
            selectDragImage.Location = new Point(191, 18);
            selectDragImage.Name = "selectDragImage";
            selectDragImage.Size = new Size(40, 40);
            selectDragImage.SizeMode = PictureBoxSizeMode.CenterImage;
            selectDragImage.TabIndex = 1;
            selectDragImage.TabStop = false;
            selectDragImage.MouseDown += selectDragImage_MouseDown;
            selectDragImage.MouseEnter += selectDragImage_MouseEnter;
            selectDragImage.MouseLeave += selectDragImage_MouseLeave;
            selectDragImage.MouseUp += selectDragImage_MouseUp;
            // 
            // dragTimer
            // 
            dragTimer.Tick += dragTimer_Tick;
            // 
            // dragLabel
            // 
            dragLabel.AutoSize = true;
            dragLabel.ForeColor = SystemColors.GrayText;
            dragLabel.Location = new Point(6, 23);
            dragLabel.Name = "dragLabel";
            dragLabel.Size = new Size(180, 30);
            dragLabel.TabIndex = 2;
            dragLabel.Text = "Drag mouse from the box to the \r\nwindow you would like to select:";
            // 
            // windowSelectorGroupBox
            // 
            windowSelectorGroupBox.Controls.Add(procNameLabel);
            windowSelectorGroupBox.Controls.Add(dragLabel);
            windowSelectorGroupBox.Controls.Add(selectDragImage);
            windowSelectorGroupBox.Location = new Point(12, 12);
            windowSelectorGroupBox.Name = "windowSelectorGroupBox";
            windowSelectorGroupBox.Size = new Size(241, 90);
            windowSelectorGroupBox.TabIndex = 3;
            windowSelectorGroupBox.TabStop = false;
            windowSelectorGroupBox.Text = "Window Selector";
            // 
            // procNameLabel
            // 
            procNameLabel.Location = new Point(9, 61);
            procNameLabel.Name = "procNameLabel";
            procNameLabel.Size = new Size(226, 19);
            procNameLabel.TabIndex = 3;
            procNameLabel.Text = "[No process selected]";
            procNameLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // scaleFactorTrackBar
            // 
            scaleFactorTrackBar.Location = new Point(31, 22);
            scaleFactorTrackBar.Maximum = 15;
            scaleFactorTrackBar.Name = "scaleFactorTrackBar";
            scaleFactorTrackBar.Size = new Size(204, 45);
            scaleFactorTrackBar.TabIndex = 0;
            scaleFactorTrackBar.Scroll += scaleFactorTrackBar_Scroll;
            // 
            // scaleFactorGroupBox
            // 
            scaleFactorGroupBox.Controls.Add(scaleFactorLabel);
            scaleFactorGroupBox.Controls.Add(scaleFactorSetButton);
            scaleFactorGroupBox.Controls.Add(scaleFactorTrackBar);
            scaleFactorGroupBox.Enabled = false;
            scaleFactorGroupBox.Location = new Point(12, 108);
            scaleFactorGroupBox.Name = "scaleFactorGroupBox";
            scaleFactorGroupBox.Size = new Size(241, 97);
            scaleFactorGroupBox.TabIndex = 6;
            scaleFactorGroupBox.TabStop = false;
            scaleFactorGroupBox.Text = "Spoofed Scale Factor";
            // 
            // scaleFactorLabel
            // 
            scaleFactorLabel.AutoSize = true;
            scaleFactorLabel.Location = new Point(6, 26);
            scaleFactorLabel.Name = "scaleFactorLabel";
            scaleFactorLabel.Size = new Size(25, 15);
            scaleFactorLabel.TabIndex = 3;
            scaleFactorLabel.Text = "100";
            scaleFactorLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // scaleFactorSetButton
            // 
            scaleFactorSetButton.Location = new Point(81, 61);
            scaleFactorSetButton.Name = "scaleFactorSetButton";
            scaleFactorSetButton.Size = new Size(75, 23);
            scaleFactorSetButton.TabIndex = 2;
            scaleFactorSetButton.Text = "Set";
            scaleFactorSetButton.UseVisualStyleBackColor = true;
            scaleFactorSetButton.Click += scaleFactorSetButton_Click;
            // 
            // procListView
            // 
            procListView.Columns.AddRange(new ColumnHeader[] { name, id, scaleFactor });
            listViewGroup1.Header = "Enabled";
            listViewGroup1.Name = "enabled";
            listViewGroup2.Header = "Disabled";
            listViewGroup2.Name = "disabled";
            procListView.Groups.AddRange(new ListViewGroup[] { listViewGroup1, listViewGroup2 });
            procListView.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            procListView.LabelWrap = false;
            procListView.Location = new Point(267, 13);
            procListView.MultiSelect = false;
            procListView.Name = "procListView";
            procListView.Size = new Size(234, 162);
            procListView.TabIndex = 7;
            procListView.UseCompatibleStateImageBehavior = false;
            procListView.View = View.Details;
            procListView.SelectedIndexChanged += procListView_SelectedIndexChanged;
            // 
            // name
            // 
            name.Text = "Name";
            name.Width = 130;
            // 
            // id
            // 
            id.Text = "PID";
            id.TextAlign = HorizontalAlignment.Right;
            id.Width = 50;
            // 
            // scaleFactor
            // 
            scaleFactor.Text = "Scale";
            scaleFactor.TextAlign = HorizontalAlignment.Right;
            scaleFactor.Width = 50;
            // 
            // separator
            // 
            separator.BorderStyle = BorderStyle.Fixed3D;
            separator.Location = new Point(259, 9);
            separator.Name = "separator";
            separator.Size = new Size(2, 198);
            separator.TabIndex = 8;
            separator.Text = "break";
            // 
            // toggleButton
            // 
            toggleButton.Enabled = false;
            toggleButton.Location = new Point(426, 182);
            toggleButton.Name = "toggleButton";
            toggleButton.Size = new Size(75, 23);
            toggleButton.TabIndex = 9;
            toggleButton.Text = "Disable";
            toggleButton.UseVisualStyleBackColor = true;
            toggleButton.Click += toggleButton_Click;
            // 
            // procListUpdateTimer
            // 
            procListUpdateTimer.Tick += procListUpdateTimer_Tick;
            // 
            // mainWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(511, 216);
            Controls.Add(toggleButton);
            Controls.Add(procListView);
            Controls.Add(separator);
            Controls.Add(scaleFactorGroupBox);
            Controls.Add(windowSelectorGroupBox);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Name = "mainWindow";
            Text = "SpoofDPI";
            ((System.ComponentModel.ISupportInitialize)selectDragImage).EndInit();
            windowSelectorGroupBox.ResumeLayout(false);
            windowSelectorGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)scaleFactorTrackBar).EndInit();
            scaleFactorGroupBox.ResumeLayout(false);
            scaleFactorGroupBox.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private PictureBox selectDragImage;
        private System.Windows.Forms.Timer dragTimer;
        private Label dragLabel;
        private GroupBox windowSelectorGroupBox;
        private TrackBar scaleFactorTrackBar;
        private GroupBox scaleFactorGroupBox;
        private Button scaleFactorSetButton;
        private Label procNameLabel;
        private Label scaleFactorLabel;
        private ListView procListView;
        private ColumnHeader name;
        private ColumnHeader id;
        private ColumnHeader scaleFactor;
        private Label separator;
        private Button toggleButton;
        private System.Windows.Forms.Timer procListUpdateTimer;
    }
}
