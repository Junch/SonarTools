using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SonarTools
{
    public class RunnerConfig {
        public String ProjectKey { get; set; }
        public String ProjectName { get; set; }
        public String ProjectDescription { get; set; }
        public String ProjectVersion { get; set; }
        public String Language { get; set; }
        public String SourceEncoding { get; set; }

        public String Include { get; set; }
        public String Sources { get; set; }

        private Dictionary<String, String> additionProperties = new Dictionary<String, String>();
        
        public String this [String prop]{
            set {
                additionProperties[prop] = value;
            }

            get {
                return additionProperties[prop];
            }
        }

        public List<String> GetSettings () {
            var type = typeof(RunnerConfig);
            PropertyInfo[] pi= type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);

            List<String> setting = new List<string>();

            foreach (var prop in pi) {
                if (prop.GetIndexParameters().Length > 0)
                    continue;

                String value = prop.GetValue(this) as String;
                if (String.IsNullOrEmpty(value))
                    continue;

                String propName = FirstLetterLowercase(prop.Name);

                setting.Add(String.Format("-Dsonar.{0}={1}", propName, value));
            }

            foreach (var pair in additionProperties) {
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
