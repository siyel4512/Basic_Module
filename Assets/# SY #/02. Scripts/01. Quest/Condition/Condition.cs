using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Condition : ScriptableObject
{
    // ���ǿ� ���� ����
    [SerializeField]
    private string decription;

    // ������ ����ߴ��� 
    public abstract bool IsPass(Quest quest);
}
