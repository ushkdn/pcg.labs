namespace Presentation;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        var winApiForm = new WinApiForm();
        winApiForm.Run();

        //ApplicationConfiguration.Initialize();
        //System.Windows.Forms.Application.Run(new Form1());
    }
}