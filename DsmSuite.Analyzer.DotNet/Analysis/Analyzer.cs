﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DsmSuite.Analyzer.DotNet.Settings;
using DsmSuite.Analyzer.Model.Interface;
using DsmSuite.Common.Util;
using Mono.Cecil;

namespace DsmSuite.Analyzer.DotNet.Analysis
{
    /// <summary>
    /// .Net code analyzer which uses Mono.Cecil to analyze dependencies between types in .Net binaries
    /// </summary>
    public class Analyzer
    {
        private readonly IDsiModel _model;
        private readonly AnalyzerSettings _analyzerSettings;
        private readonly IList<TypeDefinition> _typeList = new List<TypeDefinition>();
        private readonly Dictionary<string, FileInfo> _typeAssemblyInfoList = new Dictionary<string, FileInfo>();
        private readonly List<FileInfo> _assemblyFileInfos = new List<FileInfo>();

        public Analyzer(IDsiModel model, AnalyzerSettings analyzerSettings)
        {
            _model = model;
            _analyzerSettings = analyzerSettings;
        }

        public void Analyze()
        {
            Logger.LogUserMessage("Analyzing");
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            FindAssemblies();
            FindTypes();
            FindRelations();

            Logger.LogResourceUsage();

            stopWatch.Stop();
            Logger.LogUserMessage($" total elapsed time={stopWatch.Elapsed}");
        }

        private void FindAssemblies()
        {
            Logger.LogUserMessage("Finding assemblies");
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            foreach (string assemblyFilename in Directory.EnumerateFiles(_analyzerSettings.AssemblyDirectory))
            {
                RegisterAssembly(assemblyFilename);
            }

            stopWatch.Stop();
            Logger.LogUserMessage($"elapsed time={stopWatch.Elapsed}");
        }

        private void FindTypes()
        {
            Logger.LogUserMessage("Finding types");
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            ReaderParameters readerParameters = DetermineAssemblyReaderParameters();

            foreach (FileInfo assemblyFileInfo in _assemblyFileInfos)
            {
                Logger.LogUserMessage("Analyzing assembly: " + assemblyFileInfo.FullName);

                try
                {
                    AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(assemblyFileInfo.FullName, readerParameters);

                    foreach (ModuleDefinition module in assembly.Modules)
                    {
                        var moduleTypes = module.Types;

                        foreach (TypeDefinition typeDecl in moduleTypes)
                        {
                            if ((typeDecl != null) && (typeDecl.Name != "<Module>"))
                            {
                                AnalyseTypeElements(assemblyFileInfo, typeDecl);
                            }
                        }
                    }

                    Logger.LogUserMessage(" analysis " + assemblyFileInfo.FullName + " successful");
                }
                catch (Exception e)
                {
                    Logger.LogException($"Analysis failed assembly={assemblyFileInfo.FullName} failed", e);
                }
            }

            stopWatch.Stop();
            Logger.LogUserMessage($"elapsed time={stopWatch.Elapsed}");
        }

        private void FindRelations()
        {
            Logger.LogUserMessage("Find relations");
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            foreach (TypeDefinition typeDecl in _typeList)
            {
                FileInfo assemblyInfo = _typeAssemblyInfoList[typeDecl.FullName];
                AnalyseTypeRelations(assemblyInfo, typeDecl);
            }

            stopWatch.Stop();
            Logger.LogUserMessage($"elapsed time={stopWatch.Elapsed}");
        }

        private ReaderParameters DetermineAssemblyReaderParameters()
        {
            var resolver = new DefaultAssemblyResolver();

            IDictionary<string, bool> paths = new Dictionary<string, bool>();

            foreach (FileInfo assemblyFileInfo in _assemblyFileInfos)
            {
                if (assemblyFileInfo.Exists)
                {
                    if (assemblyFileInfo.DirectoryName != null && paths.ContainsKey(assemblyFileInfo.DirectoryName) == false)
                    {
                        paths.Add(assemblyFileInfo.DirectoryName, true);
                        resolver.AddSearchDirectory(assemblyFileInfo.DirectoryName);
                    }
                }
            }

            ReaderParameters readerParameters = new ReaderParameters() { AssemblyResolver = resolver };
            return readerParameters;
        }

