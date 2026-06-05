namespace PdfTool;

static class Program
{
    [STAThread]
    static void Main()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            File.WriteAllText("crash.log", e.ExceptionObject.ToString());

        Application.ThreadException += (_, e) =>
            File.WriteAllText("crash.log", e.Exception.ToString());

        try
        {
            // Let Windows scale the entire window — layout stays correct at any DPI
            Application.SetHighDpiMode(HighDpiMode.DpiUnaware);
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
        catch (Exception ex)
        {
            File.WriteAllText("crash.log", ex.ToString());
            MessageBox.Show(ex.ToString(), "Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
