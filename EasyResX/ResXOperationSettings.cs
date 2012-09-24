
namespace EasyResX
{
    // Class with operation settings for ResXOperator
    public class ResXOperationSettings
    {
        // backup modified resx files
        public bool Backup { get; set; }

        // do not remove missing keys
        public bool AddOnly { get; set; }

        // selected culture to operate on exporting or packaging
        public string SelectedCulture { get; set; }

        // operation mode, see enum for details
        public ResXOperationModes OperationMode { get; set; }
    }
}