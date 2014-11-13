using System;
using System.Xml;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

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

        [TestMethod]
        public void Get_AdditionalInclude_Directories_From_XML() {
            // Arrange
            string text =
            @"<?xml version=""1.0"" encoding=""utf-8""?>
            <Project DefaultTargets=""Build"" ToolsVersion=""12.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
              <ItemDefinitionGroup Condition=""'$(Configuration)|$(Platform)'=='Debug|Win32'"">
                <ClCompile>
                  <WarningLevel>Level3</WarningLevel>
                  <AdditionalIncludeDirectories>C:\Test\Include</AdditionalIncludeDirectories>
                </ClCompile>
              </ItemDefinitionGroup>
              <ItemDefinitionGroup Condition=""'$(Configuration)|$(Platform)'=='Release|Win32'"">
                <ClCompile>
                  <RuntimeLibrary>MultiThreaded</RuntimeLibrary>
                  <AdditionalIncludeDirectories>D:\Test\Include</AdditionalIncludeDirectories>
                </ClCompile>
              </ItemDefinitionGroup>
            </Project>";

            // Action
            XElement xmlTree = XElement.Parse(text);
            VcxprojParser parser = new VcxprojParser();
            List<String> dirs = new List<string>();
            parser.getAdditionalIncludeDirectories(xmlTree, dirs);

            // Assert
            Assert.AreEqual(2, dirs.Count);
            Assert.AreEqual(@"C:\Test\Include", dirs[0]);
            Assert.AreEqual(@"D:\Test\Include", dirs[1]);
        }

    }
}
