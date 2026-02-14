using System.Runtime.InteropServices;
using Dalamud.Utility;

public static class OSHelper
{
    public static bool IsWindows() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !Util.IsWine();
    public static bool IsLinux() => RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || Util.IsWine();
    public static bool IsMacOS() => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
}