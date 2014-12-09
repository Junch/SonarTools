using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SonarTools.Util {
    public class SonarConfig {
        public String Depot { get; private set; }
        public String RunnerHome { get; private set; }
        public int ThreadNumber { get; private set; }
        public List<String> Projects { get; private set; }

        public void Read(String fileName, String branchName) {
            XElement root = XElement.Load(fileName);
            Read(root, branchName);
        }

        public void Read(XElement root, String branchName) {
            var eBranchs = from item in root.Elements("Branch")
                           where (String)item.Attribute("Name") == branchName
                           select item;

            if (eBranchs.Count() == 0) {
                throw new System.ArgumentException(String.Format("Cannot find the branch: {0}", branchName));
            }
            var eBranch = eBranchs.First();

            Depot = (String)eBranch.Attribute("Depot") ?? String.Empty;
            RunnerHome = (String)eBranch.Attribute("RunnerHome") ?? String.Empty;
            var att = eBranch.Attribute("ThreadNumber");
            ThreadNumber =  (att == null) ? 0: (int) att;

            Projects = new List<String>();
            var eProjects = eBranch.Element("Projects");
            foreach (XElement e in eProjects.Elements("Project")) {
                Projects.Add(e.Value);
            }
        }
    }
}
