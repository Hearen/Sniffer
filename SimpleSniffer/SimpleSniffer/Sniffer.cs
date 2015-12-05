using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace SimpleSniffer
{
    public class Sniffer
    {
        [STAThread]
        public static void Main()
        {
             /**
              * if the current operation is done by an administrator, run it directly
              * if not, ask for the administrator's authority;
              */
             //get the current user ID;
             System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
             Application.EnableVisualStyles();
             System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);

             //whether the current operation is executed by administrators or not;
             if (principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator))
             {
                 //if this were run by administrators, run it directly;
                 Application.Run(new MainForm());
             }
             else
             {
                 //build up the booting object;
                 System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                 startInfo.UseShellExecute = true;
                 startInfo.WorkingDirectory = Environment.CurrentDirectory;
                 startInfo.FileName = Application.ExecutablePath;
                 //make sure this is run by administrator;
                 startInfo.Verb = "runas";
                 try
                 {
                     System.Diagnostics.Process.Start(startInfo);
                 }
                 catch
                 {
                     return;
                 }
                 Application.Exit();
             }
        }
    }
}
