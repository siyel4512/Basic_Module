using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Quest/Task/Target/String", fileName = "Target_")]
public class StringTarget : TaskTarget
{
    [SerializeField]
    private string value;

    public override object Value => value;

    public override bool IsEqual(object target)
    {
        string targetAsString = target as string; // target을 string형으로 casting해줌

        // 실패
        if (targetAsString == null)
            return false;

        return value == targetAsString; // 성공 (같음)
    }
}
