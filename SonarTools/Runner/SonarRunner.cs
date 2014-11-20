﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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

        public List<String> GetProperties () {
            var type = typeof(SonarRunner);
            PropertyInfo[] pi= type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);

            List<String> setting = new List<string>();

            foreach (var pair in properties) {
                if (String.IsNullOrEmpty(pair.Value))
                    continue;

                String propName = FirstLetterLowercase(pair.Key);
                setting.Add(String.Format("-Dsonar.{0}={1}", propName, pair.Value));
            }

            setting.Add(String.Format("-Dsonar.{0}={1}", "fullFilePath", FullFilePath));
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

        public void RunSonarCmd(String runnerHome) {
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

            using (Process proc = Process.Start(psi)) {
                proc.OutputDataReceived += proc_DataReceived;
                proc.ErrorDataReceived += proc_DataReceived;
                proc.EnableRaisingEvents = true;

                proc.Start();
                proc.BeginErrorReadLine();
                proc.BeginOutputReadLine();
                proc.WaitForExit();
            }
        }

        void proc_DataReceived(object sender, DataReceivedEventArgs e) {
            if (e.Data != null)
                WriteLog(e.Data);
        }

        protected void WriteLog(String log) {
            if (logWriter != null)
                logWriter.WriteLine(log);
        }

        protected virtual void PreRun() { }

        protected virtual void PosRun() { }

        public virtual async Task Run(String runnerHome) {
            await Task.Run(() => {
                using (logWriter = File.CreateText(LogFilepath)) {
                    System.Console.WriteLine("==> {0}", DepotName);
                    PreRun();
                    RunSonarCmd(runnerHome);
                    PosRun();
                    System.Console.WriteLine("<== {0}", DepotName);
                }
            });
        }
    }
}
