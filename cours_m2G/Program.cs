namespace cours_m2G
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            //  Form1 F1 = new Form1();
            Form F = new MainForm();
                Application.Run(F);
         

        }
    }
}