using System;
using System.Reflection;
using System.Reflection.Emit;

namespace DynamicCodeGenerationSample
{
    class Program
    {
        static void Main(string[] args)
        {
            DemoCreateInstance();
            DemoCallGetLastName();
            DemoCallSetName();
            DemoCallIntroduce();
        }

        static void DemoCreateInstance()
        {
            var assemblyPath = $"{AppDomain.CurrentDomain.BaseDirectory}/DynamicCodeGenerationSample.dll";
            var typeName = "DynamicCodeGenerationSample.TestClass";
            var assembly = Assembly.LoadFrom(assemblyPath);
            var type = assembly.GetType(typeName);

            var dynCtor = new DynamicMethod($"{type.FullName}_ctor", type, Type.EmptyTypes, true);
            var il = dynCtor.GetILGenerator();
            var ctorInfo = type.GetConstructor(Type.EmptyTypes);

            il.Emit(OpCodes.Newobj, ctorInfo);
            il.Emit(OpCodes.Ret);

            var ctorDelegate = (Func<object>)dynCtor.CreateDelegate(typeof(Func<object>));

            var instance = ctorDelegate();
            Console.WriteLine($"Instance is of type: {instance.GetType().FullName}");
        }

        static void DemoCallGetLastName()
        {
            var assemblyPath = $"{AppDomain.CurrentDomain.BaseDirectory}/DynamicCodeGenerationSample.dll";
            var typeName = "DynamicCodeGenerationSample.TestClass";
            var assembly = Assembly.LoadFrom(assemblyPath);
            var type = assembly.GetType(typeName);
            var methodInfo = type.GetMethod("GetLastName");

            var dynInk = new DynamicMethod($"{type.FullName}_{methodInfo.Name}_Ink",
                typeof(string), new[] { typeof(object) }, true);
            var il = dynInk.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, type);
            il.EmitCall(OpCodes.Call, methodInfo, null);
            il.Emit(OpCodes.Ret);

            var inkDelegate = (Func<object, string>)dynInk.CreateDelegate(typeof(Func<object, string>));

            object instance = new TestClass(); // or you can create this instance with dynamic code generation
            var lastName = inkDelegate(instance);
            Console.WriteLine($"Last name is: {lastName}");
        }

        static void DemoCallSetName()
        {
            var assemblyPath = $"{AppDomain.CurrentDomain.BaseDirectory}/DynamicCodeGenerationSample.dll";
            var typeName = "DynamicCodeGenerationSample.TestClass";
            var assembly = Assembly.LoadFrom(assemblyPath);
            var type = assembly.GetType(typeName);
            var methodInfo = type.GetMethod("SetName");

            var parameterTypes = new[]
            {
                typeof(object),
                typeof(string),
                typeof(string)
            };
            var dynInk = new DynamicMethod($"{type.FullName}_{methodInfo.Name}_Ink", null, parameterTypes, true);
            var il = dynInk.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, type);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_2);
            il.EmitCall(OpCodes.Call, methodInfo, null);
            il.Emit(OpCodes.Ret);

            var inkDelegate = (Action<object, string, string>)dynInk
                .CreateDelegate(typeof(Action<object, string, string>));
            object instance = new TestClass(); // or you can create this instance with dynamic code generation
            inkDelegate(instance, "new first name", "new last name");
            ((TestClass)instance).PrintDetails();
        }

        static void DemoCallIntroduce()
        {
            var arguments = new object[] { "James", "Bond" };

            var assemblyPath = $"{AppDomain.CurrentDomain.BaseDirectory}/DynamicCodeGenerationSample.dll";
            var typeName = "DynamicCodeGenerationSample.TestClass";
            var assembly = Assembly.LoadFrom(assemblyPath);
            var type = assembly.GetType(typeName);
            var methodInfo = type.GetMethod("Introduce");

            var parameterTypes = new[] { typeof(object), typeof(object[]) };
            var dynInk = new DynamicMethod($"{type.FullName}_{methodInfo.Name}_Ink", typeof(object), parameterTypes, true);
            var il = dynInk.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, type);

            var methodParameters = methodInfo.GetParameters();
            var paramLen = arguments.Length;
            for (var i = 0; i < paramLen; i++)
            {
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldc_I4_S, i);
                il.Emit(OpCodes.Ldelem_Ref);
                il.Emit(OpCodes.Castclass, methodParameters[i].ParameterType);
            }

            il.EmitCall(OpCodes.Call, methodInfo, null);

            if(methodInfo.ReturnType.IsValueType)
            {
                il.Emit(OpCodes.Box, methodInfo.ReturnType);
            }

            il.Emit(OpCodes.Ret);

            var inkDelegate = (Func<object, object[], object>)dynInk
                .CreateDelegate(typeof(Func<object, object[], object>));

            object instance = new TestClass(); // or you can create this instance with dynamic code generation
            var result = inkDelegate(instance, arguments);
            Console.WriteLine(result);
        }

        public static object CreateClass() => new TestClass();

        public static string CallGetFirstName(object instance) => ((TestClass)instance).GetLastName();

        public static void CallSetName(object instance, string first, string last) =>
            ((TestClass)instance).SetName(first, last);

        public static object CallIntroduce(object instance, object[] arguments) =>
            ((TestClass)instance).Introduce((string)arguments[0], (string)arguments[1]);
    }
}
