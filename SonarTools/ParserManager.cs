using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Evaluation;
using SonarTools.Runner;
using System.Threading.Tasks;
using System.Collections.Concurrent;

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
        public String[] Filepaths { get; set; }

        public SonarRunner Parser(String projectPath) {
            try {
                Project proj = new Project(projectPath);

                ProjectProperty prop = proj.GetProperty("Language");
                if (prop.EvaluatedValue == "C++") {
                    return ParseCpp(proj);
                }
            }
            catch (Exception e){
                Console.WriteLine("Exception Catch: {0}\n", e.Message);
            }

            return null;
        }

        private SonarRunner ParseCpp(Project proj) {
            String projectPath = proj.FullPath;

            VcxprojParser parser = new VcxprojParser(proj);
            String includePaths = parser.IncludeDirectoriesJoined;

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

        private void AddGeneralSetting(SonarRunner runner) {
            runner["SourceEncoding"] = "UTF-8";
            runner["ProjectVersion"] = Version;
        }

        public void Run(){
            var results = new BlockingCollection<SonarRunner>();

            var taskAdd = Task.Factory.StartNew(() => {
                foreach (var file in Filepaths) {
                    var v = Parser(file);
                    if (v != null)
                        results.Add(v);
                }

                results.CompleteAdding();
            });

            var task1 = Task.Factory.StartNew(() => {
                foreach(SonarRunner runner in results.GetConsumingEnumerable()) {
                    runner.Run(SonarRunnerHome);
                }
            });

            var task2 = Task.Factory.StartNew(() => {
                foreach (SonarRunner runner in results.GetConsumingEnumerable()) {
                    runner.Run(SonarRunnerHome);
                }
            });

            Task.WaitAll(taskAdd, task1, task2);
        }
    }
}
