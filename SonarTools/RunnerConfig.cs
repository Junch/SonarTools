using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SonarTools
{
    public class RunnerConfig {
        private Dictionary<String, String> properties = new Dictionary<String, String>();
        public String ProjectName { get; set; }

        public RunnerConfig(String ProjectName) {
            this.ProjectName = ProjectName;
        }

        public PluginType type { get; set; }

        public String CppCheckCmd {
            get {
                String folder = System.IO.Path.GetDirectoryName(ProjectName);
                return String.Format("cppcheck.exe -j 8 {0} --xml 2>{1}.xml", folder, this["ProjectKey"]);
            } 
        }

        public String SonarCmd {
            get {
                String setting = String.Join(" ", GetProperties());
                return "sonar-runner " + setting;
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
            var type = typeof(RunnerConfig);
            PropertyInfo[] pi= type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);

            List<String> setting = new List<string>();

            foreach (var pair in properties) {
                if (String.IsNullOrEmpty(pair.Value))
                    continue;

                String propName = FirstLetterLowercase(pair.Key);
                setting.Add(String.Format("-Dsonar.{0}={1}", propName, pair.Value));
            }

            return setting;
        }

        private static String FirstLetterLowercase(String propName) {
            return Char.ToLower(propName[0]) + propName.Substring(1);
        }
    }
}
