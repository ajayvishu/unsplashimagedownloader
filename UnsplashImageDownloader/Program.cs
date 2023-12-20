using System;
using System.Windows.Forms;

namespace UnsplashImageDownloader
{
    static class Program
    {
        //33333
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Unsplash());
        }
    }
}
