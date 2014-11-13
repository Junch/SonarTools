using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace SonarTools {
    public class VcxprojParser {

        public IEnumerable<string> GetIncludeDirectories() {
            throw new NotImplementedException();
        }

        public void getAdditionalIncludeDirectories(XElement xmlTree, IList<String> dirs) {
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
    }
}
