using System;
using System.Collections.Generic;

namespace Spigot2IntermediaryTool
{
    public class Spigot2Intermediary
    {
        private string[] BukkitClasses { get; }
        private string[] BukkitMembers { get; }
        private string[] Intermediary { get; }

        private Dictionary<string, string> BukkitToMojang { get; } = new();
        private Dictionary<string, string> MojangToBukkit { get; } = new();
        
        private Dictionary<string, string> IntermediaryClasses { get; } = new();
        // private Dictionary<string, string> IntermediaryFields { get; } = new();
        // private Dictionary<string, string> IntermediaryMethods { get; } = new();
        
        public Spigot2Intermediary(string[] bukkitClasses, string[] bukkitMembers, string[] intermediary)
        {
            BukkitClasses = bukkitClasses;
            BukkitMembers = bukkitMembers;
            Intermediary = intermediary;
        }

        public void Run()
        {
            LoadBukkit();
            LoadIntermediary();

            MakeTiny();
        }

        private void LoadBukkit()
        {
            Console.WriteLine("I: Loading bukkit classes.");
            foreach (var classLine in BukkitClasses)
            {
                if (!classLine.StartsWith("#"))
                {
                    var classMojangToBukkit = classLine.Split(" ");
                    Console.WriteLine($"D: Loaded bukkit {classMojangToBukkit[1]}.");
                    BukkitToMojang[classMojangToBukkit[1]] = classMojangToBukkit[0];
                    MojangToBukkit[classMojangToBukkit[0]] = classMojangToBukkit[1];
                }
            }
        }
        
        private void LoadIntermediary()
        {
            Console.WriteLine("I: Loading intermediary mappings.");
            foreach (var intermediaryLine in Intermediary)
            {
                if (intermediaryLine.StartsWith("CLASS"))
                {
                    var classParts = intermediaryLine.Split("\t");
                    Console.WriteLine($"D: Loaded intermediary {classParts[2]}.");
                    IntermediaryClasses[classParts[1]] = classParts[2];
                }
            }
        }
        
        private void MakeTiny()
        {
            Console.WriteLine("I: Making tiny.");
            var tiny = new List<string>();
            tiny.Add("v1\tbukkit\tbukkitIntermediary");

            foreach (var intermediaryLine in Intermediary)
            {
                if (intermediaryLine.StartsWith("CLASS"))
                {
                    var classDescription = intermediaryLine.Split("\t");

                    var mojang = classDescription[1];
                    if (MojangToBukkit.ContainsKey(mojang))
                    {
                        tiny.Add($"CLASS\t{MojangToBukkit[mojang]}\t{IntermediaryClasses[mojang]}");
                    }
                }
            }

            foreach (var t in tiny)
            {
                Console.WriteLine(t);
            }
        }
    }
}