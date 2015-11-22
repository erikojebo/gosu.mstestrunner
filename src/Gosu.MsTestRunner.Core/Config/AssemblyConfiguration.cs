using System;

namespace Gosu.MsTestRunner.Core.Config
{
    [Serializable]
    public class AssemblyConfiguration : MarshalByRefObject
    {
        public bool AllowParallel { get; set; }
        public string Path { get; set; } 
    }
}