using System;
using System.Runtime.InteropServices;
using System.Text;

namespace High6
{
    public static class RawPrinterHelper
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private class DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)] public string pDocName;
            [MarshalAs(UnmanagedType.LPStr)] public string pOutputFile;
            [MarshalAs(UnmanagedType.LPStr)] public string pDataType;
        }

        [DllImport("winspool.drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi)]
        private static extern bool OpenPrinter(string pPrinterName, out IntPtr phPrinter, IntPtr pDefault);

        [DllImport("winspool.drv", EntryPoint = "ClosePrinter", SetLastError = true)]
        private static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi)]
        private static extern bool StartDocPrinter(IntPtr hPrinter, int level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

        [DllImport("winspool.drv", EntryPoint = "EndDocPrinter", SetLastError = true)]
        private static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", EntryPoint = "StartPagePrinter", SetLastError = true)]
        private static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", EntryPoint = "EndPagePrinter", SetLastError = true)]
        private static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", EntryPoint = "WritePrinter", SetLastError = true)]
        private static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

        public static bool SendStringToPrinter(string printerName, string zpl)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(zpl);
            IntPtr ptr = Marshal.AllocCoTaskMem(bytes.Length);
            Marshal.Copy(bytes, 0, ptr, bytes.Length);

            try
            {
                return SendBytesToPrinter(printerName, ptr, bytes.Length);
            }
            finally
            {
                Marshal.FreeCoTaskMem(ptr);
            }
        }

        private static bool SendBytesToPrinter(string printerName, IntPtr pBytes, int dwCount)
        {
            if (!OpenPrinter(printerName, out IntPtr hPrinter, IntPtr.Zero))
                return false;

            var di = new DOCINFOA { pDocName = "ZPL Label", pDataType = "RAW" };

            bool success = false;
            if (StartDocPrinter(hPrinter, 1, di))
            {
                if (StartPagePrinter(hPrinter))
                {
                    WritePrinter(hPrinter, pBytes, dwCount, out _);
                    EndPagePrinter(hPrinter);
                    success = true;
                }
                EndDocPrinter(hPrinter);
            }

            ClosePrinter(hPrinter);
            return success;
        }
    }
}