using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Diagnostics; // Debugging code�� ����ϱ� ���� ���̺귯�� ����

using Debug = UnityEngine.Debug; // System.Diagnostics���� Debug�� �����ϱ⶧������ ����

public enum QuestState
{
    Inactive, // ��Ȱ��ȭ
    Runnning,
    Complete,
    Cancel,
    WaitingForCompletion // ���� ����Ʈ�� ���� Task�� ��� �Ϸ������� �ڵ����� �Ϸ�Ǵ� ����Ʈ�� �ְ�
                         // ���� �Ϸ� ��ư�� ������ �Ϸ�Ǵ� ����Ʈ�� �ִµ�, �������� ����Ʈ�� �Ϸ���Ѿ��ϴ� ����
}

[CreateAssetMenu(menuName ="Quest/Quest", fileName ="Quest_")]
public class Quest : ScriptableObject
{
    #region Events
    // ���� �޾����� ������ event
    public delegate void TaskSuccessChangedHandler(Quest quest, Task task, int currentSuccess, int prevSuccess);
    // Quest�� �Ϸ������� ������ event
    public delegate void CompletedHandler(Quest quest);
    // Quest�� ��������� ������ event
    public delegate void CanceledHandler(Quest quest);
    // ���ο� TaskGroup�� ���۵Ǿ����� ������ event
    public delegate void NewTaskGroupHandler(Quest quest, TaskGroup currentTaskGroup, TaskGroup prevTaskGroup);
    #endregion

    [SerializeField]
    private Category category;

    [SerializeField]
    private Sprite icon;

    [Header("Text")]
    [SerializeField] private string codeName;
    [SerializeField] private string displayName;
    [SerializeField, TextArea] private string description;// ���� ������ ����� �� �ֱ⶧���� TextArea Attribute �߰�


    [Header("Task")]
    [SerializeField]
    private TaskGroup[] taskGroups;

    [Header("Reward")]
    [SerializeField]
    private Reward[] rewards;

    [Header("Option")]
    [SerializeField]
    private bool useAutoComplete; // �ش� ����Ʈ�� Task �Ϸ�� �ڵ����� �Ϸ��ų������, �������� Ȯ�ι�ư�� ������ �ϴ���
    [SerializeField]
    private bool isCancelable;

    [Header("Condition")]
    [SerializeField]
    private Condition[] acceptionCondition;
    [SerializeField]
    private Condition[] cancelCondition;

    private int currentTaskGroupIndex; // ���� TaskGroup�� �� ��°���� �� �� �ְ� index ���� ����

    // ������Ƽ (�ܺο��� ����Ұ͵�...)
    public Category Category => category;
    public Sprite Icon => icon;
    public string CodeName => codeName;
    public string DisplayName => displayName;
    public string Decription => description;
    public QuestState State { get; private set; }
    public TaskGroup CurrentTaskGroup => taskGroups[currentTaskGroupIndex];
    public IReadOnlyList<TaskGroup> TaskGroups => taskGroups;
    public IReadOnlyList<Reward> Rewards => rewards;
    public bool IsRegistered => State != QuestState.Inactive;
    public bool IsCompletable => State == QuestState.WaitingForCompletion;
    public bool IsComplete => State == QuestState.Complete;
    public bool IsCancel => State == QuestState.Cancel;
    public virtual bool IsCancelable => isCancelable && cancelCondition.All(x => x.IsPass(this));
    public bool IsAcception => acceptionCondition.All(x => x.IsPass(this));

    public event TaskSuccessChangedHandler onTaskSuccessChaged;
    public event CompletedHandler onCompleted;
    public event CanceledHandler onCanceled;
    public event NewTaskGroupHandler onNewTaskGroup;

    // Awake()������ �����ϴ� �Լ��� Quest�� System�� ��ϵǾ����� �����
    public void OnRegister()
    {
        // Assert�� ���ڷ� ���� ���� false��, ���� ������ Error�� ǥ���� ��
        // Assert()�Լ��� ���α׷��Ӱ� �����ϱ⿡ ���� �Ͼ���� �ȵǴ� ������ �Ͼ�� �� �����ϱ� ���� �ڵ��̴�.
        // Assert()�Լ��� Debugging Code�� ������ Build�ؼ� �̾Ƴ��� Code�� ���õ�, ������ ������� �Ǵ��� ���ɿ� ������ ���� �ʴ´�.
        // �̸� "����� ���α׷���"�̶�� �θ�
        // ������ �ð��� �� �ɸ����� Assort Code�� �ۼ��ϴ� ������ ���̴� ���� ����.
        Debug.Assert(!IsRegistered, "This quest has already been registred.");

        foreach (var taskGroup in taskGroups)
        {
            taskGroup.Setup(this);
            foreach (var task in taskGroup.Tasks)
                task.onSuccessChaged += OnSuccessChanged;
        }

        State = QuestState.Runnning;
        CurrentTaskGroup.Start();
    }

