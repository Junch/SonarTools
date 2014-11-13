using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SonarTools.Test {
    [TestClass]
    public class UnitTest1 {
        [TestMethod]
        public void Generate_Setting_From_Properties() {
            RunnerConfig config = new RunnerConfig {
                ProjectName = "Simple example",
                SourceEncoding = "UTF-8",
                Language = "c++"};
            var setting = config.GetSettings();
            setting.Sort();

            Assert.AreEqual(3, setting.Count);

            Assert.AreEqual("-Dsonar.language=c++", setting[0]);
            Assert.AreEqual("-Dsonar.projectName=Simple example", setting[1]);
            Assert.AreEqual("-Dsonar.sourceEncoding=UTF-8", setting[2]);
        }

        [TestMethod]
        public void Generate_Setting_From_AdditionalProperties() {
            RunnerConfig config = new RunnerConfig {
                SourceEncoding = "UTF-8"
            };
            config["cxx.cppcheck.reportPath"] = "cppcheck_coredll.xml";

            var setting = config.GetSettings();
            setting.Sort();

            Assert.AreEqual(2, setting.Count);
            Assert.AreEqual("-Dsonar.cxx.cppcheck.reportPath=cppcheck_coredll.xml", setting[0]);
            Assert.AreEqual("-Dsonar.sourceEncoding=UTF-8", setting[1]);
        }
    }
}
