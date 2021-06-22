using System;
using System.Collections.Generic;
using System.IO;

namespace Spigot2IntermediaryTool
{
    public class Yarn2SpigotSrg
    {
        private string[] BukkitClasses { get; }
        private string[] BukkitMembers { get; }
        private string[] IntermediaryMerged { get; }
        
        private Dictionary<string, string> BukkitToMojangClasses { get; } = new();
        private Dictionary<string, string> MojangToBukkitClasses { get; } = new();
        private Dictionary<(string clazz, string name, string description), (string bukkitNamed, string description)> MojangToBukkitMembers { get; } = new();
        
        private Dictionary<string, (string intermediary, string named)> IntermediaryClasses { get; } = new();
        private Dictionary<(string clazz, string type, string official), (string intermediary, string named)> IntermediaryFields { get; } = new();
        private Dictionary<(string clazz, string description, string official), (string intermediary, string named)> IntermediaryMethods { get; } = new();

        private List<string> Results { get; } = new();
        
        public Yarn2SpigotSrg(string[] bukkitClasses, string[] bukkitMembers, string[] intermediaryMerged)
        {
            BukkitClasses = bukkitClasses;
            BukkitMembers = bukkitMembers;
            IntermediaryMerged = intermediaryMerged;
        }

        public void Run()
        {
            LoadBukkitClass();
            LoadBukkitMember();
            LoadIntermediary();
            
            MakeSrg();
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
                MojangToBukkitMembers[(BukkitToMojangClasses[memberParts[0]], memberParts[1], memberParts[2])] = (memberParts[3], memberParts[2]);
            }
        }
        private void LoadIntermediary()
        {
            Console.WriteLine("I: Loading intermediary mappings.");
            foreach (var intermediaryLine in IntermediaryMerged)
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

        private void MakeSrg()
        {
            Console.WriteLine("I: Making srg.");
            
            foreach (var intermediaryLine in IntermediaryMerged)
            {
                if (intermediaryLine.StartsWith("CLASS"))
                {
                    var classLine = intermediaryLine.Split("\t");
                    if (!MojangToBukkitClasses.ContainsKey(classLine[1]))
                    {
                        continue;
                    }
                    
                    var result = $"CL: {classLine[3]} {MojangToBukkitClasses[classLine[1]]}";
                    Console.WriteLine(result);
                    Results.Add(result);
                }
                
                // if (intermediaryLine.StartsWith("FIELD"))
                // {
                //     var fieldLine = intermediaryLine.Split("\t");
                //     if (!MojangToBukkitClasses.ContainsKey(fieldLine[1]))
                //     {
                //         continue;
                //     }
                //
                //     if (!MojangToBukkitMembers.ContainsKey((MojangToBukkitClasses[fieldLine[1]], fieldLine[3], fieldLine[2])))
                //     {
                //         continue;
                //     }
                //     
                //     var result = $"FD: {MojangToBukkitClasses[fieldLine[1]]}/{fieldLine[4]} " +
                //                  $"{MojangToBukkitClasses[fieldLine[1]]}/{MojangToBukkitMembers[(MojangToBukkitClasses[fieldLine[1]], fieldLine[3], fieldLine[2])]}";
                //     Console.WriteLine(result);
                //     Results.Add(result);
                // }
                
                if (intermediaryLine.StartsWith("METHOD"))
                {
                    var methodLine = intermediaryLine.Split("\t");
                    if (!MojangToBukkitClasses.ContainsKey(methodLine[1]))
                    {
                        continue;
                    }

                    var result = string.Empty;
                    if (MojangToBukkitMembers.ContainsKey((methodLine[1], methodLine[3], methodLine[2])))
                    {
                        result = $"MD: {IntermediaryClasses[methodLine[1]].named}/{methodLine[3]} " +
                                  $"{ProcessDescriptionToYarn(methodLine[2])} " +
                                  $"{MojangToBukkitClasses[methodLine[1]]}/{MojangToBukkitMembers[(methodLine[1], methodLine[3], methodLine[2])].bukkitNamed} " + 
                                  $"{ProcessDescriptionToBukkit(methodLine[2])}";
                    }
                    else
                    {
                        result = $"MD: {IntermediaryClasses[methodLine[1]].named}/{methodLine[3]} " +
                                     $"{ProcessDescriptionToYarn(methodLine[2])} " +
                                     $"{MojangToBukkitClasses[methodLine[1]]}/{methodLine[3]} " + 
                                     $"{ProcessDescriptionToBukkit(methodLine[2])}";
                    }
                    
                    Console.WriteLine(result);
                    Results.Add(result);
                }
            }
        }
        
        private string ProcessDescriptionToYarn(string description)
        {
            var queue = new Queue<string>();
            for (var i = 0; i < description.Length; i++)
            {
                if (description[i] == 'V'
                    || description[i] == 'Z'
                    || description[i] == 'B'
                    || description[i] == 'C'
                    || description[i] == 'S'
                    || description[i] == 'I'
                    || description[i] == 'J'
                    || description[i] == 'F'
                    || description[i] == 'D'
                    || description[i] == '['
                    || description[i] == '('
                    || description[i] == ')')
                {
                    queue.Enqueue(description[i].ToString());
                }
                else
                {
                    if (description[i] == 'L')
                    {
                        for (var j = i; j < description.Length; j++)
                        {
                            if (description[j] == ';')
                            {
                                var className = description.Substring(i + 1, j - i - 1);
                                if (MojangToBukkitClasses.ContainsKey(className))
                                {
                                    queue.Enqueue($"L{IntermediaryClasses[className]};");
                                }
                                else
                                {
                                    queue.Enqueue($"L{className};");
                                }
                                i = j;
                            }
                        }
                    }
                }
            }

            var result = string.Empty;
            foreach (var q in queue)
            {
                result += q;
            }

            return result;
        }

        private string ProcessDescriptionToBukkit(string description)
        {
            var queue = new Queue<string>();
            for (var i = 0; i < description.Length; i++)
            {
                if (description[i] == 'V'
                    || description[i] == 'Z'
                    || description[i] == 'B'
                    || description[i] == 'C'
                    || description[i] == 'S'
                    || description[i] == 'I'
                    || description[i] == 'J'
                    || description[i] == 'F'
                    || description[i] == 'D'
                    || description[i] == '['
                    || description[i] == '('
                    || description[i] == ')')
                {
                    queue.Enqueue(description[i].ToString());
                }
                else
                {
                    if (description[i] == 'L')
                    {
                        for (var j = i; j < description.Length; j++)
                        {
                            if (description[j] == ';')
                            {
                                var className = description.Substring(i + 1, j - i - 1);
                                if (MojangToBukkitClasses.ContainsKey(className))
                                {
                                    queue.Enqueue($"L{MojangToBukkitClasses[className]};");
                                }
                                else
                                {
                                    queue.Enqueue($"L{className};");
                                }
                                i = j;
                            }
                        }
                    }
                }
            }

            var result = string.Empty;
            foreach (var q in queue)
            {
                result += q;
            }

            return result;
        }
        
        private void Save()
        {
            File.WriteAllLines("mappings.srg", Results);
        }
    }
}