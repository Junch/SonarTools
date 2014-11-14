using Microsoft.Build.Evaluation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace SonarTools {
    public class VcxprojParser {

        public IEnumerable<string> GetIncludeDirectories() {
            throw new NotImplementedException();
        }

        public void GetAdditionalIncludeDirectories(XElement xmlTree, IList<String> dirs) {
            XNamespace ns = xmlTree.Name.Namespace;
            var query = xmlTree.Elements(ns + "ItemDefinitionGroup").
                    Elements(ns + "ClCompile").
                    Elements(ns + "AdditionalIncludeDirectories");

            foreach (XElement elem in query) {
                String[] paths = elem.Value.Split(';');
                foreach (String path in paths)
                    dirs.Add(path);
            }
        }

        public void GetAdditionalIncludeDirectories(Project project, IList<String> dirs) {
            XElement xmlTree = XElement.Parse(project.Xml.RawXml);
            GetAdditionalIncludeDirectories(xmlTree, dirs);

            foreach (var e in project.Imports) {
                xmlTree = XElement.Load(e.ImportedProject.FullPath);
                GetAdditionalIncludeDirectories(xmlTree, dirs);
            }
        }

        public List<String> EvaluateDirectories(Project project, IList<String> dirs) {
            List<String> paths = new List<String>();

            foreach (var x in dirs.Distinct()) {
                Regex regex = new Regex(@"\$\(.*?\)");
                if (!regex.IsMatch(x)) {
                    paths.Add(x);
                } else {
                    Match match = regex.Match(x);
                    String xx = x.Clone() as String;

                    while(match.Success) {
                        String val = match.Value;
                        String env = val.Substring(2, val.Length - 3);
                        ProjectProperty prop = project.GetProperty(env);
                        if (prop != null) {
                            String pattern = String.Format(@"\$\({0}\)", env);
                            xx = Regex.Replace(xx, pattern, prop.EvaluatedValue);
                        }

                        match = match.NextMatch();
                    }

                    foreach (String path in xx.Split(';'))
                        paths.Add(path);
                }
            }

            return paths;
        }

    }
}
