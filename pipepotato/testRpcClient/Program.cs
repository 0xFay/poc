using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using rpc_201ef99a_7fa0_444c_9399_19ba84f12a1a_1_0;
using System.Runtime.InteropServices;
using NtApiDotNet.Ndr.Marshal;
using NtApiDotNet.Win32;
using NtApiDotNet;
using System.ComponentModel;
using System.ServiceProcess;

namespace testRpcClient
{                           //testRpcClient.exe "C:\\Windows\\System32\\cmd.exe"
    class Program
    {
        [DllImport("User32.dll")]
        static extern IntPtr GetDesktopWindow();

        [Flags]
        enum StartFlags
        {
            None = 0,
            RunAsAdmin = 0x1,
            Unknown02 = 0x2,
            Unknown04 = 0x4,
            Wow64Path = 0x8,
            Unknown10 = 0x10,
            Unknown20 = 0x20,
            Unknown40 = 0x40,
            Untrusted = 0x80,
            CentennialElevation = 0x200,
        }

        static NtProcess LaunchAdminProcess(string executable, string cmdline, StartFlags flags, CreateProcessFlags create_flags, string desktop)
        {
            StartAppinfoService();

            using (Client client = new Client())
            {
                client.Connect();
                create_flags |= CreateProcessFlags.UnicodeEnvironment;
                Struct_0 start_info = new Struct_0();
                int retval = client.RAiLaunchAdminProcess(executable, cmdline, (int)flags, (int)create_flags,
                    @"c:\windows", desktop, start_info, new NdrUInt3264(GetDesktopWindow()),
                    -1, out Struct_2 proc_info, out int elev_type);
                if (retval != 0)
                {
                    throw new Win32Exception(retval);
                }

                using (var thread = NtThread.FromHandle(new IntPtr(proc_info.Member8.Value)))
                {
                    return NtProcess.FromHandle(new IntPtr(proc_info.Member0.Value));
                }
            }
        }

        static void StartAppinfoService()
        {
            try
            {
                ServiceController service = new ServiceController("appinfo");
                if (service.Status != ServiceControllerStatus.Running)
                {
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(5));
                }
            }
            catch
            {
            }
        }

        static void Main(string[] args)
        {
            try
            {
                StartFlags flags = StartFlags.RunAsAdmin;
                List<string> cmds = new List<string>(args);

                if (cmds.Count > 0 && cmds[0].Equals("-l"))
                {
                    flags = StartFlags.None;
                    cmds.RemoveAt(0);
                }

                if (cmds.Count < 1)
                {
                    Console.WriteLine("[Usage] LaunchAdminProcess [-l] executable [command line]");
                    Console.WriteLine("Specify -l to start process as non-admin");
                    Environment.Exit(1);
                }

                string executable = cmds[0];
                string commandline = executable;
                if (cmds.Count > 1)
                    commandline = cmds[1];

                using (var proc = LaunchAdminProcess(executable, commandline, flags, CreateProcessFlags.None, @"WinSta0\Default"))
                {
                    Console.WriteLine("Start process {0}", proc.ProcessId);
                    Console.WriteLine("Granted Access: {0}", proc.GrantedAccess);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
