using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SonarTools.Test {
    [TestClass]
    public class UnitTest1 {
        [TestMethod]
        public void Generate_Setting_From_PropertiesValue() {
            RunnerConfig config = new RunnerConfig() {
                ProjectName = "Simple example",
                SourceEncoding = "UTF-8",
                Language = "c++"};

            var setting = config.GetSettings();

            Assert.AreEqual(3, setting.Count);

            Assert.AreEqual("-Dsonar.projectName=Simple example", setting[0]);
            Assert.AreEqual("-Dsonar.language=c++", setting[1]);
            Assert.AreEqual("-Dsonar.sourceEncoding=UTF-8", setting[2]);
        }
    }
}
