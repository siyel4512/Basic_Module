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

    public IReadOnlyList<Task> Tasks => tasks; // �б� �������� ����
    //public Task[] Tasks => tasks;

    public Quest Owner { get; private set; } // TaskGroup�� ������ ����

    public bool IsAllTaskComplete => tasks.All(x => x.isComplete); // Task���� �� �Ϸ�Ǿ����� Ȯ�����ִ� Property
                                                                   // tasks.All�� .All�� Linq�� ����Ѱ��̸�
                                                                   // ���� ��α� https://ibocon.tistory.com/106

    public bool IsComplete => State == TaskGroupState.Complete; // �Ϸ� Ȯ���� �ϴ� Property

    public TaskGroupState State { get; private set; }

    // �����ָ� Setting�� �� Setup�Լ�
    public void Setup(Quest owner)
    {
        Owner = owner;
        foreach (var task in tasks)
            task.Setup(owner);
    }
    
    // Quest�� ���� ���� ���� TaskGroup�߿� ���� �۵��ؾ� �ϴ� TaskGroup�� ���۵� �� ���� ��
    public void Start()
    {
        // ���� ����
        State = TaskGroupState.Running;
        foreach (var task in tasks)
            task.Start();
    }

    public void End()
    {
        foreach (var task in tasks)
            task.End();
    }

    // Task�� ���� Ƚ���� �������� ReceiveReport�Լ�
    public void ReceiveReport(string category, object target, int successCount)
    {
        foreach (var task in tasks)
        {
            // Task�� �ش� ��ategory�� Target�� ������ �ִٸ� ��ǥ ����̹Ƿ� ���� ����
            if (task.IsTarget(category, target))
            {
                task.ReceiveReport(successCount);
            }
        }
    }

    // �Ϸ� ó���� �ϴ� Complete �Լ�
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
