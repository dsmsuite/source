using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using DsmSuite.Common.Util;
using System.Text.RegularExpressions;
using Mono.Cecil;

namespace DsmSuite.Analyzer.DotNet.Lib
{
    public class BinaryFile
    {
        private readonly IProgress<ProgressInfo> _progress;
        private readonly List<TypeDefinition> _typeList = new List<TypeDefinition>();
        private readonly List<String> _includedAssemblyStrings = new List<String>();

        public BinaryFile(string filename, IProgress<ProgressInfo> progress, List<String> includedAssemblyStrings)
        {
            FileInfo = new FileInfo(filename);
            _progress = progress;
            if (includedAssemblyStrings != null)
                _includedAssemblyStrings.AddRange(includedAssemblyStrings);
        }

        public List<DotNetType> Types { get; } = new List<DotNetType>();

        public List<DotNetRelation> Relations { get; } = new List<DotNetRelation>();

        public FileInfo FileInfo { get; }

        public bool Exists => FileInfo.Exists;

        public bool IsAssembly
        {
            get
            {
                try
                {
                    AssemblyName.GetAssemblyName(FileInfo.FullName); // Gives exception when no assembly

                    return ((FileInfo.Extension == ".exe") || (FileInfo.Extension == ".dll")) &&
                            !FileInfo.Name.EndsWith(".vshost.exe");
                }
                catch
                {
                    return false;
                }
            }
        }

