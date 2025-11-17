
using System.Diagnostics;

namespace BenScr.NugetPackageBuilder;
public static class Program
{
    private static Dictionary<string, Action> options = new Dictionary<string, Action>
    {
        {"Set Package Path", SetPackageDirectory },
        {"Build Package", BuildPackage },
        {"Exit", Exit }
    };

    private static DirectoryInfo packageDir;

    public static void Main(string[] args)
    {
        while (true)
        {
            Console.Clear();
            ShowOptions();
        }
    }

    private static void Exit()
    {
        Environment.Exit(0);
    }

    private static void BuildPackage()
    {
        if(packageDir == null)
        {
            Console.WriteLine("Package directory is null or empty, build not possible!");
            Console.WriteLine("Would you like to set the directory? (Y/N)");

            if (EnteredYes())
            {
                SetPackageDirectory();
            }

            return;
        }

        try
        {
            Process.Start("cmd.exe", $"/c dotnet pack {packageDir} -c Release");
            Console.WriteLine($"Start building package {packageDir.Name}");
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

    private static void SetPackageDirectory()
    {
        Console.WriteLine("Enter the path of the project directory:");

        string input = Console.ReadLine();
        while (!Directory.Exists(input))
        {
            Console.WriteLine("Invalid directory Path!");
            Console.Write("Re-Enter the directory path: ");
            input = Console.ReadLine();
        }

        packageDir = new DirectoryInfo(input);
        Console.WriteLine($"Succesfully set package directory to: {packageDir.Name}");
        PressEnterToContinue();
    }

    private static void PressEnterToContinue()
    {
        Console.WriteLine("...");
        Console.ReadLine();
        Console.CursorTop = Math.Max(0, Console.CursorTop-1);
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