using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using Microsoft.Build.Evaluation;

namespace SonarTools.Parser {
    public class VcxprojParser {

        public Project project { get; set; }

        public VcxprojParser() {
        }

        public VcxprojParser(Project project) {
            this.project = project;
        }

        public virtual IEnumerable<string> IncludeDirectories {
            get {
                List<String> unevaluatedPaths = GetAdditionalIncludeDirectories();
                var evaluatedPaths = EvaluateDirectories(unevaluatedPaths);

                AddIncludeDirectoriesFromEnv(evaluatedPaths);
                return evaluatedPaths.Distinct();
            }
        }

        public String IncludeDirectoriesJoined {
            get {
                IEnumerable<String> includes = IncludeDirectories;
                if (includes.Count() == 0) {
                    throw new InvalidOperationException(String.Format("Failed to get the include directories from {0}", project.FullPath));
                }

                var includesWithQuates = from x in includes select String.Format("\"{0}\"", x);
                return String.Join(",", includesWithQuates);
            }
        }

        #region The implemenation details
        public List<String> GetAdditionalIncludeDirectories() {
            List<String> dirs = new List<string>();
            
            XElement xmlTree = XElement.Parse(project.Xml.RawXml);         
            AddAdditionalIncludeDirectories(xmlTree, dirs);

            foreach (var e in project.Imports) {
                xmlTree = XElement.Load(e.ImportedProject.FullPath);
                AddAdditionalIncludeDirectories(xmlTree, dirs);
            }

            return dirs;
        }

        public List<String> EvaluateDirectories(IList<String> dirs) {
            List<String> paths = new List<String>();

            foreach (var x in dirs.Distinct()) {
                String xx = x.Clone() as String;

                Regex regex = new Regex(@"\$\(.*?\)");
                Match match = regex.Match(x);
                while (match.Success) {
                    String val = match.Value;
                    String env = val.Substring(2, val.Length - 3);
                    ProjectProperty prop = project.GetProperty(env);
                    if (prop != null) {
                        String pattern = String.Format(@"\$\({0}\)", env);
                        xx = Regex.Replace(xx, pattern, prop.EvaluatedValue);
                    }

                    match = match.NextMatch();
                }

                foreach (String path in xx.Split(';')) { 
                    String trimmed = path.Trim();
                    if (trimmed != String.Empty)
                        paths.Add(trimmed);
                }
            }

            paths.Remove("%(AdditionalIncludeDirectories)");
            return paths;
        }

        private void AddIncludeDirectoriesFromEnv(IList<String> dirs) {
            ProjectProperty includeEnv = project.GetProperty("Include");
            if (includeEnv != null) {
                String[] paths = includeEnv.EvaluatedValue.Split(';');
                foreach (String path in paths) {
                    String trimmed = path.Trim();
                    if(trimmed != String.Empty)
                        dirs.Add(trimmed);
                }
            }
        }

        public void AddAdditionalIncludeDirectories(XElement xmlTree, IList<String> dirs) {
            XNamespace ns = xmlTree.Name.Namespace;
            var query = xmlTree.Elements(ns + "ItemDefinitionGroup").
                    Elements(ns + "ClCompile").
                    Elements(ns + "AdditionalIncludeDirectories");

            foreach (XElement elem in query) {
                String[] paths = elem.Value.Split(';');
                foreach (String path in paths)
                    dirs.Add(path.Trim());
            }
        }
        #endregion
    }
}
