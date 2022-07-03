using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TaskTarget : ScriptableObject
{
    public abstract object Value { get; } // Target을 외부로 가져올 수 있는 프로퍼티

    // QuestSystem에 보고된 Target이 Task에 설정한 Target과 같은지 확인하는 함수
    public abstract bool IsEqual(object target);
}
