namespace BracletDriver;
partial class MainForm
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
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
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        tableMain = new TableLayoutPanel();
        flowConnection = new FlowLayoutPanel();
        btnPort = new Button();
        btnCamera = new Button();
        btnAnalyzer = new Button();
        btnSettings = new Button();
        lblStatus = new Label();
        flowLayers = new FlowLayoutPanel();
        chkRoi = new CheckBox();
        chkTarget = new CheckBox();
        chkProbe = new CheckBox();
        chkRodTip = new CheckBox();
        chkHoles = new CheckBox();
        picPreview = new PictureBox();
        flowPreviewTools = new FlowLayoutPanel();
        btnPickRoi = new Button();
        lblPickRoi = new Label();
        flowExposure = new FlowLayoutPanel();
        lblExposure = new Label();
        flowGuidance = new FlowLayoutPanel();
        btnStart = new Button();
        btnStop = new Button();
        btnFindTarget = new Button();
        btnTuneExposure = new Button();
        btnCalibrate = new Button();
        btnHoleExposure = new Button();
        btnHoleCancel = new Button();
        btnExposureDown = new Button();
        btnExposureUp = new Button();
        lblHole = new Label();
        lblGuidance = new Label();
        flowDrive = new FlowLayoutPanel();
        btnBaseline = new Button();
        btnRoiPosition = new Button();
        btnMiddle = new Button();
        btnHome = new Button();
        btnDriveStop = new Button();
        btnMotorsOn = new Button();
        btnMotorsOff = new Button();
        lblPosition = new Label();
        flowCommand = new FlowLayoutPanel();
        txtCommand = new TextBox();
        btnSend = new Button();
        txtLog = new TextBox();
        timerUi = new System.Windows.Forms.Timer(components);
        tableMain.SuspendLayout();
        flowConnection.SuspendLayout();
        flowLayers.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)picPreview).BeginInit();
        flowPreviewTools.SuspendLayout();
        flowGuidance.SuspendLayout();
        flowExposure.SuspendLayout();
        flowDrive.SuspendLayout();
        flowCommand.SuspendLayout();
        SuspendLayout();
        //
        // tableMain
        //
        tableMain.ColumnCount = 1;
        tableMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tableMain.Controls.Add(flowConnection, 0, 0);
        tableMain.Controls.Add(flowLayers, 0, 1);
        tableMain.Controls.Add(picPreview, 0, 2);
        tableMain.Controls.Add(flowPreviewTools, 0, 3);
        tableMain.Controls.Add(flowGuidance, 0, 4);
        tableMain.Controls.Add(flowExposure, 0, 5);
        tableMain.Controls.Add(flowDrive, 0, 6);
        tableMain.Controls.Add(flowCommand, 0, 7);
        tableMain.Controls.Add(txtLog, 0, 8);
        tableMain.Dock = DockStyle.Fill;
        tableMain.Location = new Point(0, 0);
        tableMain.Name = "tableMain";
        tableMain.RowCount = 9;
        tableMain.RowStyles.Add(new RowStyle());
        tableMain.RowStyles.Add(new RowStyle());
        tableMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tableMain.RowStyles.Add(new RowStyle());
        tableMain.RowStyles.Add(new RowStyle());
        tableMain.RowStyles.Add(new RowStyle());
        tableMain.RowStyles.Add(new RowStyle());
        tableMain.RowStyles.Add(new RowStyle());
        tableMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 150F));
        tableMain.Size = new Size(1000, 861);
        tableMain.TabIndex = 0;
        //
        // flowConnection
        //
        flowConnection.AutoSize = true;
        flowConnection.Controls.Add(btnPort);
        flowConnection.Controls.Add(btnCamera);
        flowConnection.Controls.Add(btnAnalyzer);
        flowConnection.Controls.Add(btnSettings);
        flowConnection.Controls.Add(lblStatus);
        flowConnection.Dock = DockStyle.Fill;
        flowConnection.Location = new Point(3, 3);
        flowConnection.Name = "flowConnection";
        flowConnection.Size = new Size(994, 34);
        flowConnection.TabIndex = 0;
        //
        // btnPort
        //
        btnPort.AutoSize = true;
        btnPort.Location = new Point(3, 3);
        btnPort.Name = "btnPort";
        btnPort.Size = new Size(110, 28);
        btnPort.TabIndex = 0;
        btnPort.Text = "Открыть порт";
        btnPort.UseVisualStyleBackColor = true;
        btnPort.Click += btnPort_Click;
        //
        // btnCamera
        //
        btnCamera.AutoSize = true;
        btnCamera.Location = new Point(119, 3);
        btnCamera.Name = "btnCamera";
        btnCamera.Size = new Size(120, 28);
        btnCamera.TabIndex = 1;
        btnCamera.Text = "Открыть камеру";
        btnCamera.UseVisualStyleBackColor = true;
        btnCamera.Click += btnCamera_Click;
        //
        // btnAnalyzer
        //
        btnAnalyzer.AutoSize = true;
        btnAnalyzer.Location = new Point(245, 3);
        btnAnalyzer.Name = "btnAnalyzer";
        btnAnalyzer.Size = new Size(160, 28);
        btnAnalyzer.TabIndex = 2;
        btnAnalyzer.Text = "Включить анализатор";
        btnAnalyzer.UseVisualStyleBackColor = true;
        btnAnalyzer.Click += btnAnalyzer_Click;
        //
        // btnSettings
        //
        btnSettings.AutoSize = true;
        btnSettings.Location = new Point(411, 3);
        btnSettings.Name = "btnSettings";
        btnSettings.Size = new Size(95, 28);
        btnSettings.TabIndex = 3;
        btnSettings.Text = "Настройки…";
        btnSettings.UseVisualStyleBackColor = true;
        btnSettings.Click += btnSettings_Click;
        //
        // lblStatus
        //
        lblStatus.AutoSize = true;
        lblStatus.Location = new Point(512, 9);
        lblStatus.Margin = new Padding(3, 9, 3, 0);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(0, 15);
        lblStatus.TabIndex = 4;
        //
        // flowLayers
        //
        flowLayers.AutoSize = true;
        flowLayers.Controls.Add(chkRoi);
        flowLayers.Controls.Add(chkTarget);
        flowLayers.Controls.Add(chkProbe);
        flowLayers.Controls.Add(chkRodTip);
        flowLayers.Controls.Add(chkHoles);
        flowLayers.Dock = DockStyle.Fill;
        flowLayers.Location = new Point(3, 43);
        flowLayers.Name = "flowLayers";
        flowLayers.Size = new Size(994, 25);
        flowLayers.TabIndex = 1;
        //
        // chkRoi
        //
        chkRoi.AutoSize = true;
        chkRoi.Checked = true;
        chkRoi.CheckState = CheckState.Checked;
        chkRoi.Location = new Point(3, 3);
        chkRoi.Name = "chkRoi";
        chkRoi.Size = new Size(62, 19);
        chkRoi.TabIndex = 0;
        chkRoi.Text = "Рамка";
        chkRoi.UseVisualStyleBackColor = true;
        chkRoi.CheckedChanged += chkLayer_CheckedChanged;
        //
        // chkTarget
        //
        chkTarget.AutoSize = true;
        chkTarget.Checked = true;
        chkTarget.CheckState = CheckState.Checked;
        chkTarget.Location = new Point(71, 3);
        chkTarget.Name = "chkTarget";
        chkTarget.Size = new Size(54, 19);
        chkTarget.TabIndex = 1;
        chkTarget.Text = "Цель";
        chkTarget.UseVisualStyleBackColor = true;
        chkTarget.CheckedChanged += chkLayer_CheckedChanged;
        //
        // chkProbe
        //
        chkProbe.AutoSize = true;
        chkProbe.Checked = true;
        chkProbe.CheckState = CheckState.Checked;
        chkProbe.Location = new Point(131, 3);
        chkProbe.Name = "chkProbe";
        chkProbe.Size = new Size(106, 19);
        chkProbe.TabIndex = 2;
        chkProbe.Text = "Окно яркости";
        chkProbe.UseVisualStyleBackColor = true;
        chkProbe.CheckedChanged += chkLayer_CheckedChanged;
        //
        // chkRodTip
        //
        chkRodTip.AutoSize = true;
        chkRodTip.Checked = true;
        chkRodTip.CheckState = CheckState.Checked;
        chkRodTip.Location = new Point(243, 3);
        chkRodTip.Name = "chkRodTip";
        chkRodTip.Size = new Size(65, 19);
        chkRodTip.TabIndex = 3;
        chkRodTip.Text = "Кончик";
        chkRodTip.UseVisualStyleBackColor = true;
        chkRodTip.CheckedChanged += chkLayer_CheckedChanged;
        //
        // chkHoles
        //
        chkHoles.AutoSize = true;
        chkHoles.Checked = true;
        chkHoles.CheckState = CheckState.Checked;
        chkHoles.Location = new Point(314, 3);
        chkHoles.Name = "chkHoles";
        chkHoles.Size = new Size(82, 19);
        chkHoles.TabIndex = 4;
        chkHoles.Text = "Отверстия";
        chkHoles.UseVisualStyleBackColor = true;
        chkHoles.CheckedChanged += chkLayer_CheckedChanged;
        //
        // picPreview
        //
        picPreview.BackColor = Color.Black;
        picPreview.Dock = DockStyle.Fill;
        picPreview.Location = new Point(3, 74);
        picPreview.Name = "picPreview";
        picPreview.Size = new Size(994, 422);
        picPreview.SizeMode = PictureBoxSizeMode.Zoom;
        picPreview.TabIndex = 2;
        picPreview.TabStop = false;
        picPreview.MouseClick += picPreview_MouseClick;
        //
        // flowPreviewTools
        //
        flowPreviewTools.AutoSize = true;
        flowPreviewTools.Controls.Add(btnPickRoi);
        flowPreviewTools.Controls.Add(lblPickRoi);
        flowPreviewTools.Dock = DockStyle.Fill;
        flowPreviewTools.Location = new Point(3, 502);
        flowPreviewTools.Name = "flowPreviewTools";
        flowPreviewTools.Size = new Size(994, 34);
        flowPreviewTools.TabIndex = 3;
        //
        // btnPickRoi
        //
        btnPickRoi.AutoSize = true;
        btnPickRoi.Location = new Point(3, 3);
        btnPickRoi.Name = "btnPickRoi";
        btnPickRoi.Size = new Size(160, 28);
        btnPickRoi.TabIndex = 0;
        btnPickRoi.Text = "Задать рамку кликами";
        btnPickRoi.UseVisualStyleBackColor = true;
        btnPickRoi.Click += btnPickRoi_Click;
        //
        // lblPickRoi
        //
        lblPickRoi.AutoSize = true;
        lblPickRoi.Location = new Point(169, 9);
        lblPickRoi.Margin = new Padding(3, 9, 3, 0);
        lblPickRoi.Name = "lblPickRoi";
        lblPickRoi.Size = new Size(0, 15);
        lblPickRoi.TabIndex = 1;
        //
        // flowGuidance
        //
        flowGuidance.AutoSize = true;
        flowGuidance.Controls.Add(btnStart);
        flowGuidance.Controls.Add(btnStop);
        flowGuidance.Controls.Add(btnFindTarget);
        flowGuidance.Controls.Add(btnCalibrate);
        flowGuidance.Controls.Add(lblGuidance);
        flowGuidance.Dock = DockStyle.Fill;
        flowGuidance.Location = new Point(3, 542);
        flowGuidance.Name = "flowGuidance";
        flowGuidance.Size = new Size(994, 34);
        flowGuidance.TabIndex = 4;
        //
        // btnStart
        //
        btnStart.AutoSize = true;
        btnStart.Location = new Point(3, 3);
        btnStart.Name = "btnStart";
        btnStart.Size = new Size(75, 28);
        btnStart.TabIndex = 0;
        btnStart.Text = "Старт";
        btnStart.UseVisualStyleBackColor = true;
        btnStart.Click += btnStart_Click;
        //
        // btnStop
        //
        btnStop.AutoSize = true;
        btnStop.Location = new Point(84, 3);
        btnStop.Name = "btnStop";
        btnStop.Size = new Size(75, 28);
        btnStop.TabIndex = 1;
        btnStop.Text = "Стоп";
        btnStop.UseVisualStyleBackColor = true;
        btnStop.Click += btnStop_Click;
        //
        // btnFindTarget
        //
        btnFindTarget.AutoSize = true;
        btnFindTarget.Location = new Point(165, 3);
        btnFindTarget.Name = "btnFindTarget";
        btnFindTarget.Size = new Size(120, 28);
        btnFindTarget.TabIndex = 2;
        btnFindTarget.Text = "Найти отверстие";
        btnFindTarget.UseVisualStyleBackColor = true;
        btnFindTarget.Click += btnFindTarget_Click;
        //
        // btnTuneExposure
        //
        btnTuneExposure.AutoSize = true;
        btnTuneExposure.Location = new Point(291, 3);
        btnTuneExposure.Name = "btnTuneExposure";
        btnTuneExposure.Size = new Size(140, 28);
        btnTuneExposure.TabIndex = 3;
        btnTuneExposure.Text = "Подобрать выдержку для стержня";
        btnTuneExposure.UseVisualStyleBackColor = true;
        btnTuneExposure.Click += btnTuneExposure_Click;
        //
        // btnCalibrate
        //
        btnCalibrate.AutoSize = true;
        btnCalibrate.Location = new Point(437, 3);
        btnCalibrate.Name = "btnCalibrate";
        btnCalibrate.Size = new Size(140, 28);
        btnCalibrate.TabIndex = 4;
        btnCalibrate.Text = "Калибровать моторы";
        btnCalibrate.UseVisualStyleBackColor = true;
        btnCalibrate.Click += btnCalibrate_Click;
        //
        // btnHoleExposure
        //
        btnHoleExposure.AutoSize = true;
        btnHoleExposure.Location = new Point(3, 37);
        btnHoleExposure.Name = "btnHoleExposure";
        btnHoleExposure.Size = new Size(200, 28);
        btnHoleExposure.TabIndex = 5;
        btnHoleExposure.Text = "Подобрать выдержку для отверстий";
        btnHoleExposure.UseVisualStyleBackColor = true;
        btnHoleExposure.Click += btnHoleExposure_Click;
        //
        // btnHoleCancel
        //
        btnHoleCancel.AutoSize = true;
        btnHoleCancel.Location = new Point(209, 37);
        btnHoleCancel.Name = "btnHoleCancel";
        btnHoleCancel.Size = new Size(75, 28);
        btnHoleCancel.TabIndex = 6;
        btnHoleCancel.Text = "Отмена";
        btnHoleCancel.UseVisualStyleBackColor = true;
        btnHoleCancel.Click += btnHoleCancel_Click;
        //
        // btnExposureDown
        //
        btnExposureDown.AutoSize = true;
        btnExposureDown.Location = new Point(290, 37);
        btnExposureDown.Name = "btnExposureDown";
        btnExposureDown.Size = new Size(95, 28);
        btnExposureDown.TabIndex = 7;
        btnExposureDown.Text = "Выдержка −";
        btnExposureDown.UseVisualStyleBackColor = true;
        btnExposureDown.Click += btnExposureDown_Click;
        //
        // btnExposureUp
        //
        btnExposureUp.AutoSize = true;
        btnExposureUp.Location = new Point(391, 37);
        btnExposureUp.Name = "btnExposureUp";
        btnExposureUp.Size = new Size(95, 28);
        btnExposureUp.TabIndex = 8;
        btnExposureUp.Text = "Выдержка +";
        btnExposureUp.UseVisualStyleBackColor = true;
        btnExposureUp.Click += btnExposureUp_Click;
        //
        // lblHole
        //
        lblHole.AutoSize = true;
        lblHole.Location = new Point(492, 43);
        lblHole.Margin = new Padding(3, 9, 3, 0);
        lblHole.Name = "lblHole";
        lblHole.Size = new Size(0, 15);
        lblHole.TabIndex = 9;
        //
        // lblGuidance
        //
        lblGuidance.AutoSize = true;
        lblGuidance.Location = new Point(583, 9);
        lblGuidance.Margin = new Padding(3, 9, 3, 0);
        lblGuidance.Name = "lblGuidance";
        lblGuidance.Size = new Size(0, 15);
        lblGuidance.TabIndex = 10;
        //
        // flowExposure
        //
        flowExposure.AutoSize = true;
        flowExposure.Controls.Add(btnHoleExposure);
        flowExposure.Controls.Add(btnHoleCancel);
        flowExposure.Controls.Add(btnExposureDown);
        flowExposure.Controls.Add(btnExposureUp);
        flowExposure.Controls.Add(lblHole);
        flowExposure.Controls.Add(btnTuneExposure);
        flowExposure.Controls.Add(lblExposure);
        flowExposure.Dock = DockStyle.Fill;
        flowExposure.Location = new Point(3, 582);
        flowExposure.Name = "flowExposure";
        flowExposure.Size = new Size(994, 34);
        flowExposure.TabIndex = 5;
        //
        // lblExposure
        //
        lblExposure.AutoSize = true;
        lblExposure.Location = new Point(0, 9);
        lblExposure.Margin = new Padding(3, 9, 3, 0);
        lblExposure.Name = "lblExposure";
        lblExposure.Size = new Size(0, 15);
        lblExposure.TabIndex = 7;
        //
        // flowDrive
        //
        flowDrive.AutoSize = true;
        flowDrive.Controls.Add(btnBaseline);
        flowDrive.Controls.Add(btnRoiPosition);
        flowDrive.Controls.Add(btnMiddle);
        flowDrive.Controls.Add(btnHome);
        flowDrive.Controls.Add(btnDriveStop);
        flowDrive.Controls.Add(btnMotorsOn);
        flowDrive.Controls.Add(btnMotorsOff);
        flowDrive.Controls.Add(lblPosition);
        flowDrive.Dock = DockStyle.Fill;
        flowDrive.Location = new Point(3, 582);
        flowDrive.Name = "flowDrive";
        flowDrive.Size = new Size(994, 34);
        flowDrive.TabIndex = 6;
        //
        // btnBaseline
        //
        btnBaseline.AutoSize = true;
        btnBaseline.Location = new Point(3, 3);
        btnBaseline.Name = "btnBaseline";
        btnBaseline.Size = new Size(75, 28);
        btnBaseline.TabIndex = 0;
        btnBaseline.Text = "Эталон";
        btnBaseline.UseVisualStyleBackColor = true;
        btnBaseline.Click += btnBaseline_Click;
        //
        // btnRoiPosition
        //
        btnRoiPosition.AutoSize = true;
        btnRoiPosition.Location = new Point(84, 3);
        btnRoiPosition.Name = "btnRoiPosition";
        btnRoiPosition.Size = new Size(75, 28);
        btnRoiPosition.TabIndex = 1;
        btnRoiPosition.Text = "Рамка";
        btnRoiPosition.UseVisualStyleBackColor = true;
        btnRoiPosition.Click += btnRoiPosition_Click;
        //
        // btnMiddle
        //
        btnMiddle.AutoSize = true;
        btnMiddle.Location = new Point(165, 3);
        btnMiddle.Name = "btnMiddle";
        btnMiddle.Size = new Size(90, 28);
        btnMiddle.TabIndex = 2;
        btnMiddle.Text = "Промежуток";
        btnMiddle.UseVisualStyleBackColor = true;
        btnMiddle.Click += btnMiddle_Click;
        //
        // btnHome
        //
        btnHome.AutoSize = true;
        btnHome.Location = new Point(261, 3);
        btnHome.Name = "btnHome";
        btnHome.Size = new Size(75, 28);
        btnHome.TabIndex = 3;
        btnHome.Text = "Дом";
        btnHome.UseVisualStyleBackColor = true;
        btnHome.Click += btnHome_Click;
        //
        // btnDriveStop
        //
        btnDriveStop.AutoSize = true;
        btnDriveStop.Location = new Point(342, 3);
        btnDriveStop.Name = "btnDriveStop";
        btnDriveStop.Size = new Size(75, 28);
        btnDriveStop.TabIndex = 4;
        btnDriveStop.Text = "Стоп";
        btnDriveStop.UseVisualStyleBackColor = true;
        btnDriveStop.Click += btnDriveStop_Click;
        //
        // btnMotorsOn
        //
        btnMotorsOn.AutoSize = true;
        btnMotorsOn.Location = new Point(423, 3);
        btnMotorsOn.Name = "btnMotorsOn";
        btnMotorsOn.Size = new Size(120, 28);
        btnMotorsOn.TabIndex = 5;
        btnMotorsOn.Text = "Включить моторы";
        btnMotorsOn.UseVisualStyleBackColor = true;
        btnMotorsOn.Click += btnMotorsOn_Click;
        //
        // btnMotorsOff
        //
        btnMotorsOff.AutoSize = true;
        btnMotorsOff.Location = new Point(549, 3);
        btnMotorsOff.Name = "btnMotorsOff";
        btnMotorsOff.Size = new Size(130, 28);
        btnMotorsOff.TabIndex = 6;
        btnMotorsOff.Text = "Выключить моторы";
        btnMotorsOff.UseVisualStyleBackColor = true;
        btnMotorsOff.Click += btnMotorsOff_Click;
        //
        // lblPosition
        //
        lblPosition.AutoSize = true;
        lblPosition.Location = new Point(423, 9);
        lblPosition.Margin = new Padding(3, 9, 3, 0);
        lblPosition.Name = "lblPosition";
        lblPosition.Size = new Size(0, 15);
        lblPosition.TabIndex = 7;
        //
        // flowCommand
        //
        flowCommand.AutoSize = true;
        flowCommand.Controls.Add(txtCommand);
        flowCommand.Controls.Add(btnSend);
        flowCommand.Dock = DockStyle.Fill;
        flowCommand.Location = new Point(3, 622);
        flowCommand.Name = "flowCommand";
        flowCommand.Size = new Size(994, 34);
        flowCommand.TabIndex = 7;
        //
        // txtCommand
        //
        txtCommand.Location = new Point(3, 6);
        txtCommand.Margin = new Padding(3, 6, 3, 3);
        txtCommand.Name = "txtCommand";
        txtCommand.PlaceholderText = "Команда плате";
        txtCommand.Size = new Size(320, 23);
        txtCommand.TabIndex = 0;
        txtCommand.KeyDown += txtCommand_KeyDown;
        //
        // btnSend
        //
        btnSend.AutoSize = true;
        btnSend.Location = new Point(329, 3);
        btnSend.Name = "btnSend";
        btnSend.Size = new Size(90, 28);
        btnSend.TabIndex = 1;
        btnSend.Text = "Отправить";
        btnSend.UseVisualStyleBackColor = true;
        btnSend.Click += btnSend_Click;
        //
        // txtLog
        //
        txtLog.Dock = DockStyle.Fill;
        txtLog.Location = new Point(3, 662);
        txtLog.Multiline = true;
        txtLog.Name = "txtLog";
        txtLog.ReadOnly = true;
        txtLog.ScrollBars = ScrollBars.Vertical;
        txtLog.Size = new Size(994, 196);
        txtLog.TabIndex = 8;
        //
        // timerUi
        //
        timerUi.Interval = 200;
        timerUi.Tick += timerUi_Tick;
        //
        // MainForm
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1000, 861);
        Controls.Add(tableMain);
        KeyPreview = true;
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "BracletDriver";
        FormClosing += MainForm_FormClosing;
        KeyDown += MainForm_KeyDown;
        tableMain.ResumeLayout(false);
        tableMain.PerformLayout();
        flowConnection.ResumeLayout(false);
        flowConnection.PerformLayout();
        flowLayers.ResumeLayout(false);
        flowLayers.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)picPreview).EndInit();
        flowPreviewTools.ResumeLayout(false);
        flowPreviewTools.PerformLayout();
        flowGuidance.ResumeLayout(false);
        flowGuidance.PerformLayout();
        flowExposure.ResumeLayout(false);
        flowExposure.PerformLayout();
        flowDrive.ResumeLayout(false);
        flowDrive.PerformLayout();
        flowCommand.ResumeLayout(false);
        flowCommand.PerformLayout();
        ResumeLayout(false);
    }

    #endregion

    private TableLayoutPanel tableMain;
    private FlowLayoutPanel flowConnection;
    private Button btnPort;
    private Button btnCamera;
    private Button btnAnalyzer;
    private Button btnSettings;
    private Label lblStatus;
    private FlowLayoutPanel flowLayers;
    private CheckBox chkRoi;
    private CheckBox chkTarget;
    private CheckBox chkProbe;
    private CheckBox chkRodTip;
    private CheckBox chkHoles;
    private PictureBox picPreview;
    private FlowLayoutPanel flowPreviewTools;
    private Button btnPickRoi;
    private Label lblPickRoi;
    private FlowLayoutPanel flowExposure;
    private Label lblExposure;
    private FlowLayoutPanel flowGuidance;
    private Button btnStart;
    private Button btnStop;
    private Button btnFindTarget;
    private Button btnTuneExposure;
    private Button btnCalibrate;
    private Button btnHoleExposure;
    private Button btnHoleCancel;
    private Button btnExposureDown;
    private Button btnExposureUp;
    private Label lblHole;
    private Label lblGuidance;
    private FlowLayoutPanel flowDrive;
    private Button btnBaseline;
    private Button btnRoiPosition;
    private Button btnMiddle;
    private Button btnHome;
    private Button btnDriveStop;
    private Button btnMotorsOn;
    private Button btnMotorsOff;
    private Label lblPosition;
    private FlowLayoutPanel flowCommand;
    private TextBox txtCommand;
    private Button btnSend;
    private TextBox txtLog;
    private System.Windows.Forms.Timer timerUi;
}
