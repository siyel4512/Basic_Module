using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum TaskState
{
    Inactive,
    Running,
    Complete
}

// ���� �����ؾ��� �������̱⶧���� ScirptableObject�� ��ӹ޾ƾ���
[CreateAssetMenu(menuName = "Quest/Task/Task", fileName = "Task_")]
public class Task : ScriptableObject
{
    #region Envents

    // State�� ���� ������ �˷��ִ� �̺�Ʈ
    // ������ ������ ���� �� ������
    // ��ǥ������ UI Update Code�� Event�� ������ ������
    // Task�� ���¸� Update���� ��� ������ �ʿ� ���̻��°� �ٲ�� �˾Ƽ� UI�� Update��
    public delegate void StateChangedHandler(Task task, TaskState currentState, TaskState prevState);

    // CurrentSuccess ���� ������ �� �˷��ִ� event (�ٸ� ������ ��� Update�� �����ϱ� �ʾƵ� �ǰ� �ϱ� ������...
    public delegate void SuccessChangedHandle(Task task, int currentSuccess, int prevSuccess);

    #endregion

    [SerializeField]
    private Category category;

    [Header("Text")]
    [SerializeField]
    private string codeName; // �ܺο��� �������� �̸��� �ƴ� ���α׷��Ӱ� �˻��� ���� ��� ����� ���� ���������� ����ϴ� �̸�. ������ ID ���� �������� �����ص� �ɵ�

    [SerializeField]
    private string description; // �ش� Task�� � Task ������ �˷��� Description�̴�.

    [Header("Action")]
    [SerializeField]
    private TaskAction action;

    [Header("Target")]
    [SerializeField]
    private TaskTarget[] targets;// �� Task�� Target�� ���� �� �� �� �ֱ⶧��

    [Header("Settings")]
    [SerializeField]
    private InitialSuccessValue initialSuccessValue;

    [SerializeField]
    private int needSuccessToComplete; // Task�� �����ϱ� ���� �ʿ��� ���� Ƚ���� ������ ����� �� (��ǥ)

    [SerializeField]
    private bool canReceiveRportsDuringCompletion; // Task�� �Ϸ�Ǿ�� ��� ���� Ƚ���� ���� ���� ������ Ȯ���ϴ� �ɼ�

    private TaskState state; // Task ����
    private int currentSuccess;

    public event StateChangedHandler onStateChaged;
    public event SuccessChangedHandle onSuccessChaged;

    // ������Ƽ
    //public int CurrentSuccess { get; private set; } // ���� ������ Ƚ�� (������Ƽ)
    public TaskState State
    {
        get => state;

        set
        {
            var prevState = state;
            state = value;
            onStateChaged?.Invoke(this, state, prevState); // ?.�� �̺���(onStateChaged)�� null�̸� null�� ��ȯ�ϰ� �ƴϸ� �ڿ� �Լ�(Invoke)�� �÷��϶�� �ǹ�
        }
    }

    public string CodeName => codeName; // ������Ƽ ������̵�
                                        // public string CodeName() => codeName;
                                        // public string CodeName { get => codeName;} 
                                        // public string CodeName { get return codeName;} 
                                        // public string CodeName() { return codeName;}
                                        // �� ���� �����̴�.
                                        // ���� ��α�1 https://stackoverflow.com/questions/31764532/what-does-the-operator-mean-in-a-property
                                        // ���� ��α�2 https://stackoverflow.com/questions/55779199/what-means-in-property-declaration-not-a-lambda-expression
    public string Description => description;
    public int NeedSuccessToComplete => needSuccessToComplete;
    public Category Category => category;

    public int CurrentSuccess
    {
        get => currentSuccess;

        set
        {
            int prevSuccess = currentSuccess;
            currentSuccess = Mathf.Clamp(value, 0, needSuccessToComplete);
            if (currentSuccess != prevSuccess)
            {
                state = currentSuccess == needSuccessToComplete ? TaskState.Complete : TaskState.Running;
                onSuccessChaged?.Invoke(this, currentSuccess, prevSuccess);
            }
        }
    }

    public bool isComplete => State == TaskState.Complete;

    public Quest Owner; // �� Task�� ���� Quest�� ��������...

    // Awake������ �� Setup�Լ�
    public void Setup(Quest owner)
    {
        Owner = owner;
    }
    // Task�� ���۵Ǿ����� ������ Start�Լ�
    public void Start()
    {
        State = TaskState.Running;

        if (initialSuccessValue)
            CurrentSuccess = initialSuccessValue.GetValue(this);
    }

    // Task�� ������ �������� ������ End�Լ�
    public void End()
    {
        // event reset
        onStateChaged = null;
        onSuccessChaged = null;
    }

    // �ܺο��� CurrentSuccess���� ������ �� �ִ� �Լ�
    public void ReceiveReport(int successCount)
    {
        // Logic�� ������ ������� ��ȯ�� ��
        // ù��° ���ڰ��� Task�� ������ �̷� Module�̳� event���� ������ ��ü�� �������� �������ִ°� ������� ��
        CurrentSuccess = action.Run(this, CurrentSuccess, successCount);
    }

    // Task�� ��� �Ϸ��� �� �ִ� Complete�Լ�
    public void Complete()
    {
        CurrentSuccess = needSuccessToComplete;
    }

    // TaskTarget�� ����, �� Task�� ���� Ƚ���� ���� ���� ������� Ȯ���ϴ� �Լ�
    public bool IsTarget(string category, object target)
    => Category == category &&
        targets.Any(x => x.IsEqual(target)) && // �ϳ��� ���ǿ� �´� ��Ұ� �ִ��� Ȯ��
        (!isComplete || (isComplete && canReceiveRportsDuringCompletion));
}
