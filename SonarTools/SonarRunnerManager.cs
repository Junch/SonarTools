using System;
using System.Collections.Generic;
using SonarTools.Runner;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using SonarTools.Parser;

namespace SonarTools {

    public enum CppPluginType {
        kCppCommercial,
        kCppCommunity,
    };

    public class RunnerSetting {
        public CppPluginType CppType = CppPluginType.kCppCommunity;
        public String Version = "1.0.0.0";
        public int ThreadNumber = 1;
        public String Branch;
        public String RunnerHome;
        public String[] Filepaths;
    }

    public class SonarRunnerManager {
        private RunnerSetting setting;
        private ProjectParser parser; 

        public SonarRunnerManager(RunnerSetting setting, ProjectParser parser = null) {
            this.setting = setting;
            this.parser = parser ?? new ProjectParser(setting);
        }

        public void Run(){
            List<Task> tasks = new List<Task>();
            var coll = new BlockingCollection<SonarRunner>();

            var taskAdd = Task.Factory.StartNew(() => {
                foreach (var file in setting.Filepaths) {
                    var v = parser.Parse(file);
                    if (v != null)
                        coll.Add(v);
                }

                coll.CompleteAdding();
            });
            tasks.Add(taskAdd);
            
            for (int i = 0; i < setting.ThreadNumber; ++i) {
                var task = Task.Factory.StartNew(() => {
                    foreach (SonarRunner runner in coll.GetConsumingEnumerable()) {
                        runner.Run(setting.RunnerHome);
                    }
                });

                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray());
        }
    }
}
