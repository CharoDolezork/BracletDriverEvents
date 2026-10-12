namespace BracletDriver;
internal partial class SettingsForm : Form
{
    private readonly Options _options;

    public SettingsForm(Options options)
    {
        InitializeComponent();
        _options = options;
        propertyGrid.SelectedObject = options.Clone();
    }

    private void btnReset_Click(object sender, EventArgs e) => propertyGrid.SelectedObject = new Options();

    private void btnOk_Click(object sender, EventArgs e)
    {
        _options.CopyFrom((Options)propertyGrid.SelectedObject);
        DialogResult = DialogResult.OK;
    }
}
