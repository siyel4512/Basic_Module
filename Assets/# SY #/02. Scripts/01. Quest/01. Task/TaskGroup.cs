using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum TaskGroupState
{
    Inactive,
    Running,
    Complete
}

[System.Serializable]
public class TaskGroup
{
    [SerializeField]
    private Task[] tasks;

    public IReadOnlyList<Task> Tasks => tasks; // 읽기 전용으로 만듬
    //public Task[] Tasks => tasks;

    public Quest Owner { get; private set; } // TaskGroup의 소유주 정의

    public bool IsAllTaskComplete => tasks.All(x => x.isComplete); // Task들이 다 완료되었는지 확인해주는 Property
                                                                   // tasks.All의 .All은 Linq를 사용한것이며
                                                                   // 참고 블로그 https://ibocon.tistory.com/106

    public bool IsComplete => State == TaskGroupState.Complete; // 완료 확인을 하는 Property

    public TaskGroupState State { get; private set; }

    // 소유주를 Setting해 줄 Setup함수
    public void Setup(Quest owner)
    {
        Owner = owner;
        foreach (var task in tasks)
            task.Setup(owner);
    }
    
    // Quest가 가진 여러 개의 TaskGroup중에 현재 작동해야 하는 TaskGroup이 사작될 때 실행 됨
    public void Start()
    {
        // 상태 세팅
        State = TaskGroupState.Running;
        foreach (var task in tasks)
            task.Start();
    }

    public void End()
    {
        foreach (var task in tasks)
            task.End();
    }

    // Task에 성공 횟수를 전달해줄 ReceiveReport함수
    public void ReceiveReport(string category, object target, int successCount)
    {
        foreach (var task in tasks)
        {
            // Task가 해당 ㅊategory와 Target을 가지고 있다면 목표 대상이므로 보고를 받음
            if (task.IsTarget(category, target))
            {
                task.ReceiveReport(successCount);
            }
        }
    }

    // 완료 처리를 하는 Complete 함수
    public void Complete()
    {
        if (IsComplete)
            return;

        State = TaskGroupState.Complete;

        foreach (var task in tasks)
        {
            if (!task.isComplete)
            {
                task.Complete();
            }
        }
    }
}
