using System;
using System.Collections.Generic;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;

namespace GpgPatcher
{
    internal sealed class ServiceLibPatchResult
    {
        public bool AvailableSettingsPatched { get; set; }

        public bool LaunchSettingsPatched { get; set; }

        public bool AlreadyPatched
        {
            get { return !AvailableSettingsPatched && !LaunchSettingsPatched; }
        }
    }

    internal static class ServiceLibPatcher
    {
        public static ServiceLibPatchResult Patch(string serviceLibPath, string outputPath)
        {
            using (var module = ModuleDefMD.Load(serviceLibPath))
            {
                var serviceType = module.Types.FirstOrDefault(type => type.FullName == GpgConstants.ServiceTypeName);
                if (serviceType == null)
                {
                    throw new FriendlyException("Could not find AppSessionScope in ServiceLib.dll.");
                }

                var availableMethod = FindTargetMethod(serviceType, GpgConstants.AvailableSettingsMethodName);
                var launchMethod = FindTargetMethod(serviceType, GpgConstants.LaunchSettingsMethodName);

                var result = new ServiceLibPatchResult
                {
                    AvailableSettingsPatched = PatchMethod(module, availableMethod, GpgConstants.PatchAvailableSettingsMethod),
                    LaunchSettingsPatched = PatchMethod(module, launchMethod, GpgConstants.PatchAndroidDisplaySettingsMethod),
                };

                var options = new ModuleWriterOptions(module)
                {
                    Logger = DummyLogger.NoThrowInstance,
                };

                module.Write(outputPath, options);
                return result;
            }
        }

        public static MethodDef FindTargetMethod(TypeDef type, string name)
        {
            var method = type.Methods.FirstOrDefault(candidate =>
                string.Equals(candidate.Name, name, StringComparison.Ordinal)
                && !candidate.IsStatic
                && candidate.MethodSig != null
                && candidate.MethodSig.Params.Count == 2
                && candidate.MethodSig.Params[0].FullName == "Google.Hpe.Service.V1.DisplaySize"
                && candidate.MethodSig.Params[1].FullName == "Google.Hpe.Service.V1.LaunchGameRequest");

            if (method == null)
            {
                throw new FriendlyException("Could not find target method '" + name + "' in AppSessionScope.");
            }

            if (method.Body == null)
            {
                throw new FriendlyException("Target method '" + name + "' has no IL body.");
            }

            return method;
        }

        public static bool HasAnyHookCall(MethodDef method, string hookMethodName)
        {
            return HasHookCall(method, hookMethodName)
                || HasLegacyHookCall(method, hookMethodName);
        }

        public static bool HasHookCall(MethodDef method, string hookMethodName)
        {
            return HasHookCall(method, hookMethodName, GpgConstants.HookTypeNamespace);
        }

        public static bool HasLegacyHookCall(MethodDef method, string hookMethodName)
        {
            return HasHookCall(method, hookMethodName, GpgConstants.LegacyHookTypeNamespace);
        }

        private static bool HasHookCall(MethodDef method, string hookMethodName, string hookTypeNamespace)
        {
            if (method == null || method.Body == null)
            {
                return false;
            }

            foreach (var instruction in method.Body.Instructions)
            {
                if (instruction.OpCode != OpCodes.Call)
                {
                    continue;
                }

                var operand = instruction.Operand as IMethod;
                if (operand == null)
                {
                    continue;
                }

                if (string.Equals(operand.Name, hookMethodName, StringComparison.Ordinal)
                    && operand.DeclaringType != null
                    && string.Equals(operand.DeclaringType.ReflectionNamespace, hookTypeNamespace, StringComparison.Ordinal)
                    && string.Equals(operand.DeclaringType.Name, GpgConstants.HookTypeName, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool PatchMethod(ModuleDef module, MethodDef method, string hookMethodName)
        {
            if (HasHookCall(method, hookMethodName))
            {
                return false;
            }

            if (HasLegacyHookCall(method, hookMethodName))
            {
                throw new FriendlyException(
                    "A legacy pre-rename patch was detected. Restore the original files first, then apply the current GPG Patcher build.");
            }

            if (method.IsStatic || method.MethodSig.Params.Count != 2)
            {
                throw new FriendlyException("Target method signature changed for '" + method.Name + "'.");
            }

            var hookMethod = CreateHookMethodReference(module, method, hookMethodName);
            var body = method.Body;
            var resultLocal = new Local(method.ReturnType);
            body.Variables.Add(resultLocal);
            body.InitLocals = true;

            var returns = new List<Instruction>();
            foreach (var instruction in body.Instructions)
            {
                if (instruction.OpCode == OpCodes.Ret)
                {
                    returns.Add(instruction);
                }
            }

            if (returns.Count == 0)
            {
                throw new FriendlyException("Target method '" + method.Name + "' had no return instructions.");
            }

            foreach (var ret in returns)
            {
                var index = body.Instructions.IndexOf(ret);
                ret.OpCode = OpCodes.Stloc;
                ret.Operand = resultLocal;
                body.Instructions.Insert(index + 1, Instruction.Create(OpCodes.Ldloc, resultLocal));
                body.Instructions.Insert(index + 2, Instruction.Create(OpCodes.Ldarg_2));
                body.Instructions.Insert(index + 3, Instruction.Create(OpCodes.Call, hookMethod));
                body.Instructions.Insert(index + 4, Instruction.Create(OpCodes.Ret));
            }

            body.MaxStack = (ushort)Math.Max((int)body.MaxStack, 8);
            return true;
        }

        private static MemberRef CreateHookMethodReference(ModuleDef module, MethodDef targetMethod, string hookMethodName)
        {
            var assemblyRef = module.GetAssemblyRefs()
                .FirstOrDefault(reference => string.Equals(reference.Name, GpgConstants.HookAssemblyName, StringComparison.Ordinal));

            if (assemblyRef == null)
            {
                var assemblyName = new AssemblyNameInfo(GpgConstants.HookAssemblyName);
                assemblyRef = new AssemblyRefUser(assemblyName);
            }

            var hookType = new TypeRefUser(
                module,
                GpgConstants.HookTypeNamespace,
                GpgConstants.HookTypeName,
                assemblyRef);

            var returnType = targetMethod.ReturnType;
            var requestType = targetMethod.MethodSig.Params[1];
            var signature = MethodSig.CreateStatic(
                returnType,
                returnType,
                requestType);

            return new MemberRefUser(module, hookMethodName, signature, hookType);
        }
    }
}
