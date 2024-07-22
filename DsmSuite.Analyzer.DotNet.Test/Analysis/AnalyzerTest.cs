using System.Collections.Generic;
using System.Reflection;
using DsmSuite.Analyzer.DotNet.Settings;
using DsmSuite.Analyzer.DotNet.Test.Util;
using DsmSuite.Analyzer.Model.Core;
using DsmSuite.Analyzer.Model.Interface;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DsmSuite.Analyzer.DotNet.Test.Analysis
{
    [TestClass]
    public class AnalyzerTest
    {
        private void TestAnalyze(AnalyzerSettings analyzerSettings)
        {
            IDsiModel model = new DsiModel("Test", analyzerSettings.Transformation.IgnoredNames, Assembly.GetExecutingAssembly());
            DotNet.Analysis.Analyzer analyzer = new DotNet.Analysis.Analyzer(model, analyzerSettings, null);
            analyzer.Analyze();

            // Main elements
            IDsiElement elementMainClient = model.FindElementByName("DsmSuite.Analyzer.DotNet.Test.Data.MainClient");
            Assert.IsNotNull(elementMainClient);

            IDsiElement elementMainType = model.FindElementByName("DsmSuite.Analyzer.DotNet.Test.Data.MainType");
            Assert.IsNotNull(elementMainType);

            IDsiElement elementMainTypeMyDelegate = model.FindElementByName("DsmSuite.Analyzer.DotNet.Test.Data.MainType/MyDelegate");
            Assert.IsNotNull(elementMainTypeMyDelegate);

            IDsiElement elementMainTypeAnonymous = model.FindElementByName("DsmSuite.Analyzer.DotNet.Test.Data.MainType/<>c");
            Assert.IsNotNull(elementMainTypeAnonymous);

            IDsiElement elementInterfaceA = model.FindElementByName("DsmSuite.Analyzer.DotNet.Test.Data.InterfaceA");
            Assert.IsNotNull(elementInterfaceA);

            IDsiElement elementBaseType = model.FindElementByName("DsmSuite.Analyzer.DotNet.Test.Data.BaseType");
            Assert.IsNotNull(elementBaseType);

            IDsiElement elementNestedType = model.FindElementByName("DsmSuite.Analyzer.DotNet.Test.Data.MainType/NestedType");
            Assert.IsNotNull(elementNestedType);

            // Fields
            IDsiElement elementFieldType = model.FindElementByName("DsmSuite.Analyzer.DotNet.Test.Data.FieldType");
            Assert.IsNotNull(elementFieldType);

            IDsiElement elementGenericFieldType = model.FindElementByName("DsmSuite.Analyzer.DotNet.Test.Data.GenericFieldType`1");
            Assert.IsNotNull(elementGenericFieldType);

            IDsiElement elementGenericFieldTypeParameter = model.FindElementByName("DsmSuite.Analyzer.DotNet.Test.Data.GenericFieldTypeParameter");
            Assert.IsNotNull(elementGenericFieldTypeParameter);

            // Properties
            IDsiElement elementPropertyType = model.FindElementByName("DsmSuite.Analyzer.DotNet.Test.Data.PropertyType");
            Assert.IsNotNull(elementPropertyType);

            IDsiElement elementPropertyEnum = model.FindElementByName("DsmSuite.Analyzer.DotNet.Test.Data.PropertyEnum");
            Assert.IsNotNull(elementPropertyEnum);

            IDsiElement elementGenericPropertyType = model.FindElementByName("DsmSuite.Analyzer.DotNet.Test.Data.GenericPropertyType`1");
            Assert.IsNotNull(elementGenericPropertyType);

            IDsiElement elementGenericPropertyTypeParameter = model.FindElementByName("DsmSuite.Analyzer.DotNet.Test.Data.GenericPropertyTypeParameter");
            Assert.IsNotNull(elementGenericPropertyTypeParameter);

            // Method variables
            IDsiElement elementMethodVariableType = model.FindElementByName("DsmSuite.Analyzer.DotNet.Test.Data.MethodVariableType");
            Assert.IsNotNull(elementMethodVariableType);

            // Parameters
            IDsiElement elementParameterEnum = model.FindElementByName("DsmSuite.Analyzer.DotNet.Test.Data.ParameterEnum");
            Assert.IsNotNull(elementParameterEnum);

            IDsiElement elementParameterType = model.FindElementByName("DsmSuite.Analyzer.DotNet.Test.Data.ParameterType");
            Assert.IsNotNull(elementParameterType);

            IDsiElement elementGenericParameterType = model.FindElementByName("DsmSuite.Analyzer.DotNet.Test.Data.GenericParameterType`1");
            Assert.IsNotNull(elementGenericParameterType);

            IDsiElement elementGenericParameterTypeParameter = model.FindElementByName("DsmSuite.Analyzer.DotNet.Test.Data.GenericParameterTypeParameter");
            Assert.IsNotNull(elementGenericParameterTypeParameter);

            // Return types
            IDsiElement elementReturnType = model.FindElementByName("DsmSuite.Analyzer.DotNet.Test.Data.ReturnType");
            Assert.IsNotNull(elementReturnType);

            IDsiElement elementReturnEnum = model.FindElementByName("DsmSuite.Analyzer.DotNet.Test.Data.ReturnEnum");
            Assert.IsNotNull(elementReturnEnum);

            IDsiElement elementGenericReturnType = model.FindElementByName("DsmSuite.Analyzer.DotNet.Test.Data.GenericReturnType`1");
            Assert.IsNotNull(elementGenericReturnType);

            IDsiElement elementGenericReturnTypeParameter = model.FindElementByName("DsmSuite.Analyzer.DotNet.Test.Data.GenericReturnTypeParameter");
            Assert.IsNotNull(elementGenericReturnTypeParameter);

            IDsiElement elementDelegateGenericParameter = model.FindElementByName("DsmSuite.Analyzer.DotNet.Test.Data.DelegateGenericParameter");
            Assert.IsNotNull(elementDelegateGenericParameter);

            IDsiElement elementEventsArgsGenericParameter = model.FindElementByName("DsmSuite.Analyzer.DotNet.Test.Data.EventsArgsGenericParameter");
            Assert.IsNotNull(elementEventsArgsGenericParameter);

            // Main relations
            Assert.IsTrue(model.DoesRelationExist(elementMainClient.Id, elementMainType.Id));
            Assert.IsTrue(model.DoesRelationExist(elementMainClient.Id, elementEventsArgsGenericParameter.Id));

            Assert.IsTrue(model.DoesRelationExist(elementMainType.Id, elementNestedType.Id));
            Assert.IsTrue(model.DoesRelationExist(elementMainType.Id, elementMainTypeAnonymous.Id));
            Assert.IsTrue(model.DoesRelationExist(elementMainType.Id, elementMainTypeMyDelegate.Id));

            Assert.IsTrue(model.DoesRelationExist(elementMainTypeAnonymous.Id, elementMainTypeMyDelegate.Id));

            Assert.IsTrue(model.DoesRelationExist(elementMainTypeAnonymous.Id, elementDelegateGenericParameter.Id));
            Assert.IsTrue(model.DoesRelationExist(elementMainTypeAnonymous.Id, elementDelegateGenericParameter.Id));

            Assert.IsTrue(model.DoesRelationExist(elementMainType.Id, elementInterfaceA.Id));
            Assert.IsTrue(model.DoesRelationExist(elementMainType.Id, elementBaseType.Id));

            Assert.IsTrue(model.DoesRelationExist(elementMainType.Id, elementEventsArgsGenericParameter.Id));
            Assert.IsTrue(model.DoesRelationExist(elementMainType.Id, elementDelegateGenericParameter.Id));

            // Field relations
            Assert.IsTrue(model.DoesRelationExist(elementMainType.Id, elementFieldType.Id));
            Assert.IsTrue(model.DoesRelationExist(elementMainType.Id, elementGenericFieldType.Id));

            // Property relations
            Assert.IsTrue(model.DoesRelationExist(elementMainType.Id, elementPropertyType.Id));
            Assert.IsTrue(model.DoesRelationExist(elementMainType.Id, elementPropertyEnum.Id));
            Assert.IsTrue(model.DoesRelationExist(elementMainType.Id, elementGenericPropertyType.Id));
            Assert.IsTrue(model.DoesRelationExist(elementMainType.Id, elementGenericPropertyTypeParameter.Id));

            // Method variable relations
            Assert.IsTrue(model.DoesRelationExist(elementMainType.Id, elementMethodVariableType.Id));

            // Parameters type relations
            Assert.IsTrue(model.DoesRelationExist(elementMainType.Id, elementParameterEnum.Id));
            Assert.IsTrue(model.DoesRelationExist(elementMainType.Id, elementParameterType.Id));
            Assert.IsTrue(model.DoesRelationExist(elementMainType.Id, elementGenericParameterType.Id));
            Assert.IsTrue(model.DoesRelationExist(elementMainType.Id, elementGenericParameterTypeParameter.Id));

            Assert.IsTrue(model.DoesRelationExist(elementInterfaceA.Id, elementParameterEnum.Id));
            Assert.IsTrue(model.DoesRelationExist(elementInterfaceA.Id, elementParameterType.Id));
            Assert.IsTrue(model.DoesRelationExist(elementInterfaceA.Id, elementGenericParameterType.Id));
            Assert.IsTrue(model.DoesRelationExist(elementInterfaceA.Id, elementGenericParameterTypeParameter.Id));

            // Return type relations
            Assert.IsTrue(model.DoesRelationExist(elementMainType.Id, elementReturnType.Id));
            Assert.IsTrue(model.DoesRelationExist(elementMainType.Id, elementReturnEnum.Id));
            Assert.IsTrue(model.DoesRelationExist(elementMainType.Id, elementGenericReturnType.Id));
            Assert.IsTrue(model.DoesRelationExist(elementMainType.Id, elementGenericReturnTypeParameter.Id));

            Assert.IsTrue(model.DoesRelationExist(elementInterfaceA.Id, elementReturnType.Id));
            Assert.IsTrue(model.DoesRelationExist(elementInterfaceA.Id, elementReturnEnum.Id));
            Assert.IsTrue(model.DoesRelationExist(elementInterfaceA.Id, elementGenericReturnType.Id));
            Assert.IsTrue(model.DoesRelationExist(elementInterfaceA.Id, elementGenericReturnTypeParameter.Id));
        }

        [TestMethod]
        public void TestAnalyzeDirectory()
        {
            AnalyzerSettings analyzerSettings = AnalyzerSettings.CreateDefault();
            analyzerSettings.Input.AssemblyDirectory = TestData.RootDirectory;
            analyzerSettings.Transformation.IgnoredNames = new List<string>();

            TestAnalyze(analyzerSettings);
        }

        [TestMethod]
        public void TestAnalyzeDirectories()
        {
            AnalyzerSettings analyzerSettings = AnalyzerSettings.CreateDefault();
            analyzerSettings.Input.AssemblyDirectories = new List<string> { TestData.RootDirectory };
            analyzerSettings.Transformation.IgnoredNames = new List<string>();

            TestAnalyze(analyzerSettings);
        }

        [TestMethod]
        public void TestIncludeAssemblyNames()
        {
            IDsiModel model;
            DotNet.Analysis.Analyzer analyzer;

            AnalyzerSettings analyzerSettings = AnalyzerSettings.CreateDefault();
            analyzerSettings.Input.AssemblyDirectories = new List<string> { TestData.RootDirectory };
            analyzerSettings.Transformation.IgnoredNames = new List<string>();

            analyzerSettings.Input.IncludeAssemblyNames = new List<string> { @".*" };
            model = new DsiModel("Test", analyzerSettings.Transformation.IgnoredNames, Assembly.GetExecutingAssembly());
            analyzer = new DotNet.Analysis.Analyzer(model, analyzerSettings, null);
            analyzer.Analyze();
            Assert.IsNotNull(model.FindElementByName("DsmSuite.Analyzer.DotNet.Test.Data.MainClient"));

            analyzerSettings.Input.IncludeAssemblyNames = new List<string> { @"Model", @"Dot[Nn]et\.Test\.(Data)?" };
            model = new DsiModel("Test", analyzerSettings.Transformation.IgnoredNames, Assembly.GetExecutingAssembly());
            analyzer = new DotNet.Analysis.Analyzer(model, analyzerSettings, null);
            analyzer.Analyze();
            Assert.IsNotNull(model.FindElementByName("DsmSuite.Analyzer.DotNet.Test.Data.MainClient"));

            analyzerSettings.Input.IncludeAssemblyNames = new List<string> { @"Model" };
            model = new DsiModel("Test", analyzerSettings.Transformation.IgnoredNames, Assembly.GetExecutingAssembly());
            analyzer = new DotNet.Analysis.Analyzer(model, analyzerSettings, null);
            analyzer.Analyze();
            Assert.IsNull(model.FindElementByName("DsmSuite.Analyzer.DotNet.Test.Data.MainClient"));
        }

        [TestMethod]
        public void TestIncludedNames()
        {
            IDsiModel model;
            DotNet.Analysis.Analyzer analyzer;

            AnalyzerSettings analyzerSettings = AnalyzerSettings.CreateDefault();
            analyzerSettings.Input.AssemblyDirectories = new List<string> { TestData.RootDirectory };
            analyzerSettings.Transformation.IgnoredNames = new List<string>();

            analyzerSettings.Transformation.IncludedNames = new List<string> { @"Model", @"Dot[Nn]et\.Test\.(Data)?.Main" };
            model = new DsiModel("Test", analyzerSettings.Transformation.IgnoredNames, Assembly.GetExecutingAssembly());
            analyzer = new DotNet.Analysis.Analyzer(model, analyzerSettings, null);
            analyzer.Analyze();
            Assert.IsNotNull(model.FindElementByName("DsmSuite.Analyzer.DotNet.Test.Data.MainClient"));
            Assert.IsNull(model.FindElementByName("DsmSuite.Analyzer.DotNet.Test.Data.BaseType"));
        }

        [TestMethod]
        public void TestIgnoredNames()
        {
            IDsiModel model;
            DotNet.Analysis.Analyzer analyzer;

            AnalyzerSettings analyzerSettings = AnalyzerSettings.CreateDefault();
            analyzerSettings.Input.AssemblyDirectories = new List<string> { TestData.RootDirectory };
            analyzerSettings.Transformation.IncludedNames = new List<string>();

            analyzerSettings.Transformation.IgnoredNames = new List<string> { @"Model", @"/" };
            model = new DsiModel("Test", analyzerSettings.Transformation.IgnoredNames, Assembly.GetExecutingAssembly());
            analyzer = new DotNet.Analysis.Analyzer(model, analyzerSettings, null);
            analyzer.Analyze();
            Assert.IsNotNull(model.FindElementByName("DsmSuite.Analyzer.DotNet.Test.Data.MainClient"));
            Assert.IsNotNull(model.FindElementByName("DsmSuite.Analyzer.DotNet.Test.Data.MainType"));
            Assert.IsNull(model.FindElementByName("DsmSuite.Analyzer.DotNet.Test.Data.MainType/NestedType"));
        }
        [TestMethod]
        public void TestIncludedIgnoredNames()
        {
            IDsiModel model;
            DotNet.Analysis.Analyzer analyzer;

            AnalyzerSettings analyzerSettings = AnalyzerSettings.CreateDefault();
            analyzerSettings.Input.AssemblyDirectories = new List<string> { TestData.RootDirectory };
            analyzerSettings.Transformation.IncludedNames = new List<string> { @"Main" };

            analyzerSettings.Transformation.IgnoredNames = new List<string> { @"MainType$" };
            model = new DsiModel("Test", analyzerSettings.Transformation.IgnoredNames, Assembly.GetExecutingAssembly());
            analyzer = new DotNet.Analysis.Analyzer(model, analyzerSettings, null);
            analyzer.Analyze();
            Assert.IsNotNull(model.FindElementByName("DsmSuite.Analyzer.DotNet.Test.Data.MainClient"));
            Assert.IsNull(model.FindElementByName("DsmSuite.Analyzer.DotNet.Test.Data.MainType"));
            Assert.IsNotNull(model.FindElementByName("DsmSuite.Analyzer.DotNet.Test.Data.MainType/NestedType"));
        }
    }
}
