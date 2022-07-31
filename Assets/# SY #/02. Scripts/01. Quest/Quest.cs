using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Diagnostics; // Debugging code를 사용하기 위해 라이브러리 선언

using Debug = UnityEngine.Debug; // System.Diagnostics에도 Debug가 존개하기때문에서 설정

public enum QuestState
{
    Inactive, // 비활성화
    Runnning,
    Complete,
    Cancel,
    WaitingForCompletion // 게임 퀘스트를 보면 Task를 모두 완료했을때 자동으로 완료되는 퀘스트가 있고
                         // 내가 완료 버튼을 눌러야 완료되는 퀘스트가 있는데, 수동으로 퀘스트를 완료시켜야하는 상태
}

[CreateAssetMenu(menuName ="Quest/Quest", fileName ="Quest_")]
public class Quest : ScriptableObject
{
    #region Events
    // 보고 받았을때 실행할 event
    public delegate void TaskSuccessChangedHandler(Quest quest, Task task, int currentSuccess, int prevSuccess);
    // Quest를 완료했을때 실행할 event
    public delegate void CompletedHandler(Quest quest);
    // Quest를 취소했을때 실행할 event
    public delegate void CanceledHandler(Quest quest);
    // 새로운 TaskGroup이 시작되었을때 실행할 event
    public delegate void NewTaskGroupHandler(Quest quest, TaskGroup currentTaskGroup, TaskGroup prevTaskGroup);
    #endregion

    [SerializeField]
    private Category category;

    [SerializeField]
    private Sprite icon;

    [Header("Text")]
    [SerializeField] private string codeName;
    [SerializeField] private string displayName;
    [SerializeField, TextArea] private string description;// 설명 내용이 길어질 수 있기때문에 TextArea Attribute 추가


    [Header("Task")]
    [SerializeField]
    private TaskGroup[] taskGroups;

    [Header("Reward")]
    [SerializeField]
    private Reward[] rewards;

    [Header("Option")]
    [SerializeField]
    private bool useAutoComplete; // 해당 퀘스트가 Task 완료시 자동으로 완료시킬것인지, 수동으로 확인버튼을 눌러야 하는지
    [SerializeField]
    private bool isCancelable;

    [Header("Condition")]
    [SerializeField]
    private Condition[] acceptionCondition;
    [SerializeField]
    private Condition[] cancelCondition;

    private int currentTaskGroupIndex; // 현재 TaskGroup이 몇 번째인지 알 수 있게 index 변수 선언

    // 프로퍼티 (외부에서 사용할것들...)
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

    // Awake()역할을 수행하는 함수로 Quest가 System에 등록되었을대 실행됨
    public void OnRegister()
    {
        // Assert란 인자로 들어온 값이 false면, 뒤의 문장을 Error로 표시해 줌
        // Assert()함수는 프로그래머가 예상하기에 절대 일어나서는 안되는 조건이 일어났을 때 검출하기 위한 코드이다.
        // Assert()함수는 Debugging Code라서 게임을 Build해서 뽑아내면 Code가 무시됨, 때문에 몇백줄이 되더라도 성능에 영향을 주지 않는다.
        // 이를 "방어적 프로그래밍"이라고 부름
        // 때문에 시간이 더 걸리더라도 Assort Code를 작성하는 습관을 들이는 것이 좋다.
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

    // 보고를 받을 ReceiveReport()함수
    public void ReceiveReport(string category, object target, int successCount)
    {
        Debug.Assert(IsRegistered, "This quest has already been registered.");
        Debug.Assert(!IsCancel, "This quest has been canceled.");
        // Iscomplete를 Assert롤 확인하지 않는 이유는 Quest가 Complete가 되었는데도 보고를 받을 수도 있는 상황이 있기 때문

        if (IsComplete)
            return;

        CurrentTaskGroup.ReceiveReport(category, target, successCount);

        if (CurrentTaskGroup.IsAllTaskComplete)
        {
            // 다음 TaskGroup이 없다면
            if (currentTaskGroupIndex + 1 == taskGroups.Length)
            {
                State = QuestState.WaitingForCompletion;

                // 만약 autComplete가 활성화되어 있다면
                if (useAutoComplete)
                    Complete();
            }
            // 다음 TaskGroup이 존재한다면
            else
            {
                // TaskGroup을 가져오면서 index를 증가시킴
                var prevTaskGroup = taskGroups[currentTaskGroupIndex++];

                // 이전 TaskGroup은 끝내줌
                prevTaskGroup.End();

                // 새로운 현재 TaskGroup은 시작
                CurrentTaskGroup.Start();

                // 새로운 TaskGroup이 시작되었다는 것을 event를 통해 알려줌
                onNewTaskGroup?.Invoke(this, CurrentTaskGroup, prevTaskGroup);
            }
        }
        // All Complete가 아니라면
        else
        {
            // State를 Running으로 변경
            // Task Option중에 완료가 되었어도 다시 보고를 받아야하는 상황이 올 수 있기 때문에
            State = QuestState.Runnning;
        }
    }

    // Quest를 완료하는 Complete()함수
    public void Complete()
    {
        CheckIsRunning();

        foreach (var taskGroup in taskGroups)
            taskGroup.Complete(); // 퀘스트를 즉시 완료해주는 Item이나 SaveSystem에 의해서 Quest의 Task가 다 완료가 되지 않았는데 Complete하는 경우를 위해서....

        State = QuestState.Complete;

        // Quest를 받아서 Clear하면 보상을 받음
        foreach (var reward in rewards)
            reward.Give(this);

        onCompleted?.Invoke(this);

        onTaskSuccessChaged = null;
        onCompleted = null;
        onCanceled = null;
        onNewTaskGroup = null;
    }

    // Quest를 최소하는 Cancel()함수
    public virtual void Cancel()
    {
        CheckIsRunning();
        Debug.Assert(IsCancelable, "Tis quest can't be canceled.");

        State = QuestState.Cancel;
        onCanceled?.Invoke(this);
    }

    // Clone 함수
    public Quest Clone()
    {
        var clone = Instantiate(this);
        clone.taskGroups = taskGroups.Select(x => new TaskGroup(x)).ToArray();

        return clone;
    }

    // onTaskSuccessChanged event를 Task의 event에 등록하기 위한 Callback함수
    private void OnSuccessChanged(Task task, int currentSuccess, int prevSuccess)
        => onTaskSuccessChaged?.Invoke(this, task, currentSuccess, prevSuccess);


    // Debugging code를 관리할 함수
    [Conditional("UNITY_EDITOR")] // Conditional Attribute는 인자로 전달한 Simbol값이 선언되어 있으면 함수를 실행하고, 아니라면 함수를 무시하게 해주는 Attribute이다.
    private void CheckIsRunning()
    {
        Debug.Assert(IsRegistered, "This quest has already been registered.");
        Debug.Assert(!IsCancel, "This quest has been canceled.");
        Debug.Assert(!IsCompletable, "This quest has already been completed.");
    }
}
