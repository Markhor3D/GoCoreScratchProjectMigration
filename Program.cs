using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using System.Text.RegularExpressions;

namespace GoCoreScratchProjectMigration
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args == null)
            {
                args = new string[0];
                Console.WriteLine("No file provided. Kindly drop a file on me");                
                args = Console.ReadLine()?.Trim('"', '\\', '/').Split(' ') ?? new string[0];
                if (args.Length == 0 || args[0].Length == 0)
                {
                    Console.WriteLine("No file provided. Exiting.");
                    return;
                }
            }
            //args = new string[] { @"C:\Users\umar.hassan\Downloads\Grade 2 Session 8_ Motors in Action.sb3" };
            var keys = new Dictionary<string, string>();
            foreach (var line in File.ReadAllLines("keys.txt"))
            {
                if (line.Trim().Length == 0 || line.StartsWith("#"))
                    continue;
                var parts = line.Split('=');
                if (parts.Length != 2)
                {
                    Console.WriteLine("Invalid key: " + line);
                    continue;
                }
                keys.Add(parts[0].Trim(), parts[1].Trim());
            }
            if (args.Length < 1)
            {
                Console.WriteLine("Drop a file on me: ");
                args = new string[] { Console.ReadLine().Trim('"', '\\', '/') };
            }
            foreach (var arg in args)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Converting \"" + arg+ "\": ");
                try
                {
                    var zipPath = args[0]; // your .sb3 or .zip file

                    using (var zipToOpen = new FileStream(zipPath, FileMode.Open, FileAccess.ReadWrite))
                    using (var archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                    {
                        var projectEntry = archive.Entries.FirstOrDefault(e => e.Name.Equals("project.json", StringComparison.OrdinalIgnoreCase));
                        if (projectEntry != null)
                        {
                            // Step 1: Read content
                            string jsonContent;
                            using (var reader = new StreamReader(projectEntry.Open()))
                            {
                                jsonContent = reader.ReadToEnd();
                            }

                            int replacements = 0;
                            int ReplaceAndCount(ref string input, string search, string replacement)
                            {
                                int count = Regex.Matches(input, Regex.Escape(search)).Count;
                                input = input.Replace(search, replacement);
                                return count;
                            }
                            foreach (var replaceKey in keys)
                            {
                                replacements += ReplaceAndCount(ref jsonContent, replaceKey.Key, replaceKey.Value);
                            }

                            if (replacements > 0)
                            {

                                // Step 3: Delete old entry
                                projectEntry.Delete();


                                // Step 4: Add new entry with same name
                                var newEntry = archive.CreateEntry("project.json");
                                using (var writer = new StreamWriter(newEntry.Open()))
                                {
                                    writer.Write(jsonContent);
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine("Succeeded");
                                }
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("Already patch");
                            }
                        }
                    }
                }
                catch {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Failed");
                }

                Console.ForegroundColor = ConsoleColor.White;
            }
            Console.WriteLine("Done. Press any key to exit.");
            Console.ReadKey();
        }
    }
}
