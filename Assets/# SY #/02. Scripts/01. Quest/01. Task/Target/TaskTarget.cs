using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TaskTarget : ScriptableObject
{
    public abstract object Value { get; } // Target�� �ܺη� ������ �� �ִ� ������Ƽ

    // QuestSystem�� ����� Target�� Task�� ������ Target�� ������ Ȯ���ϴ� �Լ�
    public abstract bool IsEqual(object target);
}