    // ���� ���� ReceiveReport()�Լ�
    public void ReceiveReport(string category, object target, int successCount)
    {
        Debug.Assert(IsRegistered, "This quest has already been registered.");
        Debug.Assert(!IsCancel, "This quest has been canceled.");
        // Iscomplete�� Assert�� Ȯ������ �ʴ� ������ Quest�� Complete�� �Ǿ��µ��� ���� ���� ���� �ִ� ��Ȳ�� �ֱ� ����

        if (IsComplete)
            return;

        CurrentTaskGroup.ReceiveReport(category, target, successCount);

        if (CurrentTaskGroup.IsAllTaskComplete)
        {
            // ���� TaskGroup�� ���ٸ�
            if (currentTaskGroupIndex + 1 == taskGroups.Length)
            {
                State = QuestState.WaitingForCompletion;

                // ���� autComplete�� Ȱ��ȭ�Ǿ� �ִٸ�
                if (useAutoComplete)
                    Complete();
            }
            // ���� TaskGroup�� �����Ѵٸ�
            else
            {
                // TaskGroup�� �������鼭 index�� ������Ŵ
                var prevTaskGroup = taskGroups[currentTaskGroupIndex++];

                // ���� TaskGroup�� ������
                prevTaskGroup.End();

                // ���ο� ���� TaskGroup�� ����
                CurrentTaskGroup.Start();

                // ���ο� TaskGroup�� ���۵Ǿ��ٴ� ���� event�� ���� �˷���
                onNewTaskGroup?.Invoke(this, CurrentTaskGroup, prevTaskGroup);
            }
        }
        // All Complete�� �ƴ϶��
        else
        {
            // State�� Running���� ����
            // Task Option�߿� �Ϸᰡ �Ǿ�� �ٽ� ���� �޾ƾ��ϴ� ��Ȳ�� �� �� �ֱ� ������
            State = QuestState.Runnning;
        }
    }

    // Quest�� �Ϸ��ϴ� Complete()�Լ�
    public void Complete()
    {
        CheckIsRunning();

        foreach (var taskGroup in taskGroups)
            taskGroup.Complete(); // ����Ʈ�� ��� �Ϸ����ִ� Item�̳� SaveSystem�� ���ؼ� Quest�� Task�� �� �Ϸᰡ ���� �ʾҴµ� Complete�ϴ� ��츦 ���ؼ�....

        State = QuestState.Complete;

        // Quest�� �޾Ƽ� Clear�ϸ� ������ ����
        foreach (var reward in rewards)
            reward.Give(this);

        onCompleted?.Invoke(this);

        onTaskSuccessChaged = null;
        onCompleted = null;
        onCanceled = null;
        onNewTaskGroup = null;
    }

    // Quest�� �ּ��ϴ� Cancel()�Լ�
    public virtual void Cancel()
    {
        CheckIsRunning();
        Debug.Assert(IsCancelable, "Tis quest can't be canceled.");

        State = QuestState.Cancel;
        onCanceled?.Invoke(this);
    }

    // Clone �Լ�
    public Quest Clone()
    {
        var clone = Instantiate(this);
        clone.taskGroups = taskGroups.Select(x => new TaskGroup(x)).ToArray();

        return clone;
    }

    // onTaskSuccessChanged event�� Task�� event�� ����ϱ� ���� Callback�Լ�
    private void OnSuccessChanged(Task task, int currentSuccess, int prevSuccess)
        => onTaskSuccessChaged?.Invoke(this, task, currentSuccess, prevSuccess);


    // Debugging code�� ������ �Լ�
    [Conditional("UNITY_EDITOR")] // Conditional Attribute�� ���ڷ� ������ Simbol���� ����Ǿ� ������ �Լ��� �����ϰ�, �ƴ϶�� �Լ��� �����ϰ� ���ִ� Attribute�̴�.
    private void CheckIsRunning()
    {
        Debug.Assert(IsRegistered, "This quest has already been registered.");
        Debug.Assert(!IsCancel, "This quest has been canceled.");
        Debug.Assert(!IsCompletable, "This quest has already been completed.");
    }
}