        private void AnalyseTypeElements(FileInfo assemblyFileInfo, TypeDefinition typeDecl)
        {
            try
            {
                RegisterType(assemblyFileInfo, typeDecl);
            }
            catch (Exception e)
            {
                Logger.LogException($"Analysis failed assembly={assemblyFileInfo.FullName} type={typeDecl.Name} failed", e);
            }

            foreach (TypeDefinition nestedType in typeDecl.NestedTypes)
            {
                try
                {
                    RegisterType(assemblyFileInfo, nestedType);
                }
                catch (Exception e)
                {
                    Logger.LogException($"Analysis failed assembly={assemblyFileInfo.FullName} type={typeDecl.Name} nestedType={nestedType.Name}", e);
                }
            }
        }

        private static string DetermineType(TypeDefinition typeDecl)
        {
            string type;
            if (typeDecl.IsClass)
            {
                type = typeDecl.IsEnum ? "enum" : "class";
            }
            else if (typeDecl.IsInterface)
            {
                type = "interface";
            }
            else
            {
                type = "?";
            }

            if (typeDecl.HasGenericParameters)
            {
                type = "generic " + type;
            }

            return type;
        }

        private void AnalyseTypeRelations(FileInfo assemblyFileInfo, TypeDefinition typeDecl)
        {
            AnalyzeTypeInterfaces(assemblyFileInfo, typeDecl);
            AnalyzeTypeBaseClass(assemblyFileInfo, typeDecl);
            AnalyzeTypeFields(assemblyFileInfo, typeDecl);
            AnalyzeTypeProperties(assemblyFileInfo, typeDecl);
            AnalyseTypeMethods(assemblyFileInfo, typeDecl);
        }

        private void AnalyzeTypeInterfaces(FileInfo assemblyFileInfo, TypeDefinition typeDecl)
        {
            foreach (TypeReference interf in typeDecl.Interfaces)
            {
                try
                {
                    string context = "Analyze interfaces of type " + typeDecl.Name;
                    RegisterRelation(interf, typeDecl, "realization", context);
                }
                catch (Exception e)
                {
                    Logger.LogException($"Analysis failed assembly={assemblyFileInfo.FullName} type={typeDecl.Name} interface={interf.Name}", e);
                }
            }
        }

        private void AnalyzeTypeBaseClass(FileInfo assemblyFileInfo, TypeDefinition typeDecl)
        {
            try
            {
                if (typeDecl.BaseType != null)
                {
                    string context = "Analyze base class of type " + typeDecl.Name;
                    RegisterRelation(typeDecl.BaseType, typeDecl, "generalization", context);
                }
            }
            catch (Exception e)
            {
                Logger.LogException($"Analysis failed assembly={assemblyFileInfo.FullName} type={typeDecl.Name}", e);
            }
        }

        private void AnalyzeTypeFields(FileInfo assemblyFileInfo, TypeDefinition typeDecl)
        {
            foreach (FieldDefinition fieldDecl in typeDecl.Fields)
            {
                try
                {
                    string context = "Analyze fields of type " + typeDecl.Name;
                    RegisterRelation(fieldDecl.FieldType, typeDecl, "field", context);
                }
                catch (Exception e)
                {
                    Logger.LogException($"Analysis failed assembly={assemblyFileInfo.FullName} type={typeDecl.Name} field={fieldDecl.Name}", e);
                }
            }
        }

