using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows.Forms;

namespace EZVHD
{
    class Program
    {
        static void Main(string[] args)
        {

            bool isElevated;
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }

            if (args.Length == 0)
            {
                MessageBox.Show("引数がありません。\nコンテキストメニューから実行してください。", "EzVHD Tools", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.Exit(1);
            }
            int mode;
            if (!int.TryParse(args[0], out mode))
            {
                MessageBox.Show("引数が正しくありません。\nコンテキストメニューから実行してください。", "EzVHD Tools", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string filePath = Path.GetFullPath(args[1]);
            FileInfo fi = new FileInfo(filePath);
            if (!isElevated)
            {
                RunAsAdmin(mode, NetworkPathConverter.GetUniversalName(filePath));
                return;
            }
            bool exists = fi.Exists;
            if (!exists)
            {
                MessageBox.Show("ファイルが存在しません。", "EzVHD Tools", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.Exit(1);
            }

            if (Checker.IsVHDMounted(filePath))
            {
                MessageBox.Show("すでにマウントされています。", "EzVHD Tools", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.Exit(1);
            }

            switch (mode)
            {
                case 1:
                    //Attach
                    AttachVdisk(1, filePath);
                    break;
                case 2:
                    //Attach ReadOnly
                    AttachVdisk(2, filePath);
                    break;
                default:
                    MessageBox.Show("指定された引数は存在しません。", "EzVHD Tools", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
            }

        }

        const int NO_ERROR = 0;
        const int ERROR_NOT_SUPPORTED = 50;
        const int ERROR_MORE_DATA = 234;
        const int ERROR_BAD_DEVICE = 1200;
        const int ERROR_CONNECTION_UNAVAIL = 1201;
        const int ERROR_NO_NET_OR_BAD_PATH = 1203;
        const int ERROR_EXTENDED_ERROR = 1208;
        const int ERROR_NO_NETWORK = 1222;
        const int ERROR_NOT_CONNECTED = 2250;

        public static class NetworkPathConverter
        {
            [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
            public static extern int WNetGetUniversalName(string lpLocalPath, int dwInfoLevel, IntPtr lpBuffer, ref int lpBufferSize);

            public static string GetUniversalName(string path)
            {
                if (path.StartsWith(@"\\"))
                {
                    return path;
                }

                int buf = 1024;
                IntPtr lp_c = Marshal.AllocCoTaskMem(buf);

                if (WNetGetUniversalName(path, 1, lp_c, ref buf) != NO_ERROR)
                {
                    return path;
                }

                int bufferSize = 1;
                IntPtr lp_dummy = Marshal.AllocCoTaskMem(bufferSize);
                WNetGetUniversalName(path, 1, lp_dummy, ref bufferSize);
                if (bufferSize == 0)
                {
                    throw new Win32Exception();
                }

                IntPtr buffer = IntPtr.Zero;
                try
                {
                    buffer = Marshal.AllocCoTaskMem(bufferSize);
                    if (WNetGetUniversalName(path, 1, buffer, ref bufferSize) != 0)
                    {
                        throw new Win32Exception();
                    }

                    string universalName = Marshal.PtrToStringUni(buffer);
                    if (universalName.StartsWith(@"\\"))
                    {
                        return universalName;
                    }
                    else
                    {
                        int index = universalName.IndexOf(@"\");
                        if (index != -1 && index < universalName.Length - 1)
                        {
                            return universalName.Substring(index);
                        }
                        else
                        {
                            throw new InvalidOperationException("Invalid universal name");
                        }
                    }
                }
                finally
                {
                    if (buffer != IntPtr.Zero)
                    {
                        Marshal.FreeCoTaskMem(buffer);
                    }
                }
            }
        }

        static void RunAsAdmin(int mode, string filePath)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = System.Reflection.Assembly.GetEntryAssembly().Location;
            psi.Arguments = string.Format("{0} {1}", mode, filePath);
            psi.Verb = "runas";
            try
            {
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void AttachVdisk(int mode, string filePath)
        {
            ProcessStartInfo diskpartProcess = new ProcessStartInfo("diskpart.exe");
            diskpartProcess.UseShellExecute = false;
            diskpartProcess.CreateNoWindow = true;
            diskpartProcess.RedirectStandardInput = true;
            diskpartProcess.RedirectStandardOutput = true;

            Process proc = Process.Start(diskpartProcess);
            if (mode == 1)
            {
                proc.StandardInput.WriteLine("select vdisk FILE=\"{0}\"", filePath);
                proc.StandardInput.WriteLine("attach vdisk");
                proc.StandardInput.WriteLine("exit");
                proc.StandardInput.Flush();
                proc.StandardInput.Close();

                proc.WaitForExit();
            }
            if (mode == 2)
            {
                proc.StandardInput.WriteLine("select vdisk FILE=\"{0}\"", filePath);
                proc.StandardInput.WriteLine("attach vdisk readonly");
                proc.StandardInput.WriteLine("exit");
                proc.StandardInput.Flush();
                proc.StandardInput.Close();

                proc.WaitForExit();
            }
        }

        public static class Checker
        {
            public static bool IsVHDMounted(string vhdPath)
            {
                ProcessStartInfo psi = new ProcessStartInfo("powershell.exe");
                psi.Arguments = $"Get-DiskImage -ImagePath \"{vhdPath}\" | Select -ExpandProperty Attached";
                psi.RedirectStandardOutput = true;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                Process process = Process.Start(psi);
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                bool IsMounted = bool.Parse(output);
                return IsMounted;
            }
        }
    }
}

