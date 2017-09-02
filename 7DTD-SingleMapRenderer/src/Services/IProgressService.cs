using System;

namespace _7DTD_SingleMapRenderer.Services
{
    /// <summary>
    /// Der ProgressServeice ermöglicht es einem BackgroundTask eine Nachricht an die Oberfläche zu geben.
    /// </summary>
    public interface IProgressService
    {
        void Report(string message);
        void Report(string format, params object[] args);
    }
}
