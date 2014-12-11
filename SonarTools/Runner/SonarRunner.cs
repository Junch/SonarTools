using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SonarTools.Runner
{
    public class SonarRunner {
        private Dictionary<String, String> properties = new Dictionary<String, String>();
        public readonly String FullFilePath;
        public readonly String Branch;
        private StreamWriter logWriter;

        public SonarRunner(String fullFilePath, String branch) {
            this.FullFilePath = fullFilePath;
            this.Branch = branch;
        }

        public String ProjectKey {
            get {
                String s = Regex.Replace(FullFilePath, @"\w:", Branch);
                String r = s.Replace('\\', '_').Replace('/', '_').Replace('.', '_');
                return r.Substring(2); // Escape the start string $_
            }
        }

        public String DepotName {
            get {
                String s = FullFilePath.Replace('\\', '/');
                String r = Regex.Replace(s, @"\w:", Branch);
                return r;
            }
        }

        protected string DirectoryName {
            get {
                return System.IO.Path.GetDirectoryName(FullFilePath);
            }
        }

        public String this [String prop]{
            set {
                properties[prop] = value;
            }

            get {
                return properties[prop];
            }
        }

        public String LogFilepath {
            get {
                return String.Format("{0}\\{1}.log", DirectoryName, ProjectKey);
            }
        }

        public String SymbolLinkLogFolder {
            get {
                return "log";
            }
        }

        public List<String> GetProperties () {
            List<String> setting = new List<string>();

            foreach (var pair in properties) {
                if (String.IsNullOrEmpty(pair.Value)) { 
                    continue;
                }

                String propName = FirstLetterLowercase(pair.Key);
                setting.Add(String.Format("-Dsonar.{0}={1}", propName, pair.Value));
            }

            setting.Add(String.Format("-Dsonar.{0}={1}", "projectKey", ProjectKey));
            setting.Add(String.Format("-Dsonar.{0}={1}", "projectName", DepotName));
            setting.Add(String.Format("-Dsonar.{0}={1}", "sources", "."));

            return setting;
        }

        private static String FirstLetterLowercase(String propName) {
            return Char.ToLower(propName[0]) + propName.Substring(1);
        }

        public String SonarCmdArguments {
            get {
                return String.Join(" ", GetProperties());
            }
        }

        public bool RunSonarCmd(String runnerHome) {
            String arguments = String.Format("/c {0}/bin/sonar-runner.bat {1}", runnerHome, SonarCmdArguments);
            WriteLog(String.Format("Sonar-runner arguments: {0}", SonarCmdArguments));

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "cmd.exe";
            psi.Arguments = arguments;
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.CreateNoWindow = true;
            psi.ErrorDialog = false;
            psi.WorkingDirectory = Environment.CurrentDirectory;

            bool bSuccess = true;

            using (Process proc = Process.Start(psi)) {
                proc.OutputDataReceived += ProcDataReceived;
                proc.ErrorDataReceived += ProcDataReceived;
                proc.EnableRaisingEvents = true;

                proc.BeginErrorReadLine();
                proc.BeginOutputReadLine();
                proc.WaitForExit();

                if (proc.ExitCode != 0) {
                    bSuccess = false;
                }
            }

            return bSuccess;
        }

        void ProcDataReceived(object sender, DataReceivedEventArgs e) {
            if (e.Data != null) { 
                WriteLog(e.Data);
            }
        }

        protected void WriteLog(String log) {
            if (logWriter != null) { 
                logWriter.WriteLine(log);
                logWriter.Flush();
            }
        }

        protected virtual void PreRun() {
            // To be overridden by Subclass
        }

        protected virtual void PosRun() {
            // To be overridden by Subclass
        }

        public virtual void Run(String runnerHome) {
            using (logWriter = File.CreateText(LogFilepath)) {
                AddSymbolLink(LogFilepath);

                System.Console.WriteLine("==> {0}", DepotName);
                PreRun();
                bool bSuccess = RunSonarCmd(runnerHome);
                PosRun();
                if (bSuccess) { 
                    System.Console.WriteLine("<== {0}", DepotName);
                } else {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    System.Console.WriteLine("x== {0}", DepotName);
                    Console.ResetColor();
                }
            }
        }
        
        [DllImport("kernel32.dll")]
        static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags);
        enum SymbolicLink {
            File = 0,
            Directory = 1
        }

        protected bool AddSymbolLink(String fullfilePath) {
            if (!Directory.Exists(SymbolLinkLogFolder)) { 
                Directory.CreateDirectory(SymbolLinkLogFolder);
            }

            var filename = Path.GetFileName(fullfilePath);

            var logLink = String.Format("{0}/{1}", SymbolLinkLogFolder, filename);
            return CreateSymbolicLink(logLink, fullfilePath, SymbolicLink.File);
        }
    }
}
