namespace BracletDriver;
partial class SettingsForm
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
        propertyGrid = new PropertyGrid();
        flowButtons = new FlowLayoutPanel();
        btnCancel = new Button();
        btnOk = new Button();
        btnReset = new Button();
        flowButtons.SuspendLayout();
        SuspendLayout();
        //
        // propertyGrid
        //
        propertyGrid.Dock = DockStyle.Fill;
        propertyGrid.Location = new Point(0, 0);
        propertyGrid.Name = "propertyGrid";
        propertyGrid.Size = new Size(484, 521);
        propertyGrid.TabIndex = 0;
        //
        // flowButtons
        //
        flowButtons.AutoSize = true;
        flowButtons.Controls.Add(btnCancel);
        flowButtons.Controls.Add(btnOk);
        flowButtons.Controls.Add(btnReset);
        flowButtons.Dock = DockStyle.Bottom;
        flowButtons.FlowDirection = FlowDirection.RightToLeft;
        flowButtons.Location = new Point(0, 521);
        flowButtons.Name = "flowButtons";
        flowButtons.Padding = new Padding(3);
        flowButtons.Size = new Size(484, 40);
        flowButtons.TabIndex = 1;
        //
        // btnCancel
        //
        btnCancel.AutoSize = true;
        btnCancel.DialogResult = DialogResult.Cancel;
        btnCancel.Location = new Point(390, 6);
        btnCancel.Name = "btnCancel";
        btnCancel.Size = new Size(85, 28);
        btnCancel.TabIndex = 2;
        btnCancel.Text = "Отмена";
        btnCancel.UseVisualStyleBackColor = true;
        //
        // btnOk
        //
        btnOk.AutoSize = true;
        btnOk.Location = new Point(299, 6);
        btnOk.Name = "btnOk";
        btnOk.Size = new Size(85, 28);
        btnOk.TabIndex = 1;
        btnOk.Text = "OK";
        btnOk.UseVisualStyleBackColor = true;
        btnOk.Click += btnOk_Click;
        //
        // btnReset
        //
        btnReset.AutoSize = true;
        btnReset.Location = new Point(208, 6);
        btnReset.Name = "btnReset";
        btnReset.Size = new Size(85, 28);
        btnReset.TabIndex = 0;
        btnReset.Text = "Сбросить";
        btnReset.UseVisualStyleBackColor = true;
        btnReset.Click += btnReset_Click;
        //
        // SettingsForm
        //
        AcceptButton = btnOk;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = btnCancel;
        ClientSize = new Size(484, 561);
        Controls.Add(propertyGrid);
        Controls.Add(flowButtons);
        MinimizeBox = false;
        Name = "SettingsForm";
        ShowIcon = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Настройки";
        flowButtons.ResumeLayout(false);
        flowButtons.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private PropertyGrid propertyGrid;
    private FlowLayoutPanel flowButtons;
    private Button btnCancel;
    private Button btnOk;
    private Button btnReset;
}
