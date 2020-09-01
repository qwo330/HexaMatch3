using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Top : Block
{
    int AdjcentMatchCount = 3;

    public override void SetBlock(Slot s)
    {
        base.SetBlock(s);
    }
}
