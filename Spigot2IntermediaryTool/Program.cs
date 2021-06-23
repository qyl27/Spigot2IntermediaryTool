using System;
using System.IO;

namespace Spigot2IntermediaryTool
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("This is a spigot to intermediary mappings generate tool.");
                Console.WriteLine("Usage: dotnet Spigot2IntermediaryTool.dll <minecraftVersion>");
                return;
            }

            var minecraftVersion = args[0];
            
            // Todo: Download Spigot build data and fabric intermediary.
            
            var workingPath = Directory.GetCurrentDirectory();
            var bukkitClassesPath = $"{workingPath}/bukkit-{minecraftVersion}-cl.csrg";
            var bukkitMembersPath = $"{workingPath}/bukkit-{minecraftVersion}-members.csrg";
            var intermediaryPath = $"{workingPath}/{minecraftVersion}.tiny";
            if (!File.Exists(bukkitClassesPath))
            {
                Console.WriteLine("F: Bukkit class mapping is not found.");
                return;
            }

            if (!File.Exists(bukkitMembersPath))
            {
                Console.WriteLine("F: Bukkit member mapping is not found.");
                return;
            }
            
            if (!File.Exists(intermediaryPath))
            {
                Console.WriteLine("F: Intermediary mapping is not found.");
                return;
            }

            var bukkitClasses = File.ReadAllLines(bukkitClassesPath);
            var bukkitMembers = File.ReadAllLines(bukkitMembersPath);
            var intermediary = File.ReadAllLines(intermediaryPath);

            //new Spigot2Intermediary(bukkitClasses, bukkitMembers, intermediary).Run();
            new Intermediary2SpigotSrg(bukkitClasses, bukkitMembers, intermediary).Run();
            new Intermediary2SpigotTiny(bukkitClasses, bukkitMembers, intermediary).Run();
        }
    }
}