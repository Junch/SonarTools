using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarTools.Runner {
    public class CommercialCppRunner: SonarRunner {
        public CommercialCppRunner(String filePath, String branch): base(filePath, branch) {
            this["ProjectDescription"] = "\"Last run by commercial version\"";
            this["Language"] = "cpp";
            this["ProjectBaseDir"] = DirectoryName;
        }
    }
}
