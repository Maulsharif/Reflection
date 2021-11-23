using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Task1.DoNotChange;

namespace Task1
{
    public class Container
    {
        private List<Assembly> Assemblies { get; set; }
        private Dictionary<Type, Type> TypesDependencies { get; set; }
        private Dictionary<Type, Func<object>> Types { get; set; }

        public Container ()
	    {
            Assemblies = new List<Assembly>();
            TypesDependencies = new Dictionary<Type, Type>();
            Types = new Dictionary<Type, Func<object>>();
	    }

        public void AddAssembly(Assembly assembly)
        {
            var types = GetAllTypes(assembly);
            foreach (var type in types)
            {
                AddType(type);
            }

        }
       

        public void AddType(Type type)
        {
           var attribute = type.GetCustomAttribute(typeof(ExportAttribute)) as ExportAttribute;
          
            if (attribute != null &&
               attribute.Contract != null &&
               (type.GetInterfaces().Any(inter => inter == attribute.Contract) || type.BaseType == attribute.Contract))
            {
                TypesDependencies.Add(attribute.Contract, type);
            }
          
            else
            {
                TypesDependencies.Add(type, type);
            }

            RegisterType(type);
        }

        public void AddType(Type type, Type baseType)
        {
            TypesDependencies.Add(baseType, type);
            TypesDependencies.Add(type, type);
            RegisterType(type);
        }

        public T Get<T>()
        {
           return (T)CreateObject(typeof(T));
        }

        private IEnumerable<Type> GetAllTypes(Assembly assembly)
        {

            ValidationException.ThrowIf(Assemblies.Any(a => a == assembly), new Exception("This assembly already registered."));
            Assemblies.Add(assembly);

            var types = assembly.GetTypes()
                                .Where(type => type.GetCustomAttribute(typeof(ExportAttribute)) != null ||
                                               type.GetProperties().Any(prop => prop.GetCustomAttribute(typeof(ImportAttribute)) != null) ||
                                               type.GetCustomAttribute(typeof(ImportConstructorAttribute)) != null);
            return types;

        }
        private void RegisterType(Type type)
        {
            var constructors = type.GetConstructors();
            ValidationException.ThrowIf(constructors.Count() > 1, new Exception($"{type.FullName} have several constructors"));

            var registredProperties = type.GetProperties()
                .Where(prop => prop.GetCustomAttribute(typeof(ImportAttribute)) != null);
            ValidationException.ThrowIf(registredProperties.Any(prop => prop.PropertyType == type), new Exception($"{type.FullName} have the same type properties"));

            var constructor = constructors.FirstOrDefault();
            var defaultConstructor = constructors.FirstOrDefault(constr => constr.GetParameters().Length == 0);

            if (constructor != defaultConstructor)
            {
                var constructorParameters = constructor.GetParameters();
                Func<object> typeActivator = () => constructor.Invoke(constructorParameters.Select(praram => CreateObject(praram.ParameterType)).ToArray());
                Types.Add(type, typeActivator);
            }
            else if (defaultConstructor != null)
            {
                Func<object> typeActivator = () => defaultConstructor.Invoke(new object[0]);
                Types.Add(type, typeActivator);
            }
        }

        private object CreateObject(Type type)
        {
            var resultType = TypesDependencies[type];
            var resultObject = Types[resultType]();

            var properties = type.GetProperties()
                .Where(prop => prop.GetCustomAttribute(typeof(ImportAttribute)) != null);

            foreach (var prop in properties)
            {
                prop.SetValue(resultObject, CreateObject(prop.PropertyType));
            }

            return resultObject;
        }

    }
}