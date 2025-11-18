using System.Diagnostics;
using BenScr.Serialization.Json;

namespace BenScr.NugetPackageBuilder;
public static class Program
{
    private static Dictionary<string, Action> options = new Dictionary<string, Action>
    {
        {"Set Package Path", SetPackagePath },
        { "Select Package Path", SelectPackagePath  },
        { "Display Current Package Path", DisplayPackagePath },
        {"Build Package", BuildPackage },
        { "Save Directory Path" , SaveDirectoryPath},
        {"Exit", Exit }
    };

    private static bool canExit = false;
    private static HashSet<string> buildedPackagesPaths;
    private static string packageDirPath;
    private static readonly string MAIN_FOLDER_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BenScr");
    private static readonly string BUILDED_PACKAGES_FILE_PATH = Path.Combine(MAIN_FOLDER_PATH, "NugetPackageBuilder", "packages.json");

    public static void Main(string[] args)
    {
        buildedPackagesPaths = Json.Load(BUILDED_PACKAGES_FILE_PATH, new HashSet<string>(), Json.FormatedJson);
        Console.Clear();

        if (args.Length > 0)
        {
            if (Directory.Exists(args[0]))
                packageDirPath = args[0];

            BuildPackage();
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
        Json.Save(BUILDED_PACKAGES_FILE_PATH, buildedPackagesPaths, Json.FormatedJson);
    }

    private static void DisplayPackagePath()
    {
        Console.WriteLine(packageDirPath);
        PressEnterToContinue();
    }

    private static void SaveDirectoryPath()
    {

    }

    private static void Exit()
    {
        canExit = true;
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
            Console.WriteLine($"{i + 1}. {new DirectoryInfo(path).Name}");
        }

        while (true)
        {
            string input = Console.ReadLine();

            if (int.TryParse(input, out int result) && result > 0 && result <= buildedPackagesPaths.Count)
            {
                packageDirPath = buildedPackagesPaths.ElementAt(result - 1);
                break;
            }

            Console.WriteLine("Invalid option!");
            Console.Write($"Re-enter (1-{buildedPackagesPaths.Count}): ");
        }
    }

    private static void BuildPackage()
    {
        if (packageDirPath == null)
        {
            Console.WriteLine("Package directory is null or empty, build not possible!");
            Console.WriteLine("Would you like to set the directory path? (Y/N)");

            if (EnteredYes())
            {
                SetPackagePath();
            }

            return;
        }

        try
        {
            Process.Start("cmd.exe", $"/c dotnet pack {packageDirPath} -c Release");
            Console.WriteLine($"Start building package {new DirectoryInfo(packageDirPath).Name}");
            buildedPackagesPaths.Add(packageDirPath);
            SavePaths();
        }
        catch
        {
            Console.WriteLine("Error Occured!");
        }

        PressEnterToContinue();
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
        while (!Directory.Exists(input))
        {
            Console.WriteLine("Invalid directory Path!");
            Console.Write("Re-Enter the directory path: ");
            input = Console.ReadLine();
        }

        packageDirPath = input;
        Console.WriteLine($"Succesfully set package directory to: {new DirectoryInfo(packageDirPath).Name}");
        PressEnterToContinue();
    }

    private static void PressEnterToContinue()
    {
        Console.WriteLine("...");
        Console.ReadLine();
        Console.CursorTop = Math.Max(0, Console.CursorTop - 1);
    }

    private static void ShowOptions()
    {
        Console.WriteLine($"Choose one of the following options (1-{options.Count})");
        Console.WriteLine("--------------------------------------------------------");

        for (int i = 0; i < options.Count; i++)
        {
            var key = options.Keys.ElementAt(i);
            Console.WriteLine($"{i + 1}) {key}");
        }

        string input = Console.ReadLine();
        Console.Clear();

        if (int.TryParse(input, out int result) && result > 0 && result <= options.Count)
        {
            var key = options.Keys.ElementAt(result - 1);
            Console.WriteLine($"You choose {key}");
            Console.WriteLine("--------------------------------------------------------");

            var action = options[key];
            action();
        }
    }
}