using System;

namespace EasyResX
{
    // Processing end event handler
    public delegate void ProcessingEndedEventHandler(object sender, ProcessingEndedEventArgs e);

    // Processing event args class
    public class ProcessingEndedEventArgs : EventArgs
    {
        // operation mode for processing
        public ResXOperationModes OperationMode { get; private set; }

        // selected culture if present
        public string SelectedCulture { get; private set; }

        // ctor
        public ProcessingEndedEventArgs(ResXOperationModes operationMode, string selectedCulture)
        {
            this.OperationMode = operationMode;
            this.SelectedCulture = selectedCulture;
        }
    }
}