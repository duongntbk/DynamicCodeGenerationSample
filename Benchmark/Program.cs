using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        }
    }

    [MemoryDiagnoser]
    public class TestCreatorRunner
    {
        private Assembly _assembly;
        private Type _type;
        private Func<object> _delegate;

        [GlobalSetup]
        public void Setup()
        {
            _assembly = typeof(DynamicCodeGenerationSample.TestClass).Assembly;
            _type = _assembly.GetType("DynamicCodeGenerationSample.TestClass");

            var dynCtor = new DynamicMethod($"{_type.FullName}_ctor", _type, Type.EmptyTypes, true);
            var il = dynCtor.GetILGenerator();
            var ctorInfo = _type.GetConstructor(Type.EmptyTypes);

            il.Emit(OpCodes.Newobj, ctorInfo);
            il.Emit(OpCodes.Ret);

            _delegate = (Func<object>)dynCtor.CreateDelegate(typeof(Func<object>));
        }

        [Benchmark]
        public object Reflection_CreateInstance() => Activator.CreateInstance(_type);

        [Benchmark]
        public object DynamicGenerator_CreateInstance() => _delegate();

        [Benchmark]
        public object Static_CreateInstance() => new DynamicCodeGenerationSample.TestClass();
    }

    [MemoryDiagnoser]
    public class TestMethodRunner
    {
        private Assembly _assembly;
        private Type _type;
        private MethodInfo _methodInfo;
        private Func<object, object[], object> _delegate;
        private DynamicCodeGenerationSample.TestClass _instance;
        private object[] _arguments;

        [GlobalSetup]
        public void Setup()
        {
            _assembly = typeof(DynamicCodeGenerationSample.TestClass).Assembly;
            _type = _assembly.GetType("DynamicCodeGenerationSample.TestClass");
            _methodInfo = _type.GetMethod("Introduce");
            _instance = new DynamicCodeGenerationSample.TestClass();
            _arguments = new[] { "Duong", "Nguyen" };

            var parameterTypes = new[] { typeof(object), typeof(object) };
            var methodParameters = _methodInfo.GetParameters();
            var dynInk = new DynamicMethod($"{_type.FullName}_{_methodInfo.Name}_Ink", typeof(object), parameterTypes, true);
            var il = dynInk.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, _type);

            var paramLen = _arguments.Length;
            for (var i = 0; i < paramLen; i++)
            {
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldc_I4_S, i);
                il.Emit(OpCodes.Ldelem_Ref);
                il.Emit(OpCodes.Castclass, methodParameters[i].ParameterType);
            }

            il.EmitCall(OpCodes.Call, _methodInfo, null);
            il.Emit(OpCodes.Ret);

            _delegate = (Func<object, object[], object>)dynInk.CreateDelegate(typeof(Func<object, object[], object>));
        }

        [Benchmark]
        public object Reflection_Invoke() => _methodInfo.Invoke(_instance, _arguments);

        [Benchmark]
        public object DynamicGenerator_Invoke() => _delegate(_instance, _arguments);

        [Benchmark]
        public object Static_Invoke() => _instance.Introduce("Duong", "Nguyen");
    }
}
