using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SonarTools
{
    public class RunnerConfig {
        private Dictionary<String, String> properties = new Dictionary<String, String>();
        public String FullFilePath { get; set; }

        public String Branch { get; set; }

        public PluginType type { get; set; }

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

        public String CppCheckCmdArguments {
            get {
                return String.Format("-j 8 {0} --xml 2>{1}.xml", DirectoryName, this["ProjectKey"]);
            } 
        }

        private string DirectoryName {
            get {
                return System.IO.Path.GetDirectoryName(FullFilePath);
            }
        }

        public String SonarCmdArguments {
            get {
                return String.Join(" ", GetProperties(true));
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

        public List<String> GetProperties (bool expand) {
             var type = typeof(RunnerConfig);
            PropertyInfo[] pi= type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);

            List<String> setting = new List<string>();

            foreach (var pair in properties) {
                if (String.IsNullOrEmpty(pair.Value))
                    continue;

                String propName = FirstLetterLowercase(pair.Key);
                setting.Add(String.Format("-Dsonar.{0}={1}", propName, pair.Value));
            }

            if (expand) {
                setting.Add(String.Format("-Dsonar.{0}={1}", "fullFilePath", FullFilePath));
                setting.Add(String.Format("-Dsonar.{0}={1}", "projectKey", ProjectKey));
                setting.Add(String.Format("-Dsonar.{0}={1}", "ProjectName", DepotName));
                setting.Add(String.Format("-Dsonar.{0}={1}", "Sources", DirectoryName));
            }

            return setting;
        }

        private static String FirstLetterLowercase(String propName) {
            return Char.ToLower(propName[0]) + propName.Substring(1);
        }

        public void Run() {
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            //proc.StartInfo.WorkingDirectory = "...";
            proc.StartInfo.FileName = "sonar-runner.bat";
            proc.StartInfo.Arguments = SonarCmdArguments;
            proc.Start();
            proc.WaitForExit();
        }

    }
}
