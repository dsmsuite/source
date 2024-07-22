using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using DsmSuite.Common.Util;

namespace DsmSuite.Analyzer.DotNet.Settings
{
    /// <summary>
    /// InputSettings determine which assemblies are analyzed.<para/>
    /// Either a single directory can be specified
    /// in <c>AssemblyDirectory</c>, or multiple directories in <c>AssemblyDirectories</c>. Setting both
    /// <c>AssemblyDirectory</c> and <c>AssemblyDirectories</c> leads to undefined behaviour.<para/>
    /// If <c>IncludeAssemblyNames</c> is non-empty, an assembly is only analyzed if its <b>basename</b> contains
    /// a match for a regex in <c>IncludeAssemblyNames</c>; otherwise all assemblies are analyzed.
    /// Note that regex matching is case-sensitive.
    /// </summary>
    [Serializable]
    public class InputSettings
    {
        public string AssemblyDirectory { get; set; }

        public List<string> AssemblyDirectories { get; set; }

        public List<string> IncludeAssemblyNames { get; set; }
    }

    /// <summary>
    /// TransformationSettings determine which symbols are included in the output model.<para/>
    /// If <c>IncludedNames</c> is non-empty, symbols are only included if they contain a match for
    /// a regex in this list. Otherwise all symbols are included.<para/>
    /// If <c>IgnoredNames</c> is non-empty, symbols are not included if they contain a match for
    /// a regex in this list. Otherwise all symbols are included.
    /// IgnoredNames is evaluated after IncludedNames, so a symbol that matches both is ignored.<para/>
    /// Note that regex matching is case-sensitive.
    /// </summary>
    /// todo Why both IgnoredNames and IncludedNames and why is one handled by DsiModel and the other by BinaryFile
    [Serializable]
    public class TransformationSettings
    {
        public List<string> IgnoredNames { get; set; }

        public List<string> IncludedNames { get; set; }
    }

    [Serializable]
    public class OutputSettings
    {
        public string Filename { get; set; }
        public bool Compress { get; set; }
    }

    /// <summary>
    /// Settings used during code analysis. Persisted in XML format using serialization.
    /// </summary>
    [Serializable]
    public class AnalyzerSettings
    {
        public LogLevel LogLevel { get; set; }
        public InputSettings Input { get; set; }
        public TransformationSettings Transformation { get; set; }
        public OutputSettings Output { get; set; }

        /// <summary>
        /// A convenience method that returns the configured assembly directory/ies.
        /// </summary>
        public IEnumerable<string> AssemblyDirectories()
        {
            if (Input.AssemblyDirectories?.Count > 0)
                return Input.AssemblyDirectories;
            else
                return Enumerable.Repeat(Input.AssemblyDirectory, 1);
        }

        public static AnalyzerSettings CreateDefault()
        {
            AnalyzerSettings analyzerSettings = new AnalyzerSettings
            {
                LogLevel = LogLevel.Error,
                Input = new InputSettings(),
                Transformation = new TransformationSettings(),
                Output = new OutputSettings(),
            };

            analyzerSettings.Input.AssemblyDirectory = "";

            analyzerSettings.Transformation.IgnoredNames = new List<string>
            {
                // Ignore Microsoft stuff
                "^System.",
                "^Microsoft.",
                // Ignore COM/C++ Interop stuff
                "^Interop",
                // Ignore anonymous classes
                "<",
                "^_"
            };

            analyzerSettings.Output.Filename = "Output.dsi";
            analyzerSettings.Output.Compress = true;
            return analyzerSettings;
        }

        public static void WriteToFile(string filename, AnalyzerSettings analyzerSettings)
        {
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings() { Indent = true };
            XmlSerializer serializer = new XmlSerializer(typeof(AnalyzerSettings));

            using (XmlWriter xmlWriter = XmlWriter.Create(filename, xmlWriterSettings))
            {
                serializer.Serialize(xmlWriter, analyzerSettings);
            }
        }

        public static AnalyzerSettings ReadFromFile(string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(AnalyzerSettings));

            AnalyzerSettings analyzerSettings;
            using (XmlReader reader = XmlReader.Create(filename))
            {
                analyzerSettings = (AnalyzerSettings)serializer.Deserialize(reader);
            }

            analyzerSettings.ResolvePaths(Path.GetDirectoryName(filename));
            return analyzerSettings;
        }

        private void ResolvePaths(string settingFilePath)
        {
            Input.AssemblyDirectory = FilePath.ResolveFile(settingFilePath, Input.AssemblyDirectory);
            for (int i = 0; i < (Input.AssemblyDirectories?.Count ?? 0); i++)
                Input.AssemblyDirectories[i] = FilePath.ResolveFile(settingFilePath, Input.AssemblyDirectories[i]);
            Output.Filename = FilePath.ResolveFile(settingFilePath, Output.Filename);
        }
    }
}
