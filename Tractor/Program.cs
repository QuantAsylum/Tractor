using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tractor
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        /// <summary>
        /// Called when the resolution of an assembly fails. Here, we have a chance to load the assembly
        /// before a fatal error occurs. The case we're trying to catch here is the QAAnalyzer assembly
        /// not being found. We want to always rely on the QAAnalyzer that has been installed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // See https://weblog.west-wind.com/posts/2016/Dec/12/Loading-NET-Assemblies-out-of-Seperate-Folders

            // check for assemblies already loaded
            //Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);
            //if (assembly != null)
            //    return assembly;

            if (args.Name.Contains("QAAnalyzer"))
            {
                Assembly ass;
                ass = TryToLoadAssembly(@"C:\Program Files (x86)\QuantAsylum\QA401\QAAnalyzer.exe");

                if (ass != null)
                    return ass;

                ass = TryToLoadAssembly(@"D:\Program Files (x86)\QuantAsylum\QA401\QAAnalyzer.exe");
                if (ass != null)
                    return ass;

                throw new DllNotFoundException("The application QAAnalyzer.EXE was not found in the expected install directories. You need to manually copy the QAAnalyzer.EXE file to the Tractor.EXE directory");
            }

            return null;
        }

        static private Assembly TryToLoadAssembly(string fullPath)
        {
            try
            {
                Assembly a = Assembly.LoadFrom(fullPath);
                return a;
            }
            catch (Exception ex)
            {
                
            }

            return null;
        }
    }
}
