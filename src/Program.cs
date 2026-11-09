using System;
using System.IO;

namespace RestoryItalianPatch
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length < 2 || (args[0] != "install" && args[0] != "uninstall"))
            {
                Console.Error.WriteLine("Usage: RestoryItalianPatch.exe install <game-folder> [payload-folder]");
                Console.Error.WriteLine("       RestoryItalianPatch.exe uninstall <game-folder>");
                return 2;
            }

            try
            {
                var game = args[1];
                var payload = args.Length > 2 ? args[2] : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "payload");
                var engine = new InstallerEngine(game, payload);
                if (args[0] == "install")
                {
                    engine.Install();
                    Console.WriteLine("Italian patch installed or already current.");
                }
                else
                {
                    engine.Uninstall();
                    Console.WriteLine("Italian patch removed; original files restored where applicable.");
                }
                return 0;
            }
            catch (UnsupportedGameException ex)
            {
                Console.Error.WriteLine("Compatibility check failed: " + ex.Message);
                return 3;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Installer error: " + ex.Message);
                return 1;
            }
        }
    }
}
