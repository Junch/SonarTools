using System;
using SonarTools.Runner;
using Microsoft.Build.Evaluation;

namespace SonarTools.Parser {
    public class ProjectParser {
        private RunnerSetting setting;

        public ProjectParser(RunnerSetting setting) {
            this.setting = setting;
        }

        public virtual SonarRunner Parse(String projectPath) {
            try {
                Project proj = new Project(projectPath);

                ProjectProperty prop = proj.GetProperty("Language");
                if (prop.EvaluatedValue == "C++") {
                    return ParseCpp(proj);
                }
            } catch (Exception e) {
                Console.WriteLine("Exception Catch: {0}\n", e.Message);
            }

            return null;
        }

        private SonarRunner ParseCpp(Project proj) {
            String projectPath = proj.FullPath;

            VcxprojParser parser = new VcxprojParser(proj);
            String includePaths = parser.IncludeDirectoriesJoined;
            includePaths = includePaths.Replace('\\', '/');

            SonarRunner runner;
            if (setting.CppType == CppPluginType.kCppCommercial) {
                runner = new CommercialCppRunner(projectPath, setting.Branch);
                runner["cfamily.library.directories"] = includePaths;
            } else {
                runner = new CommunityCppRunner(projectPath, setting.Branch);
                //runner["cxx.include_directories"] = includePaths; // For V0.9.0
                runner["cxx.includeDirectories"] = includePaths; // For V0.9.1
            }

            AddGeneralSetting(runner);
            return runner;
        }

        private void AddGeneralSetting(SonarRunner runner) {
            runner["SourceEncoding"] = "UTF-8";
            runner["ProjectVersion"] = setting.Version;
        }
    }
}
