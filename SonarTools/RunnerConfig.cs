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
        public String ProjectVersion { get; set; }
        public String Language { get; set; }
        public String SourceEncoding { get; set; }

        public String Include { get; set; }
        public String Sources { get; set; }

        public List<String> GetSettings () {
            var type = typeof(RunnerConfig);
            PropertyInfo[] pi= type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);

            List<String> setting = new List<string>();

            foreach (var prop in pi) {
                String value = prop.GetValue(this) as String;
                if (String.IsNullOrEmpty(value))
                    continue;

                String propName = FirstLetterLowercase(prop.Name);

                setting.Add(String.Format("-Dsonar.{0}={1}", propName, value));
            }

            return setting;
        }

        private static String FirstLetterLowercase(String propName) {
            return Char.ToLower(propName[0]) + propName.Substring(1);
        }
    }
}
