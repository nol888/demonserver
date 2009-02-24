namespace DemonClient
{
    using System;
    using System.Windows.Forms;

    internal static class Program
    {
        public static string Server = "127.0.0.1";

        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new DemonClient.Main());
        }
    }
}

