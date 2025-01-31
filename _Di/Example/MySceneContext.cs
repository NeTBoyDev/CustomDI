using System;
using System.Collections.Generic;
using DI;
using UnityEngine;

namespace Example
{
    public class MySceneContext : SceneContext
    {
        public override void RegisterDependencies()
        {
            Register<IsBasicService, IsnputService>(true);
        }
    }
}
