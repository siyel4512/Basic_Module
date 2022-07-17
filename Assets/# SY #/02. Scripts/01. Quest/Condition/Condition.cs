using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Condition : ScriptableObject
{
    // 조건에 대한 설명
    [SerializeField]
    private string decription;

    // 조건을 통과했는지 
    public abstract bool IsPass(Quest quest);
}
