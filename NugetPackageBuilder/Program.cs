using System.Diagnostics;
using BenScr.Serializer;

namespace BenScr.NugetPackageBuilder
{
    public static class Program
    {
        private static Dictionary<string, Action> options = new Dictionary<string, Action>
    {
        { "Set Package Path", SetPackagePath },
        { "Select Package Path", SelectPackagePath  },
        { "Remove Package Path", RemovePackagePath  },
        { "Clear Package Paths", ClearPackagePaths  },
        { "Display Current Package Path", DisplayPackagePath },
        { "Build Package (C#)", BuildPackageCS },
        { "Build Package (CPP)", BuildPackageCPP },
        { "Help", Help },
        { "Exit", Exit }
    };

        private const string SEPERATOR = "--------------------------------------------------------";

        private static bool canExit = false;
        private static HashSet<string> buildedPackagesPaths;
        private static string packagePath;

        private static readonly string MainDirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BenScr", "NugetPackageBuilder");
        private static readonly string BuildedPackagesFilePath = Path.Combine(MainDirPath, "packages.json");
        private static readonly string LastPackageFilePath = Path.Combine(MainDirPath, "data.txt");


        private static void Help()
        {
            Console.WriteLine("This application helps you build NuGet packages for your C# and C++ projects.");
            Console.WriteLine("You can set or select a package path, display the current package path, and build packages.");
            Console.WriteLine("To build a package, ensure that the specified path is valid and points to a project directory.");
            Console.WriteLine("For C# projects, the application uses 'dotnet pack' command.");
            Console.WriteLine("For C++ projects, it uses 'nuget.exe pack' command.");
            Console.WriteLine("Make sure you have the necessary tools installed and configured in your system PATH.");
            PressEnterToContinue();
        }

        private static bool FileOrDirectoryExists(string path)
        {
            return File.Exists(path) || Directory.Exists(path);
        }

        public static void Main(string[] args)
        {
            buildedPackagesPaths = Json.Load(BuildedPackagesFilePath, new HashSet<string>(), Json.FormatedJson);
            Console.Clear();

            if (args.Length > 0)
            {
                string path = args[0];

                if (FileOrDirectoryExists(path))
                    packagePath = path;

                BuildPackageCS();
            }
            else
            {
                packagePath = Json.Load<string>(LastPackageFilePath);
            }

            while (!canExit)
            {
                Console.Clear();
                ShowOptions();
            }

            SavePaths();
        }

        private static void SavePaths()
        {
            Json.Save(BuildedPackagesFilePath, buildedPackagesPaths, Json.FormatedJson);
            Json.Save(LastPackageFilePath, packagePath);
        }

        private static void DisplayPackagePath()
        {
            Console.WriteLine(string.IsNullOrEmpty(packagePath) ? "No path set" : packagePath);
            PressEnterToContinue();
        }

        private static void Exit()
        {
            Console.WriteLine("Are you sure that you want to exit? (Y/N)");
            if(EnteredYes()) canExit = true;
        }

