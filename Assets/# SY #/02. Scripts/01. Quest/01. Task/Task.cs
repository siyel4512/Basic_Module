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

// 추후 생성해야할 데이터이기때문에 ScirptableObject를 상속받아야함
[CreateAssetMenu(menuName = "Quest/Task/Task", fileName = "Task_")]
public class Task : ScriptableObject
{
    #region Envents

    // State가 변할 때마다 알려주는 이벤트
    // 다향한 곳에서 사용될 수 있으며
    // 대표적으로 UI Update Code를 Event에 연결해 놓으면
    // Task의 상태를 Update에서 계속 추적할 필요 없이상태가 바뀌면 알아서 UI가 Update됨
    public delegate void StateChangedHandler(Task task, TaskState currentState, TaskState prevState);

    // CurrentSuccess 값이 변했을 때 알려주는 event (다름 곳에서 계속 Update로 추적하기 않아도 되게 하기 위함임...
    public delegate void SuccessChangedHandle(Task task, int currentSuccess, int prevSuccess);

    #endregion

    [SerializeField]
    private Category category;

    [Header("Text")]
    [SerializeField]
    private string codeName; // 외부에서 보여지는 이름이 아닌 프로그래머가 검색과 같은 어떠한 기능을 위해 내부적으로 사용하는 이름. 일종의 ID 같은 개념으로 생각해도 될듯

    [SerializeField]
    private string description; // 해당 Task가 어떤 Task 인지를 알려줄 Description이다.

    [Header("Action")]
    [SerializeField]
    private TaskAction action;

    [Header("Target")]
    [SerializeField]
    private TaskTarget[] targets;// 한 Task의 Target이 여러 개 일 수 있기때문

    [Header("Settings")]
    [SerializeField]
    private InitialSuccessValue initialSuccessValue;

    [SerializeField]
    private int needSuccessToComplete; // Task가 성공하기 위해 필요한 성공 횟수를 변수로 만들어 줌 (목표)

    [SerializeField]
    private bool canReceiveRportsDuringCompletion; // Task가 완료되었어도 계속 성공 횟수를 보고 받을 것인지 확인하는 옵션

    private TaskState state; // Task 상태
    private int currentSuccess;

    public event StateChangedHandler onStateChaged;
    public event SuccessChangedHandle onSuccessChaged;

    // 프로퍼티
    //public int CurrentSuccess { get; private set; } // 현재 성공한 횟수 (프로퍼티)
    public TaskState State
    {
        get => state;

        set
        {
            var prevState = state;
            state = value;
            onStateChaged?.Invoke(this, state, prevState); // ?.은 이변수(onStateChaged)가 null이면 null을 반환하고 아니면 뒤에 함수(Invoke)를 시랭하라는 의미
        }
    }

    public string CodeName => codeName; // 프로퍼티 람디식이디
                                        // public string CodeName() => codeName;
                                        // public string CodeName { get => codeName;} 
                                        // public string CodeName { get return codeName;} 
                                        // public string CodeName() { return codeName;}
                                        // 과 같은 형태이다.
                                        // 참고 블로그1 https://stackoverflow.com/questions/31764532/what-does-the-operator-mean-in-a-property
                                        // 참고 블로그2 https://stackoverflow.com/questions/55779199/what-means-in-property-declaration-not-a-lambda-expression
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

    public Quest Owner; // 이 Task를 가진 Quest가 누구인지...

    // Awake역살을 할 Setup함수
    public void Setup(Quest owner)
    {
        Owner = owner;
    }
    // Task가 시작되었을대 실행할 Start함수
    public void Start()
    {
        State = TaskState.Running;

        if (initialSuccessValue)
            CurrentSuccess = initialSuccessValue.GetValue(this);
    }

    // Task가 완전히 끝났을대 실행할 End함수
    public void End()
    {
        // event reset
        onStateChaged = null;
        onSuccessChaged = null;
    }

    // 외부에서 CurrentSuccess값을 조작할 수 있는 함수
    public void ReceiveReport(int successCount)
    {
        // Logic을 실행한 결과값을 반환해 줌
        // 첫번째 인자값이 Task인 이유는 이런 Module이나 event들은 실행한 주체가 누구인지 전달해주는게 여러모로 편리
        CurrentSuccess = action.Run(this, CurrentSuccess, successCount);
    }

    // Task를 즉시 완료할 수 있는 Complete함수
    public void Complete()
    {
        CurrentSuccess = needSuccessToComplete;
    }

    // TaskTarget을 통해, 이 Task가 성공 횟수를 보고 받을 대상인지 확인하는 함수
    public bool IsTarget(string category, object target)
    => Category == category &&
        targets.Any(x => x.IsEqual(target)) && // 하나라도 조건에 맞는 요소가 있는지 확인
        (!isComplete || (isComplete && canReceiveRportsDuringCompletion));
}
