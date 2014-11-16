using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarTools.Runner {
    public class CommunityCppRunner: SonarRunner {
        public CommunityCppRunner(String fullFilePath, String branch): base(fullFilePath, branch) {
            this["ProjectDescription"] = "\"Last run by community version\"";
            this["Language"] = "c++";
            this["cxx.cppcheck.reportPath"] = ProjectKey + ".xml";
        }

        public String CppCheckCmdArguments {
            get {
                return String.Format("-j 8 {0} --xml 2>{1}.xml", DirectoryName, ProjectKey);
            }
        }

        public void RunCppCheckCmd() {
            System.Console.WriteLine("Cppcheck: arguments: {0}", CppCheckCmdArguments);

            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            //proc.StartInfo.WorkingDirectory = "...";
            proc.StartInfo.FileName = "cppcheck.exe";
            proc.StartInfo.Arguments = CppCheckCmdArguments;
            proc.Start();
            proc.WaitForExit();
        }

        protected override void PreRun() {
            RunCppCheckCmd();
        }
    }
}
