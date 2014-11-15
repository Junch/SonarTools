using Microsoft.Build.Evaluation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

        public RunnerConfig Parser(String projectPath) {
            Project proj = new Project(projectPath);
            //String cppcheckReportpath = keyName + ".xml";

            RunnerConfig cfg = new RunnerConfig();
            cfg.Branch = Branch;
            cfg["SourceEncoding"] = "UTF-8";
            cfg["ProjectVersion"] = Version;

            ProjectProperty prop = proj.GetProperty("Language");
            if (prop.EvaluatedValue == "C++") {
                cfg.type = cppPlugType;
                VcxprojParser parser = new VcxprojParser(proj);
                IEnumerable<String> includes = parser.IncludeDirectories;
                String includePaths = String.Join(";", includes);

                if (cppPlugType == PluginType.kCppCommercial) {
                    cfg["ProjectDescription"] = "Last run by commercial version";
                    cfg["Language"] = "cpp";
                    cfg["cfamily.library.directories"] = includePaths;
                } else {
                    cfg["ProjectDescription"] = "Last run by community version";
                    cfg["Language"] = "c++";
                    cfg["cxx.include_directories"] = includePaths; // For V0.9.0
                    cfg["cxx.includeDirectories"] = includePaths; // For V0.9.1
                    //cfg["cxx.cppcheck.reportPath"] = cppcheckReportpath;
                }
            }

            return cfg;
        }

        public void Run(RunnerConfig config){
        }
    }
}
