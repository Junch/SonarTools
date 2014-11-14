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

        public String DepotName(String filePath) {
            String s = filePath.Replace('\\', '/');
            String r = Regex.Replace(s, @"\w:", Branch);
            return r;
        }

        public String KeyName(String filePath) {
            String s = Regex.Replace(filePath, @"\w:", Branch);
            String r = s.Replace('\\', '_').Replace('/', '_').Replace('.','_');
            return r.Substring(2); // Escape the start string $_
        }

        public RunnerConfig Parser(String projFilepath) {
            Project proj = new Project(projFilepath);
            String keyName = KeyName(proj.FullPath);
            String dirName = System.IO.Path.GetDirectoryName(proj.FullPath);
            String cppcheckReportpath = keyName + ".xml";

            RunnerConfig cfg = new RunnerConfig() { ProjectName = proj.FullPath};
            cfg["SourceEncoding"] = "UTF-8";
            cfg["ProjectVersion"] = Version;
            cfg["ProjectName"] = DepotName(proj.FullPath);
            cfg["ProjectKey"] = keyName;
            cfg["Sources"] = dirName;

            ProjectProperty prop = proj.GetProperty("Language");
            if (prop.EvaluatedValue == "C++") {
                cfg.type = cppPlugType;
                VcxprojParser parser = new VcxprojParser(proj);
                IEnumerable<String> includes = parser.IncludeDirectories;

                if (cppPlugType == PluginType.kCppCommercial) {
                    cfg["ProjectDescription"] = "Last run by commercial version";
                    cfg["Language"] = "cpp";
                    cfg["cfamily.library.directories"] = String.Join(";", includes);
                } else {
                    cfg["ProjectDescription"] = "Last run by community version";
                    cfg["Language"] = "c++";
                    cfg["cxx.include_directories"] = String.Join(";", includes);
                    cfg["cxx.cppcheck.reportPath"] = cppcheckReportpath;
                }
            }

            return cfg;
        }

        public void Run(RunnerConfig config){

        }
    }
}