        private void AnalyzeTypeProperties(FileInfo assemblyFileInfo, TypeDefinition typeDecl)
        {
            foreach (PropertyDefinition propertyDecl in typeDecl.Properties)
            {
                try
                {
                    string context = "Analyze properties of type " + typeDecl.Name;
                    RegisterRelation(propertyDecl.PropertyType, typeDecl, "property", context);
                }
                catch (Exception e)
                {
                    Logger.LogException($"Analysis failed assembly={assemblyFileInfo.FullName} type={typeDecl.Name} property={propertyDecl.Name}", e);
                }
            }
        }

        private void AnalyseTypeMethods(FileInfo assemblyFileInfo, TypeDefinition typeDecl)
        {
            foreach (MethodDefinition method in typeDecl.Methods)
            {
                try
                {
                    AnalyzeGenericMethodParameters(assemblyFileInfo, typeDecl, method);
                    AnalyzeMethodParameters(assemblyFileInfo, typeDecl, method);
                    AnalyzeMethodReturnType(assemblyFileInfo, typeDecl, method);
                    AnalyseMethodBody(assemblyFileInfo, typeDecl, method);
                }
                catch (Exception e)
                {
                    Logger.LogException("Analysis failed assembly={assemblyFileInfo.FullName} type={typeDecl.Name} method={method.Name}", e);
                }
            }
        }

        private void AnalyzeGenericMethodParameters(FileInfo assemblyFileInfo, TypeDefinition typeDecl, MethodDefinition method)
        {
            foreach (GenericParameter genericArgument in method.GenericParameters)
            {
                foreach (TypeReference constraint in genericArgument.Constraints)
                {
                    try
                    {
                        string context = "Analyze generic parameters of method " + typeDecl.Name + "::" + method.Name;
                        RegisterRelation(constraint, typeDecl, "parameter", context);
                    }
                    catch (Exception e)
                    {
                        Logger.LogException($"Analysis failed assembly={assemblyFileInfo.FullName} type={typeDecl.Name} constraint={constraint.Name}", e);
                    }
                }
            }
        }

        private void AnalyzeMethodParameters(FileInfo assemblyFileInfo, TypeDefinition typeDecl, MethodDefinition method)
        {
            foreach (ParameterDefinition paramDecl in method.Parameters)
            {
                try
                {
                    string context = "Analyze parameters of method " + typeDecl.Name + "::" + method.Name;
                    RegisterRelation(paramDecl.ParameterType, typeDecl, "parameter", context);
                }
                catch (Exception e)
                {
                    Logger.LogException($"Analysis failed assembly={assemblyFileInfo.FullName} type={typeDecl.Name} method={method.Name} parameter={paramDecl.Name}", e);
                }
            }
        }

        private void AnalyzeMethodReturnType(FileInfo assemblyFileInfo, TypeDefinition typeDecl, MethodDefinition method)
        {
            TypeReference returnType = method.ReturnType;

            try
            {
                string context = "Analyze return type of method " + typeDecl.Name + "::" + method.Name;
                RegisterRelation(returnType, typeDecl, "return", context);
            }
            catch (Exception e)
            {
                Logger.LogException($"Analysis failed assemblyFileInfo={assemblyFileInfo.FullName} type={typeDecl.Name} method={method.Name} return={returnType.Name}", e);
            }
        }

        private void AnalyseMethodBody(FileInfo assemblyFileInfo, TypeDefinition typeDecl, MethodDefinition method)
        {
            Mono.Cecil.Cil.MethodBody body = method.Body;

            try
            {
                if (body != null)
                {
                    AnalyzeLocalVariables(assemblyFileInfo, typeDecl, method, body);
                    AnalyzeBodyTypeReferences(typeDecl, method, body);
                }
            }
            catch (Exception e)
            {
                Logger.LogException($"Analysis failed asssmbly={assemblyFileInfo.FullName} type={typeDecl.Name} method={method.Name}", e);
            }
        }

