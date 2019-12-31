using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using _7DTD_SingleMapRenderer.Core;
using _7DTD_SingleMapRenderer.CommandLine;
using _7DTD_SingleMapRenderer.Presentation.Windows;
using _7DTD_SingleMapRenderer.Services;
using _7DTD_SingleMapRenderer.Settings;

namespace _7DTD_SingleMapRenderer
{
    public class Program
    {
        #region PInvoke
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetActiveWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AttachConsole(int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool FreeConsole();

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
        #endregion

        static void ShowConsoleWindow()
        {
            var handle = GetConsoleWindow();
            if (handle == IntPtr.Zero)
                AllocConsole();
            else
                ShowWindow(handle, SW_SHOW);
        }

        static void HideConsoleWindow()
        {
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);
        }

        [STAThread]
        public static int Main(string[] args)
        {
            // Applikation ist "Windows-Anwendung" -> losgelöst von bestehender Konsole
            // App wird geladen, damit der MapRenderer Zugriff auf die Ressourcen hat.
            App app = new App();
            if (args != null && args.Length > 0)
            {
                bool attached = false;
                bool helpRequested = false;
                try
                {
                    // mit bestehender Konsole verbinden, sonst neue erstellen
                    attached = AttachConsole(-1);
                    if (!attached)
                        ShowConsoleWindow();

                    // weil gelöst von aktueller Konsole war, steht jetzt der Prompt in der Zeile -> Zeile leeren
                    int pos = Console.CursorLeft;
                    Console.CursorLeft = 0;
                    Console.Write(new String(' ', pos));
                    Console.CursorLeft = 0;

                    // Argumente parsen
                    AppSettings set = CommandLine<AppSettings>.Parse(args);
                    // wenn Hilfe angefordert, dann diese ausgeben
                    if (args.Contains("/?") || args.Contains("--help") || args.Contains("-h"))
                    {
                        CommandLine<AppSettings>.PrintHelp();
                        helpRequested = true;
                    }
                    else
                    {
                        IProgressService progress = new ConsoleOutput();
                        var mapfilename = System.IO.Path.GetFileNameWithoutExtension(set.MapFilePath);
                        var mapdirectory = System.IO.Path.GetDirectoryName(set.MapFilePath);
                        if (String.IsNullOrEmpty(set.ImageFilePath))
                        {
                            set.ImageFilePath = System.IO.Path.Combine(mapdirectory, mapfilename + ".png");
                        }

                        IEnumerable<POI> pois;
                        IEnumerable<PrefabPOI> prefabPois;
                        int height, width;
                        string worldFolderPath;
                        List<SevenDaysSaveManipulator.Data.Prefab> prefabs = null;
                        Util.Helper.GetPois(set, out pois);
                        Util.Helper.GetPrefabPoisAndMapInfo(set, ref prefabs, out prefabPois, out height, out width, out worldFolderPath);

                        var map = new MapRenderer(set);
                        map.RenderWholeMap(set.MapFilePath,
                            set.ImageFilePath,
                            pois,
                            prefabPois,
                            worldFolderPath,
                            height,
                            width,
                            progress);
                    }
                }
                catch (Exception ex)
                {
                    var oldfore = Console.ForegroundColor;
                    var oldback = Console.BackgroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.WriteLine(ex.Message);
                    Console.ForegroundColor = oldfore;
                    Console.BackgroundColor = oldback;
                }
                finally
                {
                    /*
                     * Der auskommentierte Code sollte das Problem beheben, dass die Konsole nicht wieder auf den Prompt zurückspringt.
                     * Grund für das Verhalten: Applikation ist eine "Windows-Anwendung"
                     * Das Verhalten des Workarounds ist aber unlogisch, da einfach die Konsole aktiviert wird und damit das aktuelle Fenster seinen Fokus verliert.
                     * Besonders nervig, falls man grade in einem anderen Fenster etwas tippt.
                     * 
                     * Wird die Anwendung über einen Shortcut gestartet, dann tritt das Verhalten nicht auf und die Konsole schließt sich automatisch.
                    */

                    //var hnd = GetConsoleWindow();
                    //bool res = false;
                    //if (hnd != IntPtr.Zero)
                    //    res = SetForegroundWindow(hnd);

                    if (!helpRequested)
                        Console.WriteLine("Please press Enter.");

                    if (attached)
                        FreeConsole();

                    //if (res != false)
                    if (helpRequested)
                    {
                        // normalen prompt wieder holen
                        // HACK from http://stackoverflow.com/questions/1305257/using-attachconsole-user-must-hit-enter-to-get-%20regular-command-line/2463039#2463039
                        System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                    }
                }
            }
            else
            {
                app.Run(new MainWindow());
            }
            app.Shutdown(0);
            return 0;
        }

        class ConsoleOutput : IProgressService
        {

            #region IProgressService Member

            public void Report(string message)
            {
                Console.WriteLine(message);
            }

            public void Report(string format, params object[] args)
            {
                Report(String.Format(format, args));
            }

            #endregion
        }
    }
}
