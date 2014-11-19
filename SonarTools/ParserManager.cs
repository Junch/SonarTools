using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Evaluation;
using SonarTools.Runner;

namespace SonarTools {

    public enum PluginType {
        kCppCommercial,
        kCppCommunity,
        kCSharp,
        kJava
    };
 
    public class ParserManager {
        public PluginType cppPlugType { get; set; }
        public String Version { get; set; }
        public String Branch { get; set; }
        public String SonarRunnerHome { get; set; }

        public SonarRunner Parser(String projectPath) {
            Project proj = new Project(projectPath);

            ProjectProperty prop = proj.GetProperty("Language");
            if (prop.EvaluatedValue == "C++") {

                VcxprojParser parser = new VcxprojParser(proj);
                IEnumerable<String> includes = parser.IncludeDirectories;
                System.Diagnostics.Debug.Assert(includes.Count() > 0);
                if (includes.Count() == 0) {
                    System.Console.WriteLine("Failed to get the include directories");
                }
                var includesWithQuates = from x in includes select String.Format("\"{0}\"",x);
                String includePaths = String.Join(",", includesWithQuates);

                SonarRunner runner;
                if (cppPlugType == PluginType.kCppCommercial) {
                     runner = new CommercialCppRunner(projectPath, Branch);
                     runner["cfamily.library.directories"] = includePaths;
                } else {
                    runner = new CommunityCppRunner(projectPath, Branch);
                    //runner["cxx.include_directories"] = includePaths; // For V0.9.0
                    runner["cxx.includeDirectories"] = includePaths; // For V0.9.1
                }

                AddGeneralSetting(runner);
                return runner;
            }

            return null;
        }

        private void AddGeneralSetting(SonarRunner runner) {
            runner["SourceEncoding"] = "UTF-8";
            runner["ProjectVersion"] = Version;
        }

        public void Run(SonarRunner config){
        }
    }
}
