using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SonarTools.Util {
    public class SonarConfig {
        public RunnerSetting Read(String fileName, String id) {
            XElement root = XElement.Load(fileName);
            return Read(root, id);
        }

        public RunnerSetting Read(XElement root, String id) {
            var eBranchs = from item in root.Elements("Branch")
                           where (String)item.Attribute("Id") == id
                           select item;

            if (eBranchs.Count() == 0) {
                throw new System.ArgumentException(String.Format("Cannot find the Id: {0}", id));
            } else if (eBranchs.Count() > 1) {
                throw new System.ArgumentException(String.Format("The Id {0} is not unique", id));
            }
            var eBranch = eBranchs.First();

            return GetValues(eBranch);
        }

        private RunnerSetting GetValues(XElement eBranch) {
            RunnerSetting setting = new RunnerSetting();

            setting.Branch = (String)eBranch.Attribute("Depot") ?? String.Empty;
            setting.RunnerHome = (String)eBranch.Attribute("RunnerHome") ?? String.Empty;
            var att = eBranch.Attribute("ThreadNumber");
            if (att != null) {
                setting.ThreadNumber = (int)att;
            }
            setting.BuildWrapper = (String)eBranch.Attribute("BuildWrapper") ?? setting.BuildWrapper;
            setting.MaxHeapSize = (String)eBranch.Attribute("MaxHeapSize") ?? setting.MaxHeapSize;

            var type = (String)eBranch.Attribute("CppType");
            if (type != null && String.Compare(type, "commerical", true) == 0) {
                setting.CppType = CppPluginType.kCppCommercial;
            }

            var v = from item in eBranch.Element("Projects").Elements("Project")
                    where item.Attribute("Skip") == null || (bool)item.Attribute("Skip") == false
                    select item;

            var projects = new List<ProjectSetting>();
            foreach (XElement e in v) {
                var proj = new ProjectSetting() {
                    Filepath = e.Value,
                    BuildWrapper = (String)e.Attribute("BuildWrapper") ?? setting.BuildWrapper,
                    MaxHeapSize = (String)e.Attribute("MaxHeapSize") ?? setting.MaxHeapSize
                };

                projects.Add(proj);
            }
            setting.Projects = projects.ToArray();

            return setting;
        }
    }
}
