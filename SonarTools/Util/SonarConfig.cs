using System;
using System.Collections.Generic;
using System.IO;
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

            var projects = new List<ProjectSetting>();

            GetDataFromProjects(eBranch.Element("Projects"), setting, projects);
            GetDataFromFolders(eBranch.Element("Folders"), projects);

            setting.Projects = projects.ToArray();
            return setting;
        }

        private void GetDataFromProjects(XElement eProjects, RunnerSetting setting, List<ProjectSetting> projects) {
            if (eProjects != null) {
                var v = from item in eProjects.Elements("Project")
                        where item.Attribute("Skip") == null || !(bool)item.Attribute("Skip")
                        select item;

                foreach (XElement e in v) {
                    var proj = new ProjectSetting() {
                        Filepath = e.Value,
                        BuildWrapper = (String)e.Attribute("BuildWrapper") ?? setting.BuildWrapper,
                        MaxHeapSize = (String)e.Attribute("MaxHeapSize") ?? setting.MaxHeapSize
                    };

                    projects.Add(proj);
                }
            }
        }

        private void GetDataFromFolders(XElement eFolders, List<ProjectSetting> projects) {
            if (eFolders != null) {
                var u = from item in eFolders.Elements("Folder")
                        where item.Attribute("Skip") == null || !(bool)item.Attribute("Skip")
                        select item;

                foreach (XElement e in u) {
                    Traverse(e.Value, projects);
                }
            }
        }

        private bool IsVcxproj(FileInfo fi) {
            return 0 == String.Compare(fi.Extension, ".vcxproj", true);
        }

        private bool IsCsproj(FileInfo fi) {
            return 0 == String.Compare(fi.Extension, ".csproj", true);
        }

        private void Traverse(string sPathName, List<ProjectSetting> arr) {
            Queue<string> pathQueue = new Queue<string>();
            pathQueue.Enqueue(sPathName);
            while (pathQueue.Count > 0) {
                DirectoryInfo diParent = new DirectoryInfo(pathQueue.Dequeue());
                foreach (DirectoryInfo diChild in diParent.GetDirectories()) {
                    pathQueue.Enqueue(diChild.FullName);
                }

                foreach (FileInfo fi in diParent.GetFiles()) {
                    if (IsVcxproj(fi) || IsCsproj(fi)) {
                        ProcessFile(fi, arr);
                    } 
                }
            }

            Console.WriteLine("{0} projecs are added from {1}", arr.Count, sPathName);
        }

        private void ProcessFile(FileInfo fi, List<ProjectSetting> arr) {
            DirectoryInfo dir = fi.Directory;
            String fileName = Path.GetFileName(fi.FullName);

            foreach (DirectoryInfo di in dir.GetDirectories()) {
                ProjectSetting proj = null;

                if (IsVcxproj(fi) && di.Name.StartsWith("sonarbuild_" + fileName, StringComparison.OrdinalIgnoreCase) && !di.Name.EndsWith("_work")) {
                    proj = new ProjectSetting() {
                        Filepath = fi.FullName,
                        BuildWrapper = di.Name
                    };
                } else if (IsCsproj(fi)) {
                    proj = new ProjectSetting() {
                        Filepath = fi.FullName
                    };
                }

                if (proj != null) {
                    arr.Add(proj);
                    break;
                }
            }
        }
    }
}
