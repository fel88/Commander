using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;

namespace commander
{
    //internal static class NativeMethods
    //{
    //    internal const uint FileAccessGenericRead = 0x80000000;
    //    internal const uint FileShareWrite = 0x2;
    //    internal const uint FileShareRead = 0x1;
    //    internal const uint CreationDispositionOpenExisting = 0x3;
    //    internal const uint IoCtlDiskGetDriveGeometry = 0x70000;
    //    // Win32 constants for accessing files.
    //    internal const uint GENERIC_READ = unchecked((int)0x80000000);

    //    internal const int FILE_FLAG_BACKUP_SEMANTICS = unchecked((int)0x02000000);

    //    internal const int OPEN_EXISTING = unchecked((int)3);

    //    [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    //    public static extern SafeFileHandle CreateFile(
    //        string fileName,
    //        uint fileAccess,
    //        uint fileShare,
    //        IntPtr securityAttributes,
    //        uint creationDisposition,
    //        uint flags,
    //        IntPtr template);

    //    [DllImport("Kernel32.dll", SetLastError = false, CharSet = CharSet.Auto)]
    //    public static extern int DeviceIoControl(
    //        SafeFileHandle device,
    //        uint controlCode,
    //        IntPtr inBuffer,
    //        uint inBufferSize,
    //        IntPtr outBuffer,
    //        uint outBufferSize,
    //        ref uint bytesReturned,
    //        IntPtr overlapped);
    //}
}

