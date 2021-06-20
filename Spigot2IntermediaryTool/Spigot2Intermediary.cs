using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Spigot2IntermediaryTool
{
    public class Spigot2Intermediary
    {
        private string[] BukkitClasses { get; }
        private string[] BukkitMembers { get; }
        private string[] Intermediary { get; }

        private Dictionary<string, string> BukkitToMojangClasses { get; } = new();
        private Dictionary<string, string> MojangToBukkitClasses { get; } = new();
        private Dictionary<(string clazz, string name, string description), string> MojangToBukkitMembers { get; } = new();
        
        private Dictionary<string, (string intermediary, string named)> IntermediaryClasses { get; } = new();
        private Dictionary<(string clazz, string type, string official), (string intermediary, string named)> IntermediaryFields { get; } = new();
        private Dictionary<(string clazz, string description, string official), (string intermediary, string named)> IntermediaryMethods { get; } = new();

        private List<string> Results { get; } = new();
        
        public Spigot2Intermediary(string[] bukkitClasses, string[] bukkitMembers, string[] intermediary)
        {
            BukkitClasses = bukkitClasses;
            BukkitMembers = bukkitMembers;
            Intermediary = intermediary;
        }

        public void Run()
        {
            LoadBukkitClass();
            LoadBukkitMember();
            LoadIntermediary();

            MakeTiny();

            Save();
        }

        private void LoadBukkitClass()
        {
            Console.WriteLine("I: Loading bukkit classes.");
            foreach (var classLine in BukkitClasses)
            {
                if (classLine.StartsWith("#"))
                {
                    continue;
                }
                
                var classMojangToBukkit = classLine.Split(" ");
                Console.WriteLine($"D: Loaded bukkit class {classMojangToBukkit[1]}.");
                BukkitToMojangClasses[classMojangToBukkit[1]] = classMojangToBukkit[0];
                MojangToBukkitClasses[classMojangToBukkit[0]] = classMojangToBukkit[1];
            }
        }

        private void LoadBukkitMember()
        {
            Console.WriteLine("I: Loading bukkit members.");
            foreach (var memberLine in BukkitMembers)
            {
                if (memberLine.StartsWith("#"))
                {
                    continue;
                }

                var memberParts = memberLine.Split(" ");
                Console.WriteLine($"D: Loaded bukkit member {memberParts[3]}.");
                MojangToBukkitMembers[(BukkitToMojangClasses[memberParts[0]], memberParts[1], memberParts[2])] = memberParts[3];
            }
        }
        
        private void LoadIntermediary()
        {
            Console.WriteLine("I: Loading intermediary mappings.");
            foreach (var intermediaryLine in Intermediary)
            {
                if (intermediaryLine.StartsWith("CLASS"))
                {
                    var classLine = intermediaryLine.Split("\t");
                    Console.WriteLine($"D: Loaded intermediary class {classLine[2]}.");
                    IntermediaryClasses[classLine[1]] = (classLine[2], classLine[3]);
                }

                if (intermediaryLine.StartsWith("FIELD"))
                {
                    var fieldLine = intermediaryLine.Split("\t");
                    Console.WriteLine($"D: Loaded intermediary field {fieldLine[4]}");
                    IntermediaryFields[(fieldLine[1], fieldLine[2], fieldLine[3])] = (fieldLine[4], fieldLine[5]);
                }

                if (intermediaryLine.StartsWith("METHOD"))
                {
                    var methodLine = intermediaryLine.Split("\t");
                    Console.WriteLine($"D: Loaded intermediary method {methodLine[4]}");
                    IntermediaryMethods[(methodLine[1], methodLine[2], methodLine[3])] = (methodLine[4], methodLine[5]);
                }
            }
        }
        
        private void MakeTiny()
        {
            Console.WriteLine("I: Making tiny.");
            var tiny = new List<string>();
            tiny.Add("v1\tofficial\tbukkit\tintermediary\tnamed");

            foreach (var intermediaryLine in Intermediary)
            {
                if (intermediaryLine.StartsWith("CLASS"))
                {
                    var classLine = intermediaryLine.Split("\t");

                    var mojang = classLine[1];
                    if (MojangToBukkitClasses.ContainsKey(mojang))
                    {
                        var result =
                            $"CLASS\t{mojang}\t{MojangToBukkitClasses[mojang]}\t{IntermediaryClasses[mojang].intermediary}\t{IntermediaryClasses[mojang].named}";
                        Console.WriteLine($"D: Processed {result}");
                        tiny.Add(result);
                    }
                }

                if (intermediaryLine.StartsWith("FIELD"))
                {
                    var fieldLine = intermediaryLine.Split("\t");

                }
            }

            Results.AddRange(tiny);
        }
        
        private void Save()
        {
            File.WriteAllLines("mappings.tiny", Results);
        }
    }
}