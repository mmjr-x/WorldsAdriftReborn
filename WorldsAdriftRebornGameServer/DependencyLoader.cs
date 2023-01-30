using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Gtk;

namespace WorldsAdriftRebornGameServer
{
    internal static class DependencyLoader
    {

        private static readonly List<string> requiredAssemblyFullNames = new List<string>() {
            "Improbable.WorkerSdkCsharp, Version=10.4.3.0, Culture=neutral, PublicKeyToken=null",
            "Generated.Code, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
            "Improbable.WorkerSdkCsharp.Framework, Version=10.4.3.0, Culture=neutral, PublicKeyToken=null"
        };

        private static Dictionary<string, string> requiredAssemblyDllNameLocations = new Dictionary<string, string>();

        private static string GetAssemblyDllNameFromFullName(string fullName )
        {
            return $"{fullName.Split(", ")[0]}.dll";
        }

        private static void LoadReferencedAssemblyIfGameAssembly( Assembly assembly )
        {
            foreach (AssemblyName name in assembly.GetReferencedAssemblies())
            {
                if (!AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName == name.FullName))
                {
                    //Console.WriteLine(name.FullName);

                    if (requiredAssemblyFullNames.Contains(name.FullName))
                    {
                        //Console.WriteLine("LOADING ^");
                        Console.WriteLine($"Loading game assembly \"{name.FullName}\"");
                        try
                        {
                            LoadReferencedAssemblyIfGameAssembly(
                                Assembly.Load(name)
                            );
                        } catch (Exception ex) {
                            // For some reason we need to try catch here, else the error wont show in the console when not having a debugger attached 
                            Console.WriteLine(ex);
                            throw;
                        }
                    }
                }
            }
        }

        // try to load the assembly yourself
        private static Assembly? AssemblyResolveHandler( object? sender, ResolveEventArgs args )
        {

            if (requiredAssemblyFullNames.Contains(args.Name))
            {

                if (requiredAssemblyDllNameLocations.ContainsKey(args.Name))
                {
                    //Console.WriteLine("IS THIS TRIGGERING?");
                    // TODO: Replace loadFile with a copy of the assembly to the gameServer folder and a Assembly.Load() instead
                    //return Assembly.LoadFile(requiredAssemblyDllNameLocations[assemblyDllName]);
                    return Assembly.LoadFile(requiredAssemblyDllNameLocations[args.Name]);
                }

                Console.WriteLine($"Failed to resolve required game assembly (\"{args.Name}\")");

                Application.Init();
                Window window = new Window("");

                FileChooserNative fileChooser = new FileChooserNative(
                    "Select the required game assemblies (can be found in \"<game root>\\UnityClient@Windows_Data\\Managed\")",
                    window,
                    FileChooserAction.Open,
                    "Accept",
                    "Cancel"
                );
                fileChooser.SelectMultiple = true;
                FileFilter fileFilter = new FileFilter();

                foreach (string requiredAssemblyFullName in requiredAssemblyFullNames)
                {
                    if (!requiredAssemblyDllNameLocations.ContainsKey(requiredAssemblyFullName))
                    {
                        fileFilter.AddPattern(GetAssemblyDllNameFromFullName(requiredAssemblyFullName));
                    }
                    
                }

                fileChooser.AddFilter(fileFilter);
                fileChooser.Run();

                //Array.ForEach(fileChooser.Filenames, Console.WriteLine);

                foreach (string assemblyPath in fileChooser.Filenames)
                {
                    string? fileName = Path.GetFileName(assemblyPath);

                    string? matchingFullname = requiredAssemblyFullNames.First(fullName => GetAssemblyDllNameFromFullName(fullName) == fileName);

                    if (matchingFullname != null && !requiredAssemblyDllNameLocations.ContainsKey(matchingFullname))
                    {
                        requiredAssemblyDllNameLocations[matchingFullname] = assemblyPath;
                    }

                }


                fileChooser.Destroy();

                window.Destroy();

                //Console.WriteLine(requiredAssemblyDllNameLocations[args.Name]);

                // TODO: Replace loadFile with a copy of the assembly to the gameServer folder and a Assembly.Load() instead
                string? location;
                if (requiredAssemblyDllNameLocations.TryGetValue(args.Name, out location))
                {
                    Console.WriteLine($"Loading assembly from specified location (\"{location}\")");
                    return Assembly.LoadFile(location);
                }

            }

            // If we get to here this means that we have failed
            Console.WriteLine($"Specified location for game assembly (\"{args.Name}\") was not provided");

            return null;
        }

        public static void LoadDependencies()
        {

            // register on assembly resolve exception
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(AssemblyResolveHandler);

            // Load game assemblies
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                LoadReferencedAssemblyIfGameAssembly(assembly);
            }
        }

    }
}