        private void AnalyzeLocalVariables(FileInfo assemblyFileInfo, TypeDefinition typeDecl, MethodDefinition method, Mono.Cecil.Cil.MethodBody body)
        {
            foreach (Mono.Cecil.Cil.VariableDefinition variable in body.Variables)
            {
                try
                {
                    string context = "Analyze local variables of method " + typeDecl.Name + "::" + method.Name;
                    RegisterRelation(variable.VariableType, typeDecl, "local", context);
                }
                catch (Exception e)
                {
                    Logger.LogException($"Analysis failed assembly={assemblyFileInfo.FullName} type={typeDecl.Name} method={method.Name} variable={variable}", e);
                }
            }
        }

        private void AnalyzeBodyTypeReferences(TypeDefinition typeDecl, MethodDefinition method, Mono.Cecil.Cil.MethodBody body)
        {
            int index = 0;

            var instructions = body.Instructions;
            while (index < instructions.Count)
            {
                var i = instructions[index];
                var opCode = i.OpCode;

                switch (opCode.OperandType)
                {
                    case Mono.Cecil.Cil.OperandType.InlineTok:
                    case Mono.Cecil.Cil.OperandType.InlineType:
                    case Mono.Cecil.Cil.OperandType.InlineMethod:
                    case Mono.Cecil.Cil.OperandType.InlineField:
                        {
                            object op = i.Operand;

                            if (op == null)
                            {
                                Logger.LogError("Unexpected null operand in method=" + method.Name);
                            }
                            else
                            {
                                TypeReference t = op as TypeReference;
                                if (t != null)
                                {
                                    string context = "Analyze type references of method " + typeDecl.Name + "::" + method.Name;
                                    RegisterRelation(t, typeDecl, "reference", context);
                                }
                                else
                                {
                                    MemberReference m = op as MemberReference;
                                    if (m != null)
                                    {
                                        string context = "Analyze member references of method " + typeDecl.Name + "::" + method.Name;
                                        RegisterRelation(m.DeclaringType, typeDecl, "reference", context);
                                    }
                                    else
                                    {
                                        Logger.LogError("Unhandled token type: " + op + " in method = " + method.Name);
                                    }
                                }
                            }
                        }
                        break;
                }

                index++;
            }
        }

        private void RegisterAssembly(string assemblyFilename)
        {
            FileInfo fileInfo = new FileInfo(assemblyFilename);

            if (((fileInfo.Extension == ".exe") || (fileInfo.Extension == ".dll")) && (!fileInfo.Name.EndsWith(".vshost.exe")))
            {
                _assemblyFileInfos.Add(fileInfo);
            }
        }

        private void RegisterType(FileInfo assemblyFileInfo, TypeDefinition typeDecl)
        {
            string typeName = typeDecl.GetElementType().ToString();
            if (!IsExternal(typeName))
            {
                if (
                    _model.AddElement(typeDecl.GetElementType().ToString(), DetermineType(typeDecl),
                        assemblyFileInfo.FullName) != null)
                {
                    _typeList.Add(typeDecl);
                    _typeAssemblyInfoList[typeDecl.FullName] = assemblyFileInfo;
                }
            }
        }


        private void RegisterRelation(TypeReference providerType, TypeReference consumerType, string type, string context)
        {
            if ((providerType != null) && (consumerType != null))
            {
                string consumerName = consumerType.GetElementType().ToString();
                string providerName = providerType.GetElementType().ToString();

                if (!providerType.ContainsGenericParameter &&
                    !IsExternal(providerName))
                {
                    _model.AddRelation(consumerName, providerName, type, 1, context);
                }

                GenericInstanceType providerGenericType = providerType as GenericInstanceType;
                if (providerGenericType != null)
                {
                    foreach (TypeReference providerGenericArgumentType in providerGenericType.GenericArguments)
                    {
                        RegisterRelation(providerGenericArgumentType, consumerType, type, context);
                    }
                }
            }
        }

        private bool IsExternal(string providerName)
        {
            bool isExternalType = false;

            foreach (string externalName in _analyzerSettings.ExternalNames)
            {
                if (providerName.StartsWith(externalName))
                {
                    isExternalType = true;
                }
            }
            return isExternalType;
        }
    }
}

