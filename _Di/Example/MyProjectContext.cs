using System;
using System.Collections.Generic;
using DI;

namespace Example
{
    public class MyProjectContext : ProjectContext
    {
        public override void RegisterDependencies()
        {
            Register<IsBasicService, IsnputService>(true);
        }
    }
}
