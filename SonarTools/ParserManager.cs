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

        public SonarRunner Parser(String projectPath) {
            Project proj = new Project(projectPath);

            ProjectProperty prop = proj.GetProperty("Language");
            if (prop.EvaluatedValue == "C++") {

                VcxprojParser parser = new VcxprojParser(proj);
                IEnumerable<String> includes = parser.IncludeDirectories;
                System.Diagnostics.Debug.Assert(includes.Count() > 0);
                String includePaths = String.Join(";", includes);
                includePaths = String.Format("\"{0}\"", includePaths);
  
                if (cppPlugType == PluginType.kCppCommercial) {
                    var runner = new CommercialCppRunner(projectPath, Branch);
                    runner["SourceEncoding"] = "UTF-8";
                    runner["ProjectVersion"] = Version;
                    runner["cfamily.library.directories"] = includePaths;
                    return runner;
                } else {
                    var runner = new CommunityCppRunner(projectPath, Branch);
                    runner["SourceEncoding"] = "UTF-8";
                    runner["ProjectVersion"] = Version;
                    runner["cxx.include_directories"] = includePaths; // For V0.9.0
                    runner["cxx.includeDirectories"] = includePaths; // For V0.9.1
                    return runner;
                }
            }

            return null;
        }

        public void Run(SonarRunner config){
        }
    }
}
