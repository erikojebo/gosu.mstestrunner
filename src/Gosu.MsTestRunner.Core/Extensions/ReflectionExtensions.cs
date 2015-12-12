using System;

namespace Gosu.MsTestRunner.Core.Extensions
{
    public static class ReflectionExtensions
    {
        public static T CreateInstance<T>(this AppDomain appDomain) where T : MarshalByRefObject
        {
            return (T)CreateInstance(appDomain, typeof(T));
        }

        public static object CreateInstance(this AppDomain appDomain, Type typeToInstantiate)
        {
            return appDomain.CreateInstanceFromAndUnwrap(
                    typeToInstantiate.Assembly.Location,
                    typeToInstantiate.FullName);
        }

        public static int GetInheritanceHierarchyDepth(this Type arg)
        {
            return GetInheritanceHierarchyDepth(arg.BaseType, 0);
        }

        private static int GetInheritanceHierarchyDepth(this Type arg, int startingDepth)
        {
            if (arg.BaseType == null)
            {
                return startingDepth;
            }

            return GetInheritanceHierarchyDepth(arg.BaseType, startingDepth + 1);
        }

    }
}