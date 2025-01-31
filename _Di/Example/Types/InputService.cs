using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsnputService : IsBasicService
{
    public Vector2 GetInput() => new Vector2(1,1);
}

public interface IsBasicService
{
    public Vector2 GetInput();
}