        public void FindTypes(DotNetResolver resolver)
        {
            try
            {
                AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(FileInfo.FullName, resolver.ReaderParameters);

                foreach (ModuleDefinition module in assembly.Modules)
                {
                    var moduleTypes = module.Types;

                    foreach (TypeDefinition typeDecl in moduleTypes)
                    {
                        if ((typeDecl != null) && (typeDecl.Name != "<Module>") && !isClrSupportType(typeDecl))
                        {
                            AnalyseTypeElements(typeDecl);
                            UpdateTypeProgress(false);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogException($"Analysis failed assembly={FileInfo.FullName} failed", e);
            }
            UpdateTypeProgress(true);
        }

        private bool isClrSupportType(TypeDefinition typeDecl)
        {
            return typeDecl.Name.StartsWith("_") ||
                   typeDecl.Name.StartsWith("tag") ||
                   typeDecl.Name.StartsWith("IMPORT_OBJECT_") ||
                   typeDecl.Name.StartsWith("SYSGEO") ||
                   typeDecl.Name == "IMAGE_AUX_SYMBOL_TYPE" ||
                   typeDecl.Name == "ReplacesCorHdrNumericDefines" ||
                   typeDecl.Name == "ORIENTATION_PREFERENCE" ||
                   typeDecl.Name == "SYSNLS_FUNCTION" ||
                   typeDecl.Name == "RPC_ADDRESS_CHANGE_TYPE" ||
                   typeDecl.Name == "RpcProxyPerfCounters" ||
                   typeDecl.Name == "CO_MARSHALING_CONTEXT_ATTRIBUTES" ||
                   typeDecl.Name == "CWMO_FLAGS" ||
                   typeDecl.Name == "VARENUM" ||
                   typeDecl.Name == "PIDMSI_STATUS_VALUE" ||
                   typeDecl.Name == "ValidatorFlags" ||
                   typeDecl.Name == "ETaskType" ||
                   typeDecl.Name == "ISA_AVAILABILITY" ||
                   typeDecl.Name == "IUnknown" ||
                   typeDecl.Name == "HWND__" ||
                   typeDecl.Name == "ICLRRuntimeHost" ||
                   typeDecl.Name == "HINSTANCE__" ||
                   typeDecl.Name == "HDC__" ||
                   typeDecl.Name == "ICorRuntimeHost";
        }

        private void AnalyseTypeElements(TypeDefinition typeDecl)
        {
            try
            {
                RegisterType(typeDecl);
            }
            catch (Exception e)
            {
                Logger.LogException($"Analysis failed assembly={FileInfo.FullName} type={typeDecl.Name} failed", e);
            }

            foreach (TypeDefinition nestedType in typeDecl.NestedTypes)
            {
                try
                {
                    RegisterType(nestedType);
                }
                catch (Exception e)
                {
                    Logger.LogException($"Analysis failed assembly={FileInfo.FullName} type={typeDecl.Name} nestedType={nestedType.Name}", e);
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

        public void FindRelations()
        {
            foreach (TypeDefinition typeDecl in _typeList)
            {
                AnalyseTypeRelations(typeDecl);
                UpdateRelationProgress(false);
            }
            UpdateRelationProgress(true);
        }

        private void AnalyseTypeRelations(TypeDefinition typeDecl)
        {
            AnalyzeTypeInterfaces(typeDecl);
            AnalyzeTypeBaseClass(typeDecl);
            AnalyzeTypeFields(typeDecl);
            AnalyzeTypeProperties(typeDecl);
            AnalyseTypeMethods(typeDecl);
        }

        private void AnalyzeTypeInterfaces(TypeDefinition typeDecl)
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
                    Logger.LogException($"Analysis failed assembly={FileInfo.Name} type={typeDecl.Name} interface={interf.Name}", e);
                }
            }
        }

        private void AnalyzeTypeBaseClass(TypeDefinition typeDecl)
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
                Logger.LogException($"Analysis failed assembly={FileInfo.Name} type={typeDecl.Name}", e);
            }
        }

        private void AnalyzeTypeFields(TypeDefinition typeDecl)
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
                    Logger.LogException($"Analysis failed assembly={FileInfo.Name} type={typeDecl.Name} field={fieldDecl.Name}", e);
                }
            }
        }

        private void AnalyzeTypeProperties(TypeDefinition typeDecl)
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
                    Logger.LogException($"Analysis failed assembly={FileInfo.Name} type={typeDecl.Name} property={propertyDecl.Name}", e);
                }
            }
        }

        private void AnalyseTypeMethods(TypeDefinition typeDecl)
        {
            foreach (MethodDefinition method in typeDecl.Methods)
            {
                try
                {
                    AnalyzeGenericMethodParameters(typeDecl, method);
                    AnalyzeMethodParameters(typeDecl, method);
                    AnalyzeMethodReturnType(typeDecl, method);
                    AnalyseMethodBody(typeDecl, method);
                }
                catch (Exception e)
                {
                    Logger.LogException("Analysis failed assembly={assemblyFileInfo.FullName} type={typeDecl.Name} method={method.Name}", e);
                }
            }
        }

        private void AnalyzeGenericMethodParameters(TypeDefinition typeDecl,
            MethodDefinition method)
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
                        Logger.LogException($"Analysis failed assembly={FileInfo.Name} type={typeDecl.Name} constraint={constraint.Name}", e);
                    }
                }
            }
        }

        private void AnalyzeMethodParameters(TypeDefinition typeDecl, MethodDefinition method)
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
                    Logger.LogException($"Analysis failed assembly={FileInfo.Name} type={typeDecl.Name} method={method.Name} parameter={paramDecl.Name}", e);
                }
            }
        }

        private void AnalyzeMethodReturnType(TypeDefinition typeDecl, MethodDefinition method)
        {
            TypeReference returnType = method.ReturnType;

            try
            {
                string context = "Analyze return type of method " + typeDecl.Name + "::" + method.Name;
                RegisterRelation(returnType, typeDecl, "return", context);
            }
            catch (Exception e)
            {
                Logger.LogException($"Analysis failed assemblyFileInfo={FileInfo.Name} type={typeDecl.Name} method={method.Name} return={returnType.Name}", e);
            }
        }

        private void AnalyseMethodBody(TypeDefinition typeDecl, MethodDefinition method)
        {
            Mono.Cecil.Cil.MethodBody body = method.Body;

            try
            {
                if (body != null)
                {
                    AnalyzeLocalVariables(typeDecl, method, body);
                    AnalyzeBodyTypeReferences(typeDecl, method, body);
                }
            }
            catch (Exception e)
            {
                Logger.LogException($"Analysis failed asssmbly={FileInfo.Name} type={typeDecl.Name} method={method.Name}", e);
            }
        }

        private void AnalyzeLocalVariables(TypeDefinition typeDecl, MethodDefinition method,
            Mono.Cecil.Cil.MethodBody body)
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
                    Logger.LogException($"Analysis failed assembly={FileInfo.Name} type={typeDecl.Name} method={method.Name} variable={variable}", e);
                }
            }
        }

        private void AnalyzeBodyTypeReferences(TypeDefinition typeDecl, MethodDefinition method,
            Mono.Cecil.Cil.MethodBody body)
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
                                    string context = "Analyze type references of method " + typeDecl.Name + "::" +
                                                     method.Name;
                                    RegisterRelation(t, typeDecl, "reference", context);
                                }
                                else
                                {
                                    MemberReference m = op as MemberReference;
                                    if (m != null)
                                    {
                                        string context = "Analyze member references of method " + typeDecl.Name + "::" +
                                                         method.Name;
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

        private bool Accept(string name)
        {
            bool accept = false;
            if (_includedAssemblyStrings.Count() > 0)
            {
                foreach (string ignoredName in _includedAssemblyStrings)
                {
                    Regex regex = new Regex(ignoredName);
                    Match match = regex.Match(name);
                    if (match.Success)
                    {
                        accept = true;
                    }
                }
            }
            else
            {
                accept = true;
            }
            return accept;
        }

        private void RegisterType(TypeDefinition typeDecl)
        {
            DotNetType myDotNetType = new DotNetType(typeDecl.GetElementType().ToString(), DetermineType(typeDecl));
            if (Accept(myDotNetType.Name))
            {
                Types.Add(myDotNetType);
                _typeList.Add(typeDecl);
                UpdateTypeProgress(false);
            }
        }

        private void RegisterRelation(TypeReference providerType, TypeReference consumerType, string type, string context)
        {
            if ((providerType != null) && (consumerType != null))
            {
                string consumerName = consumerType.GetElementType().ToString();
                string providerName = providerType.GetElementType().ToString();

                if (!providerType.ContainsGenericParameter)
                {
                    Relations.Add(new DotNetRelation(consumerName, providerName, type));
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

        private void UpdateTypeProgress(bool done)
        {
            ProgressInfo progressInfo = new ProgressInfo
            {
                ActionText = "Finding types: " + FileInfo.Name,
                CurrentItemCount = Types.Count,
                TotalItemCount = 0,
                ItemType = "types",
                Percentage = null,
                Done = done
            };
            _progress?.Report(progressInfo);
        }

        private void UpdateRelationProgress(bool done)
        {
            ProgressInfo progressInfo = new ProgressInfo
            {
                ActionText = "Finding relations: " + FileInfo.Name,
                CurrentItemCount = Relations.Count,
                TotalItemCount = 0,
                ItemType = "relations",
                Percentage = null,
                Done = done
            };
            _progress?.Report(progressInfo);
        }
    }
}

