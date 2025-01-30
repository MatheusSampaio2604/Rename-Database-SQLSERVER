using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RenameDatabaseSQLSERVER
{
    internal class Utils
    {
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
