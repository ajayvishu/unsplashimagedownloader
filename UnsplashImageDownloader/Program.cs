using System;
using System.Windows.Forms;

namespace UnsplashImageDownloader
{
    static class Program
    {
        //5555
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Unsplash());
        }
    }
}
