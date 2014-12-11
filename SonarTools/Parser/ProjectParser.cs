using System;
using SonarTools.Runner;
using Microsoft.Build.Evaluation;

namespace SonarTools.Parser {
    public class ProjectParser {
        private readonly RunnerSetting setting;

        public ProjectParser(RunnerSetting setting) {
            this.setting = setting;
        }

        public virtual SonarRunner Parse(String projectPath) {
            SonarRunner runner = null ;

            try {
                Project proj = new Project(projectPath);
                runner = Parse(proj);
            } catch (Exception e) {
                Console.WriteLine("Exception Catch: {0}\n", e.Message);
            }

            if (runner != null) {
                AddGeneralSetting(runner);
            }

            return runner;
        }

        private SonarRunner Parse(Project proj) {
            SonarRunner runner = null;

            ProjectProperty prop = proj.GetProperty("Language");
            if (prop.EvaluatedValue == "C++") {
                VcxprojParser parser = new VcxprojParser(proj);
                runner = ParseCpp(parser);
            } else if (prop.EvaluatedValue == "C#") {
                runner = new CSharpRunner(proj.FullPath, setting.Branch);
            }

            return runner;
        }

        private SonarRunner ParseCpp(VcxprojParser parser) {
            String projectPath = parser.project.FullPath;

            SonarRunner runner;
            if (setting.CppType == CppPluginType.kCppCommercial) {
                runner = new CommercialCppRunner(projectPath, setting.Branch);
                if (String.IsNullOrEmpty(setting.BuildWrapper)) {
                    runner["cfamily.library.directories"] = parser.IncludeDirectoriesJoined;
                } else {
                    runner["cfamily.build-wrapper-output"] = setting.BuildWrapper;
                }
            } else {
                runner = new CommunityCppRunner(projectPath, setting.Branch);
                //runner["cxx.include_directories"] = includePaths; // For V0.9.0
                runner["cxx.includeDirectories"] = parser.IncludeDirectoriesJoined; // For V0.9.1
            }

            return runner;
        }

        private void AddGeneralSetting(SonarRunner runner) {
            runner["SourceEncoding"] = "UTF-8";
            runner["ProjectVersion"] = setting.Version;
        }
    }
}
