using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReplaceStringOptions
{
    internal class Utils
    {
        internal static string[] TextFileExtensions =
        [
            ".zip", ".rar", ".7z", ".tar", ".gz", ".bz2", ".xz", ".tar.gz", ".tar.bz2", ".tar.xz", ".tgz", ".tbz2", ".txz", ".cab",
            ".iso", ".img", ".dmg", ".apk", ".ipa", ".xpi", ".deb", ".rpm", ".msi", ".appimage", ".snap", ".pkg",
            ".exe", ".dll", ".so", ".dylib", ".bin", ".out", ".app", ".elf", ".class", ".jar", ".war", ".ear", ".msi", ".cpl",
            ".sys", ".drv", ".vxd", ".scr", ".o", ".a", ".lib", ".nupkg", ".whl",
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".tif", ".webp", ".heic", ".heif", ".ico", ".cur", ".svgz", ".psd", ".xcf",
            ".ai", ".eps", ".cdr", ".sketch", ".indd",
            ".mp3", ".wav", ".flac", ".aac", ".ogg", ".opus", ".wma", ".m4a", ".amr", ".aiff", ".au", ".mid", ".midi", ".ra", ".vox",
            ".mp4", ".mkv", ".avi", ".mov", ".wmv", ".flv", ".webm", ".mpg", ".mpeg", ".ogv", ".3gp", ".m4v", ".ts", ".vob",
            ".db", ".sqlite", ".mdb", ".accdb", ".sqlitedb", ".fdb", ".gdb", ".ib", ".ndf", ".ldf", ".frm", ".myd", ".myi", ".parquet",
            ".avro",
            ".iso", ".img", ".vhd", ".vhdx", ".vdi", ".vmdk", ".qcow2", ".dmg",
            ".stl", ".obj", ".fbx", ".blend", ".dae", ".glb", ".gltf", ".3ds", ".max", ".step", ".stp", ".iges", ".igs", ".dwg", ".dxf",
            ".iso", ".bin", ".cue", ".nrg", ".gcm", ".gba", ".nds", ".sfc", ".smc", ".nes", ".z64", ".v64", ".n64", ".psx", ".cso",
            ".pak", ".wad", ".vpk", ".xci", ".nsp",
        ];

        internal static void WriteOnlyCharacter(char character, int repeatInLine)
        {
            Console.WriteLine(string.Concat("\n", new string(character, repeatInLine)));
        }

        internal static void ConsoleSuccessText()
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("\n**********SUCCESS**********\n");
            Console.ResetColor();
        }

        internal static void ConsoleWarningText()
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("\n**********WARNING**********\n");
            Console.ResetColor();
        }

        internal static void ConsoleErrorText()
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("\n**********ERROR**********\n");
            Console.ResetColor();
        }

        internal static string ReturnReadLineConsole(string writeLine, string write)
        {
            Console.WriteLine(writeLine);
            Console.Write(write);
            return Console.ReadLine()?.Trim()!;
        }
    }
}
