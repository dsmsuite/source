using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DsmSuite.Analyzer.DotNet.Lib;
using DsmSuite.Analyzer.DotNet.Settings;
using DsmSuite.Analyzer.Model.Interface;
using DsmSuite.Common.Util;
using Mono.Cecil;

namespace DsmSuite.Analyzer.DotNet.Analysis
{
    public class Analyzer
    {
        private readonly IDsiModel _model;
        private readonly AnalyzerSettings _analyzerSettings;
        private readonly IProgress<ProgressInfo> _progress;
        private readonly List<BinaryFile> _assemblyFiles = new List<BinaryFile>();
        private readonly DotNetResolver _resolver = new DotNetResolver();

        public Analyzer(IDsiModel model, AnalyzerSettings analyzerSettings, IProgress<ProgressInfo> progress)
        {
            _model = model;
            _analyzerSettings = analyzerSettings;
            _progress = progress;
        }

        public void Analyze()
        {
            FindAssemblies();
            FindTypes();
            FindRelations();
        }

        private void FindAssemblies()
        {
            //For every configured assembly directory lookup the files in the
            //directory and process the content of the files
            foreach (string assemblyFolder in _analyzerSettings.AssemblyDirectories())
            {
                Logger.LogUserMessage($"Assembly folder: {assemblyFolder}");
                foreach (string assemblyFilename in Directory.EnumerateFiles(assemblyFolder))
                {
                    BinaryFile assemblyFile = new BinaryFile(assemblyFilename, _progress, _analyzerSettings.Transformation.IncludedNames);
                    if (assemblyFile.Exists && assemblyFile.IsAssembly && Accept(assemblyFilename))
                    {
                        _assemblyFiles.Add(assemblyFile);
                        _resolver.AddSearchPath(assemblyFile);
                        UpdateAssemblyProgress(false);
                    }
                }
                UpdateAssemblyProgress(true);
            }
        }

        private void FindTypes()
        {
            foreach (BinaryFile assemblyFile in _assemblyFiles)
            {
                assemblyFile.FindTypes(_resolver);
                foreach (DotNetType type in assemblyFile.Types)
                {
                    _model.AddElement(type.Name, type.Type, null);
                }
            }
        }

        private void FindRelations()
        {
            foreach (BinaryFile assemblyFile in _assemblyFiles)
            {
                assemblyFile.FindRelations();
                foreach (DotNetRelation relation in assemblyFile.Relations)
                {
                    _model.AddRelation(relation.ConsumerName, relation.ProviderName, relation.Type, 1, null);
                }
            }
        }


        private bool Accept(string name)
        {
            /*  Check whether the name meets the regex descriptions that have been configured.
             *  If no regex descriptions have been configured then the file is accepted.
             */
            bool accept = false;

            if (_analyzerSettings.Input.IncludeAssemblyNames?.Count > 0)
            {
                string fileNameWithoutExtension = "";
                try
                {
                    fileNameWithoutExtension = Path.GetFileNameWithoutExtension(name);
                }
                catch (ArgumentException)
                {
                    //The file does not exist
                }

                if (fileNameWithoutExtension.Length > 0)
                {
                    foreach (string regexIncludedAssembly in _analyzerSettings.Input.IncludeAssemblyNames)
                    {
                        Regex regex = new Regex(regexIncludedAssembly);
                        Match match = regex.Match(fileNameWithoutExtension);
                        if (match.Success)
                        {
                            accept = true;
                        }

                    }
                }
            }
            else
            {
                accept = true;
            }

            return accept;
        }



        private void UpdateAssemblyProgress(bool done)
        {
            ProgressInfo progressInfo = new ProgressInfo
            {
                ActionText = "Finding assemblies",
                CurrentItemCount = _assemblyFiles.Count,
                TotalItemCount = 0,
                ItemType = "assemblies",
                Percentage = null,
                Done = done
            };
            _progress?.Report(progressInfo);
        }
    }
}
