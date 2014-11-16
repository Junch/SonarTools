using System;
using System.Collections.Generic;
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
            setting.Add(String.Format("-Dsonar.{0}={1}", "sources", DirectoryName));


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

        public void RunSonarCmd() {
            System.Console.WriteLine("SonarCmd: {0}", SonarCmdArguments);

            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            //proc.StartInfo.WorkingDirectory = "...";
            proc.StartInfo.FileName = "sonar-runner.bat";
            proc.StartInfo.Arguments = SonarCmdArguments;
            proc.Start();
            proc.WaitForExit();
        }

        protected virtual void PreRun() { }

        protected virtual void PosRun() { }

        public virtual void Run() {
            PreRun();
            RunSonarCmd();
            PosRun();
        }
    }
}
