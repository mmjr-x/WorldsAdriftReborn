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

        //private static readonly List<string> requiredAssemblyDllNames = requiredAssemblyFullNames
        //    .Select(GetAssemblyDllNameFromFullName)
        //    .ToList();

        private static Dictionary<string, string> requiredAssemblyDllNameLocations = new Dictionary<string, string>();

        private static string GetAssemblyDllNameFromFullName(string fullName )
        {
            return $"{fullName.Split(", ")[0]}.dll";
        }

        private static void LoadReferencedAssembly( Assembly assembly )
        {
            foreach (AssemblyName name in assembly.GetReferencedAssemblies())
            {
                if (!AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName == name.FullName))
                {
                    Console.WriteLine(name.FullName);
                    //LoadReferencedAssembly(Assembly.Load(name));

                    if (requiredAssemblyFullNames.Contains(name.FullName))
                    {
                        Console.WriteLine("LOADING ^");
                        try
                        {
                            LoadReferencedAssembly(Assembly.Load(name));
                        } catch (Exception ex) {
                            Console.WriteLine(ex);
                            throw ex;
                        }
                        //LoadReferencedAssembly(Assembly.Load(name));
                    }
                }
            }
        }

        // try to load the assembly yourself
        private static Assembly? AssemblyResolveHandler( object? sender, ResolveEventArgs args )
        {
            
            //Console.WriteLine("SEE IF THIS IS TRIGGERD?", sender, args);
            ////Console.WriteLine(sender);
            //Console.WriteLine("");
            //Console.WriteLine(args.Name);
            //Console.WriteLine("");

            //if (requiredAssemblyLocations.ContainsValue(args.Name))
            //{
            //    return Assembly.Load(requiredAssemblyLocations[args.Name]);
            //} 

            if (requiredAssemblyFullNames.Contains(args.Name))
            {
                //string assemblyDllName = GetAssemblyDllNameFromFullName(args.Name);
                //if (requiredAssemblyDllNameLocations.ContainsKey(assemblyDllName))
                if (requiredAssemblyDllNameLocations.ContainsKey(args.Name))
                {
                    // TODO: Replace loadFile with a copy of the assembly to the gameServer folder and a Assembly.Load() instead
                    //return Assembly.LoadFile(requiredAssemblyDllNameLocations[assemblyDllName]);
                    return Assembly.LoadFile(requiredAssemblyDllNameLocations[args.Name]);
                }

                //Console.WriteLine("HIIIIIIIIIIIIIT!!!");
                Console.WriteLine($"Failed to resolve required game assembly (\"{args.Name}\")");

                Application.Init();
                Window window = new Window("");

                //Gtk.FileChooserDialog fileChooser = new Gtk.FileChooserDialog("Test", window, FileChooserAction.Open, "Cancel", ResponseType.Cancel, "Open", ResponseType.Accept);

                //fileChooser.Run();

                //fileChooser.Destroy();

                FileChooserNative fileChooser = new FileChooserNative(
                    "Select the required game assemblies (can be found in \"<game root>\\UnityClient@Windows_Data\\Managed\")",
                    window,
                    FileChooserAction.Open,
                    "Accept",
                    "Cancel"
                );
                fileChooser.SelectMultiple = true;
                //fileChooser2.
                FileFilter fileFilter = new FileFilter();

                //foreach(string requiredAssemblyDllName in requiredAssemblyDllNames)
                //foreach (string requiredAssemblyDllName in requiredAssemblyFullNames.Select(GetAssemblyDllNameFromFullName))
                foreach (string requiredAssemblyFullName in requiredAssemblyFullNames)
                {
                    //if (!requiredAssemblyDllNameLocations.ContainsKey(requiredAssemblyDllName))
                    if (!requiredAssemblyDllNameLocations.ContainsKey(requiredAssemblyFullName))
                    {
                        //fileFilter.AddPattern(requiredAssemblyDllName);
                        fileFilter.AddPattern(GetAssemblyDllNameFromFullName(requiredAssemblyFullName));
                    }
                    
                }

                //fileFilter.AddPattern("Improbable.WorkerSdkCsharp.dll");
                //fileFilter.AddPattern("Improbable.WorkerSdkCsharp.Framework.dll");
                //fileFilter.AddPattern("Generated.Code.dll");
                fileChooser.AddFilter(fileFilter);
                fileChooser.Run();

                //Console.WriteLine(fileChooser2.Filenames);
                Array.ForEach(fileChooser.Filenames, Console.WriteLine);

                foreach (string assemblyPath in fileChooser.Filenames)
                {
                    string? fileName = Path.GetFileName(assemblyPath);

                    string? matchingFullname = requiredAssemblyFullNames.First(fullName => GetAssemblyDllNameFromFullName(fullName) == fileName);

                    //if (!requiredAssemblyDllNameLocations.ContainsKey(fileName))
                    //{
                    //    requiredAssemblyDllNameLocations[fileName] = assemblyPath;
                    //}

                    if (matchingFullname != null && !requiredAssemblyDllNameLocations.ContainsKey(matchingFullname))
                    {
                        requiredAssemblyDllNameLocations[matchingFullname] = assemblyPath;
                    }

                }


                fileChooser.Destroy();

                window.Destroy();

                Console.WriteLine(requiredAssemblyDllNameLocations[args.Name]);
                //Console.WriteLine(requiredAssemblyDllNameLocations[assemblyDllName]);
                //return Assembly.Load(requiredAssemblyDllNameLocations[assemblyDllName]);

                //try
                //{
                // TODO: Replace loadFile with a copy of the assembly to the gameServer folder and a Assembly.Load() instead
                if(requiredAssemblyDllNameLocations[args.Name] != null)
                {
                    return Assembly.LoadFile(requiredAssemblyDllNameLocations[args.Name]);
                }
                
                //return Assembly.LoadFile(requiredAssemblyDllNameLocations[assemblyDllName]);
                //} catch (Exception ex) {
                //    Console.WriteLine("WE ARE CATCHING!");
                //    Console.WriteLine(ex.Message);
                //}

                //MessageBox.Show("Test", "Test2", MessageBoxButtons.OK);

                //OpenFileDialog openFileDialog1 = new OpenFileDialog();
                //openFileDialog1.ShowDialog();




                ////var fileContent = string.Empty;
                //var filePath = string.Empty;

                //using (OpenFileDialog openFileDialog = new OpenFileDialog())
                //{
                //    openFileDialog.InitialDirectory = "c:\\";
                //    openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                //    openFileDialog.FilterIndex = 2;
                //    openFileDialog.RestoreDirectory = true;

                //    if (openFileDialog.ShowDialog() == DialogResult.OK)
                //    {
                //        //Get the path of specified file
                //        filePath = openFileDialog.FileName;

                //        //Read the contents of the file into a stream
                //        //var fileStream = openFileDialog.OpenFile();

                //        //using (StreamReader reader = new StreamReader(fileStream))
                //        //{
                //        //    fileContent = reader.ReadToEnd();
                //        //}
                //    }
                //}

                //MessageBox.Show("Test", "File Content at path: " + filePath, MessageBoxButtons.OK);

            }

            // If we get to here this means that we have failed
            Console.WriteLine("WE SHOULD NOT BE GETTING HERE!!!");
            //return Assembly.Load("");
            //return Assembly.LoadFile("");
            return null;
            //return "";
            //return Assembly.Load(args.Name);
        }

        public static void LoadDependencies()
        {
            //var test = FilePicker.PickAsync();
            //test.Wait();

            //Application.EnableVisualStyles();
            //MessageBox.Show("Test", "Test2", MessageBoxButtons.OK);

            //string fileName;
            //OpenFileDialog fd = new OpenFileDialog();
            //fd.ShowDialog();
            //fileName = fd.FileName;
            //Console.Write(fileName);

            //Application.Init();
            //Gtk.Window window = new Window("Test 1");
            //Gtk.FileChooserDialog fileChooser = new Gtk.FileChooserDialog("Test", window, FileChooserAction.Open, "Cancel", ResponseType.Cancel, "Open", ResponseType.Accept);

            //fileChooser.Run();

            //fileChooser.Destroy();
            //window.Destroy();

            //fileChooser.Run();

            //OpenFileDialog openFileDialog1 = new OpenFileDialog();
            //if (openFileDialog1 == null)
            //{
            //    Console.WriteLine("openFileDialog1 WAS NULL!!!");
            //}
            //openFileDialog1.ShowDialog();

            // register on assembly resolve exception
            //AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolveHandler;
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(AssemblyResolveHandler);

            // Load everything
            // TODO: Make this specifically try and load the game assemblies that might be missing
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                //Console.WriteLine(assembly.FullName);
                LoadReferencedAssembly(assembly);
            }
        }

    }
}