        private static void SelectPackagePath()
        {
            if (buildedPackagesPaths.Count == 0)
            {
                Console.WriteLine("There are no paths");
                PressEnterToContinue();
                return;
            }

            Console.WriteLine($"Select one of the follwing package paths (1-{buildedPackagesPaths.Count})");

            for (int i = 0; i < buildedPackagesPaths.Count; i++)
            {
                var path = buildedPackagesPaths.ElementAt(i);
                Console.WriteLine($"{i + 1}. {new DirectoryInfo(path).Name} ({(File.Exists(path) ? "CPP" : "C#")})");
            }

            int j = 0;
            while (true)
            {
                if (j++ >= 3) return;

                string input = Console.ReadLine();

                if (int.TryParse(input, out int result) && result > 0 && result <= buildedPackagesPaths.Count)
                {
                    packagePath = buildedPackagesPaths.ElementAt(result - 1);
                    break;
                }

                Console.WriteLine("Invalid option!");
                Console.Write($"Re-enter (1-{buildedPackagesPaths.Count}): ");
            }
        }

        private static void RemovePackagePath()
        {
            if (buildedPackagesPaths.Count == 0)
            {
                Console.WriteLine("There are no paths");
                PressEnterToContinue();
                return;
            }

            Console.WriteLine($"Select one of the follwing package paths (1-{buildedPackagesPaths.Count})");

            for (int i = 0; i < buildedPackagesPaths.Count; i++)
            {
                var path = buildedPackagesPaths.ElementAt(i);
                Console.WriteLine($"{i + 1}. {new DirectoryInfo(path).Name} ({(File.Exists(path) ? "CPP" : "C#")})");
            }

            string selectedPath = "";

            int j = 0;
            while (true)
            {
                if (j++ >= 3) return;

                string input = Console.ReadLine();

                if (int.TryParse(input, out int result) && result > 0 && result <= buildedPackagesPaths.Count)
                {
                    selectedPath = buildedPackagesPaths.ElementAt(result - 1);
                    break;
                }

                Console.WriteLine("Invalid option!");
                Console.Write($"Re-enter (1-{buildedPackagesPaths.Count}): ");
            }

            buildedPackagesPaths.Remove(selectedPath);
            Console.WriteLine($"Removed Path \"{selectedPath}\"");
        }

        private static void ClearPackagePaths()
        {
            Console.WriteLine("Are you sure that you want to delete all package paths? (Y/N)");
            if (EnteredYes()) buildedPackagesPaths.Clear();
        }

        private static void BuildPackageCS()
        {
            if (CheckForEmtpyPath()) return;

            try
            {
                Process.Start("cmd.exe", $"/c dotnet pack {packagePath} -c Release");
                Console.WriteLine($"Start building package {new DirectoryInfo(packagePath).Name}");
                buildedPackagesPaths.Add(packagePath);
                SavePaths();
            }
            catch
            {
                Console.WriteLine("Error Occured!");
            }

            PressEnterToContinue();
        }
        private static void BuildPackageCPP()
        {
            if (CheckForEmtpyPath()) return;

            try
            {

                Process.Start(new ProcessStartInfo
                {
                    FileName = @"C:\Tools\nuget.exe",
                    Arguments = $"pack \"{packagePath}\"",
                    WorkingDirectory = Path.GetDirectoryName(packagePath),
                    UseShellExecute = false
                });

                Console.WriteLine($"Start building package {new DirectoryInfo(packagePath).Name}");
                buildedPackagesPaths.Add(packagePath);
                SavePaths();
            }
            catch
            {
                Console.WriteLine("Error Occured!");
            }

            PressEnterToContinue();
        }

        private static bool CheckForEmtpyPath()
        {
            if (packagePath == null)
            {
                Console.WriteLine("Package path is null or empty, build not possible!");
                Console.WriteLine("Would you like to set the path path? (Y/N)");

                if (EnteredYes()) SetPackagePath();
                return true;
            }

            return false;
        }

        public static bool EnteredYes()
        {
            bool enteredYes = false;
            enteredYes = Console.ReadLine().ToLower() == "y";
            return enteredYes;
        }

        private static void SetPackagePath()
        {
            Console.WriteLine("Enter the path of the project directory:");

            string input = Console.ReadLine();

            int i = 0;
            while (!FileOrDirectoryExists(input))
            {
                if (i++ >= 3) return;

                Console.WriteLine("Invalid package Path!");
                Console.Write("Re-Enter the package path: ");
                input = Console.ReadLine();
            }

            packagePath = input;
            Console.WriteLine($"Succesfully set package path to: {new DirectoryInfo(packagePath).Name}");
            PressEnterToContinue();
        }

        private static void PressEnterToContinue()
        {
            Console.WriteLine("...");
            Console.ReadLine();
            Console.CursorTop = System.Math.Max(0, Console.CursorTop - 1);
        }

        private static void ShowOptions()
        {
            Console.WriteLine($"Choose one of the following options (1-{options.Count})");
            Console.WriteLine(SEPERATOR);

            for (int i = 0; i < options.Count; i++)
            {
                var key = options.Keys.ElementAt(i);
                Console.WriteLine($"{i + 1}) {key}");
            }

            Console.WriteLine(SEPERATOR);

            string input = Console.ReadLine();
            Console.Clear();

            if (int.TryParse(input, out int result) && result > 0 && result <= options.Count)
            {
                var key = options.Keys.ElementAt(result - 1);
                Console.WriteLine($"You choose {key}");
                Console.WriteLine(SEPERATOR);

                var action = options[key];
                action();
            }
        }
    }
}