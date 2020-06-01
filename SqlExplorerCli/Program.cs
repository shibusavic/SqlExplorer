using SqlExplorer.MsSqlServer;
using SqlExplorer.Reports;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SqlExplorerCli
{
    class Program
    {
        static readonly string newline = Environment.NewLine;
        static bool showHelp = false;
        static string outputDirectory;
        static bool overwriteFiles = false;
        static string connectionString;

        static async Task Main(string[] args)
        {
            int exitCode = 0;
            HandleArgs(args);

            try
            {
                if (showHelp)
                {
                    ShowHelp();
                }
                else
                {
                    Validate();

                    var db = await DatabaseFactory.CreateAsync(connectionString);

                    await GenerateReportAsync<DependencyReport>(db, outputDirectory, overwriteFiles);
                    await GenerateReportAsync<TablesReport>(db, outputDirectory, overwriteFiles);
                    await GenerateReportAsync<ViewsReport>(db, outputDirectory, overwriteFiles);
                    await GenerateReportAsync<RoutinesReport>(db, outputDirectory, overwriteFiles);

                }
            }
            catch (Exception exc)
            {
                exitCode = -1;
                ShowHelp(exc.Message);
            }
            finally
            {
                Environment.Exit(exitCode);
            }
        }

        static async Task GenerateReportAsync<T>(Database database, string directoryName, bool overwriteFiles) where T : ReportBase
        {
            T report = (T)Activator.CreateInstance(typeof(T), database, directoryName, overwriteFiles);
            await report.GenerateAsync();
        }

        static void HandleArgs(string[] args)
        {
            for (int a = 0; a < args.Length; a++)
            {
                string argument = args[a].ToLower();

                switch (argument)
                {
                    case "--connection-string":
                    case "-c":
                        if (a >= args.Length - 1) { throw new ArgumentException($"Expecting a connection string after {args[a]}"); }
                        connectionString = args[++a];
                        break;
                    case "--output-directory":
                    case "-d":
                        if (a >= args.Length - 1) { throw new ArgumentException($"Expecting a directory after {args[a]}"); }
                        outputDirectory = args[++a];
                        break;
                    case "--overwrite":
                    case "-o":
                        overwriteFiles = true;
                        break;
                    case "--help":
                    case "-h":
                    case "?":
                        showHelp = true;
                        break;
                    default:
                        throw new ArgumentException($"Unknown argument: {args[a]}");
                }
            }
        }

        static void Validate()
        {
            if (string.IsNullOrWhiteSpace(connectionString)) { throw new ArgumentException("Connection string is required. Use -c."); }
            if (string.IsNullOrWhiteSpace(outputDirectory)) { throw new ArgumentException("Output directory is required. Use -d."); }

            while (outputDirectory.EndsWith("/") || outputDirectory.EndsWith("\\"))
            {
                outputDirectory = outputDirectory[0..^1];
            }

            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }
        }

        static void ShowHelp(string message = null)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                Console.WriteLine($"{newline}Error: {message}");
            }

            Dictionary<string, string> helpDefinitions = new Dictionary<string, string>()
            {
                { "{--connection-string | -c} <connection string>","Define the connection string." },
                { "{--output-directory | -d} <directory>]","Define the output directory." },
                { "[--overwrite | -o]","Overwrite output files if they exists." },
                { "[--help | -h | ?]","Show this help." }
            };

            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

            int maxKeyLength = helpDefinitions.Keys.Max(k => k.Length) + 1;

            Console.WriteLine($"{newline}{assemblyName} {string.Join(' ', helpDefinitions.Keys)}{newline}");

            foreach (var helpItem in helpDefinitions)
            {
                Console.WriteLine($"{helpItem.Key.PadRight(maxKeyLength)}\t{helpItem.Value}");
            }

            Console.WriteLine($"{newline}{newline}Usages:{newline}");
            Console.WriteLine($"To generate reports for a given database:");
            Console.WriteLine($"\t{assemblyName} -d /c/temp/db -c \"connection string\"");
            Console.WriteLine($"{Environment.NewLine}To ensure created files are overwritten:");
            Console.WriteLine($"\t{assemblyName} -d /c/temp/db -c \"connection string\" -o");
        }
    }
}
