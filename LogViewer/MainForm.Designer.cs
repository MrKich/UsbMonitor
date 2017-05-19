namespace LogViewer {
    partial class MainForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.tcMain = new System.Windows.Forms.TabControl();
            this.tpLogEntries = new System.Windows.Forms.TabPage();
            this.splitContainerLeftRight = new System.Windows.Forms.SplitContainer();
            this.lbEvents = new System.Windows.Forms.ListBox();
            this.bReset = new System.Windows.Forms.Button();
            this.tbPath = new System.Windows.Forms.TextBox();
            this.lblFilename = new System.Windows.Forms.Label();
            this.lblTo = new System.Windows.Forms.Label();
            this.lblFrom = new System.Windows.Forms.Label();
            this.lblSerial = new System.Windows.Forms.Label();
            this.lblUsers = new System.Windows.Forms.Label();
            this.tbSerial = new System.Windows.Forms.TextBox();
            this.dtpTo = new System.Windows.Forms.DateTimePicker();
            this.dtpFrom = new System.Windows.Forms.DateTimePicker();
            this.cbUsers = new System.Windows.Forms.ComboBox();
            this.tpRegisteredUsb = new System.Windows.Forms.TabPage();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.lbUsb = new System.Windows.Forms.ListBox();
            this.bDeleteUsb = new System.Windows.Forms.Button();
            this.bAddUsb = new System.Windows.Forms.Button();
            this.lbUsbSerial = new System.Windows.Forms.Label();
            this.lbUsbUser = new System.Windows.Forms.Label();
            this.tbUsbSerial = new System.Windows.Forms.TextBox();
            this.cbUsbUsers = new System.Windows.Forms.ComboBox();
            this.tcMain.SuspendLayout();
            this.tpLogEntries.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerLeftRight)).BeginInit();
            this.splitContainerLeftRight.Panel1.SuspendLayout();
            this.splitContainerLeftRight.Panel2.SuspendLayout();
            this.splitContainerLeftRight.SuspendLayout();
            this.tpRegisteredUsb.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tcMain
            // 
            this.tcMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tcMain.Controls.Add(this.tpLogEntries);
            this.tcMain.Controls.Add(this.tpRegisteredUsb);
            this.tcMain.Location = new System.Drawing.Point(0, 0);
            this.tcMain.Name = "tcMain";
            this.tcMain.SelectedIndex = 0;
            this.tcMain.Size = new System.Drawing.Size(614, 416);
            this.tcMain.TabIndex = 0;
            // 
            // tpLogEntries
            // 
            this.tpLogEntries.BackColor = System.Drawing.SystemColors.Control;
            this.tpLogEntries.Controls.Add(this.splitContainerLeftRight);
            this.tpLogEntries.Location = new System.Drawing.Point(4, 22);
            this.tpLogEntries.Name = "tpLogEntries";
            this.tpLogEntries.Padding = new System.Windows.Forms.Padding(3);
            this.tpLogEntries.Size = new System.Drawing.Size(606, 390);
            this.tpLogEntries.TabIndex = 0;
            this.tpLogEntries.Text = "Логи";
            // 
            // splitContainerLeftRight
            // 
            this.splitContainerLeftRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerLeftRight.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainerLeftRight.IsSplitterFixed = true;
            this.splitContainerLeftRight.Location = new System.Drawing.Point(3, 3);
            this.splitContainerLeftRight.Name = "splitContainerLeftRight";
            // 
            // splitContainerLeftRight.Panel1
            // 
            this.splitContainerLeftRight.Panel1.Controls.Add(this.lbEvents);
            // 
            // splitContainerLeftRight.Panel2
            // 
            this.splitContainerLeftRight.Panel2.BackColor = System.Drawing.Color.Transparent;
            this.splitContainerLeftRight.Panel2.Controls.Add(this.bReset);
            this.splitContainerLeftRight.Panel2.Controls.Add(this.tbPath);
            this.splitContainerLeftRight.Panel2.Controls.Add(this.lblFilename);
            this.splitContainerLeftRight.Panel2.Controls.Add(this.lblTo);
            this.splitContainerLeftRight.Panel2.Controls.Add(this.lblFrom);
            this.splitContainerLeftRight.Panel2.Controls.Add(this.lblSerial);
            this.splitContainerLeftRight.Panel2.Controls.Add(this.lblUsers);
            this.splitContainerLeftRight.Panel2.Controls.Add(this.tbSerial);
            this.splitContainerLeftRight.Panel2.Controls.Add(this.dtpTo);
            this.splitContainerLeftRight.Panel2.Controls.Add(this.dtpFrom);
            this.splitContainerLeftRight.Panel2.Controls.Add(this.cbUsers);
            this.splitContainerLeftRight.Size = new System.Drawing.Size(600, 384);
            this.splitContainerLeftRight.SplitterDistance = 311;
            this.splitContainerLeftRight.TabIndex = 1;
            this.splitContainerLeftRight.TabStop = false;
            // 
            // lbEvents
            // 
            this.lbEvents.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbEvents.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lbEvents.FormattingEnabled = true;
            this.lbEvents.HorizontalScrollbar = true;
            this.lbEvents.IntegralHeight = false;
            this.lbEvents.ItemHeight = 27;
            this.lbEvents.Location = new System.Drawing.Point(0, 0);
            this.lbEvents.Margin = new System.Windows.Forms.Padding(0);
            this.lbEvents.Name = "lbEvents";
            this.lbEvents.Size = new System.Drawing.Size(311, 384);
            this.lbEvents.TabIndex = 0;
            this.lbEvents.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lbEvents_DrawItem);
            // 
            // bReset
            // 
            this.bReset.Location = new System.Drawing.Point(3, 121);
            this.bReset.Name = "bReset";
            this.bReset.Size = new System.Drawing.Size(77, 25);
            this.bReset.TabIndex = 11;
            this.bReset.Text = "Сброс";
            this.bReset.UseVisualStyleBackColor = true;
            this.bReset.Click += new System.EventHandler(this.bReset_Click);
            // 
            // tbPath
            // 
            this.tbPath.Location = new System.Drawing.Point(3, 95);
            this.tbPath.Name = "tbPath";
            this.tbPath.Size = new System.Drawing.Size(278, 20);
            this.tbPath.TabIndex = 10;
            this.tbPath.TextChanged += new System.EventHandler(this.tbPath_TextChanged);
            // 
            // lblFilename
            // 
            this.lblFilename.AutoSize = true;
            this.lblFilename.Location = new System.Drawing.Point(3, 79);
            this.lblFilename.Name = "lblFilename";
            this.lblFilename.Size = new System.Drawing.Size(75, 13);
            this.lblFilename.TabIndex = 9;
            this.lblFilename.Text = "Фильтр пути:";
            // 
            // lblTo
            // 
            this.lblTo.AutoSize = true;
            this.lblTo.Location = new System.Drawing.Point(142, 40);
            this.lblTo.Name = "lblTo";
            this.lblTo.Size = new System.Drawing.Size(106, 13);
            this.lblTo.TabIndex = 8;
            this.lblTo.Text = "До (включительно):";
            // 
            // lblFrom
            // 
            this.lblFrom.AutoSize = true;
            this.lblFrom.Location = new System.Drawing.Point(3, 40);
            this.lblFrom.Name = "lblFrom";
            this.lblFrom.Size = new System.Drawing.Size(104, 13);
            this.lblFrom.TabIndex = 7;
            this.lblFrom.Text = "От (включительно):";
            // 
            // lblSerial
            // 
            this.lblSerial.AutoSize = true;
            this.lblSerial.Location = new System.Drawing.Point(142, 0);
            this.lblSerial.Name = "lblSerial";
            this.lblSerial.Size = new System.Drawing.Size(96, 13);
            this.lblSerial.TabIndex = 6;
            this.lblSerial.Text = "Серийный номер:";
            // 
            // lblUsers
            // 
            this.lblUsers.AutoSize = true;
            this.lblUsers.Location = new System.Drawing.Point(3, 0);
            this.lblUsers.Name = "lblUsers";
            this.lblUsers.Size = new System.Drawing.Size(83, 13);
            this.lblUsers.TabIndex = 5;
            this.lblUsers.Text = "Пользователь:";
            // 
            // tbSerial
            // 
            this.tbSerial.Location = new System.Drawing.Point(145, 16);
            this.tbSerial.Name = "tbSerial";
            this.tbSerial.Size = new System.Drawing.Size(136, 20);
            this.tbSerial.TabIndex = 3;
            this.tbSerial.TextChanged += new System.EventHandler(this.tbSerial_TextChanged);
            // 
            // dtpTo
            // 
            this.dtpTo.Location = new System.Drawing.Point(145, 56);
            this.dtpTo.Name = "dtpTo";
            this.dtpTo.Size = new System.Drawing.Size(136, 20);
            this.dtpTo.TabIndex = 2;
            this.dtpTo.ValueChanged += new System.EventHandler(this.dtpTo_ValueChanged);
            // 
            // dtpFrom
            // 
            this.dtpFrom.Location = new System.Drawing.Point(3, 56);
            this.dtpFrom.Name = "dtpFrom";
            this.dtpFrom.Size = new System.Drawing.Size(136, 20);
            this.dtpFrom.TabIndex = 1;
            this.dtpFrom.ValueChanged += new System.EventHandler(this.dtpFrom_ValueChanged);
            // 
            // cbUsers
            // 
            this.cbUsers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbUsers.FormattingEnabled = true;
            this.cbUsers.Location = new System.Drawing.Point(3, 16);
            this.cbUsers.Name = "cbUsers";
            this.cbUsers.Size = new System.Drawing.Size(136, 21);
            this.cbUsers.Sorted = true;
            this.cbUsers.TabIndex = 0;
            this.cbUsers.SelectedIndexChanged += new System.EventHandler(this.cbUsers_SelectedIndexChanged);
            // 
            // tpRegisteredUsb
            // 
            this.tpRegisteredUsb.BackColor = System.Drawing.SystemColors.Control;
            this.tpRegisteredUsb.Controls.Add(this.splitContainer1);
            this.tpRegisteredUsb.Location = new System.Drawing.Point(4, 22);
            this.tpRegisteredUsb.Name = "tpRegisteredUsb";
            this.tpRegisteredUsb.Padding = new System.Windows.Forms.Padding(3);
            this.tpRegisteredUsb.Size = new System.Drawing.Size(606, 390);
            this.tpRegisteredUsb.TabIndex = 1;
            this.tpRegisteredUsb.Text = "Допустимые USB";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(3, 3);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.lbUsb);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.BackColor = System.Drawing.Color.Transparent;
            this.splitContainer1.Panel2.Controls.Add(this.bDeleteUsb);
            this.splitContainer1.Panel2.Controls.Add(this.bAddUsb);
            this.splitContainer1.Panel2.Controls.Add(this.lbUsbSerial);
            this.splitContainer1.Panel2.Controls.Add(this.lbUsbUser);
            this.splitContainer1.Panel2.Controls.Add(this.tbUsbSerial);
            this.splitContainer1.Panel2.Controls.Add(this.cbUsbUsers);
            this.splitContainer1.Size = new System.Drawing.Size(600, 384);
            this.splitContainer1.SplitterDistance = 311;
            this.splitContainer1.TabIndex = 2;
            this.splitContainer1.TabStop = false;
            // 
            // lbUsb
            // 
            this.lbUsb.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbUsb.FormattingEnabled = true;
            this.lbUsb.HorizontalScrollbar = true;
            this.lbUsb.IntegralHeight = false;
            this.lbUsb.Location = new System.Drawing.Point(0, 0);
            this.lbUsb.Margin = new System.Windows.Forms.Padding(0);
            this.lbUsb.Name = "lbUsb";
            this.lbUsb.Size = new System.Drawing.Size(311, 384);
            this.lbUsb.TabIndex = 0;
            // 
            // bDeleteUsb
            // 
            this.bDeleteUsb.Location = new System.Drawing.Point(86, 43);
            this.bDeleteUsb.Name = "bDeleteUsb";
            this.bDeleteUsb.Size = new System.Drawing.Size(77, 25);
            this.bDeleteUsb.TabIndex = 8;
            this.bDeleteUsb.Text = "Удалить";
            this.bDeleteUsb.UseVisualStyleBackColor = true;
            this.bDeleteUsb.Click += new System.EventHandler(this.bDeleteUsb_Click);
            // 
            // bAddUsb
            // 
            this.bAddUsb.Location = new System.Drawing.Point(3, 43);
            this.bAddUsb.Name = "bAddUsb";
            this.bAddUsb.Size = new System.Drawing.Size(77, 25);
            this.bAddUsb.TabIndex = 7;
            this.bAddUsb.Text = "Добавить";
            this.bAddUsb.UseVisualStyleBackColor = true;
            this.bAddUsb.Click += new System.EventHandler(this.bAddUsb_Click);
            // 
            // lbUsbSerial
            // 
            this.lbUsbSerial.AutoSize = true;
            this.lbUsbSerial.Location = new System.Drawing.Point(142, 0);
            this.lbUsbSerial.Name = "lbUsbSerial";
            this.lbUsbSerial.Size = new System.Drawing.Size(96, 13);
            this.lbUsbSerial.TabIndex = 6;
            this.lbUsbSerial.Text = "Серийный номер:";
            // 
            // lbUsbUser
            // 
            this.lbUsbUser.AutoSize = true;
            this.lbUsbUser.Location = new System.Drawing.Point(3, 0);
            this.lbUsbUser.Name = "lbUsbUser";
            this.lbUsbUser.Size = new System.Drawing.Size(83, 13);
            this.lbUsbUser.TabIndex = 5;
            this.lbUsbUser.Text = "Пользователь:";
            // 
            // tbUsbSerial
            // 
            this.tbUsbSerial.Location = new System.Drawing.Point(145, 16);
            this.tbUsbSerial.Name = "tbUsbSerial";
            this.tbUsbSerial.Size = new System.Drawing.Size(136, 20);
            this.tbUsbSerial.TabIndex = 3;
            this.tbUsbSerial.TextChanged += new System.EventHandler(this.tbUsbSerial_TextChanged);
            // 
            // cbUsbUsers
            // 
            this.cbUsbUsers.FormattingEnabled = true;
            this.cbUsbUsers.Location = new System.Drawing.Point(3, 16);
            this.cbUsbUsers.Name = "cbUsbUsers";
            this.cbUsbUsers.Size = new System.Drawing.Size(136, 21);
            this.cbUsbUsers.Sorted = true;
            this.cbUsbUsers.TabIndex = 0;
            this.cbUsbUsers.SelectedIndexChanged += new System.EventHandler(this.cbUsbUsers_SelectedIndexChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(616, 416);
            this.Controls.Add(this.tcMain);
            this.DoubleBuffered = true;
            this.Name = "MainForm";
            this.Text = "LogViewer";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.tcMain.ResumeLayout(false);
            this.tpLogEntries.ResumeLayout(false);
            this.splitContainerLeftRight.Panel1.ResumeLayout(false);
            this.splitContainerLeftRight.Panel2.ResumeLayout(false);
            this.splitContainerLeftRight.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerLeftRight)).EndInit();
            this.splitContainerLeftRight.ResumeLayout(false);
            this.tpRegisteredUsb.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tcMain;
        private System.Windows.Forms.TabPage tpLogEntries;
        private System.Windows.Forms.SplitContainer splitContainerLeftRight;
        private System.Windows.Forms.ListBox lbEvents;
        private System.Windows.Forms.TextBox tbPath;
        private System.Windows.Forms.Label lblFilename;
        private System.Windows.Forms.Label lblTo;
        private System.Windows.Forms.Label lblFrom;
        private System.Windows.Forms.Label lblSerial;
        private System.Windows.Forms.Label lblUsers;
        private System.Windows.Forms.TextBox tbSerial;
        private System.Windows.Forms.DateTimePicker dtpTo;
        private System.Windows.Forms.DateTimePicker dtpFrom;
        private System.Windows.Forms.ComboBox cbUsers;
        private System.Windows.Forms.TabPage tpRegisteredUsb;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListBox lbUsb;
        private System.Windows.Forms.Button bDeleteUsb;
        private System.Windows.Forms.Button bAddUsb;
        private System.Windows.Forms.Label lbUsbSerial;
        private System.Windows.Forms.Label lbUsbUser;
        private System.Windows.Forms.TextBox tbUsbSerial;
        private System.Windows.Forms.ComboBox cbUsbUsers;
        private System.Windows.Forms.Button bReset;
    }
}

