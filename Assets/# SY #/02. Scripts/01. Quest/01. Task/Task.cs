using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 추후 생성해야할 데이터이기때문에 ScirptableObject를 상속받아야함
[CreateAssetMenu(menuName = "Quest/Task/Task", fileName = "Task_")]
public class Task : ScriptableObject
{
    [Header("Text")]
    [SerializeField]
    private string codeName; // 외부에서 보여지는 이름이 아닌 프로그래머가 검색과 같은 어떠한 기능을 위해 내부적으로 사용하는 이름. 일종의 ID 같은 개념으로 생각해도 될듯

    [SerializeField]
    private string description; // 해당 Task가 어떤 Task 인지를 알려줄 Description이다.

    [Header("Action")]
    [SerializeField]
    private TaskAction action;

    [Header("Settings")]
    [SerializeField]
    private InitialSuccessValue initialSuccessValue;

    [SerializeField]
    private int needSuccessToComplete; // Task가 성공하기 위해 필요한 성공 횟수를 변수로 만들어 줌 (목표)

    // 프로퍼티
    public int CurrentSuccess { get; private set; } // 현재 성공한 횟수 (프로퍼티)
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

    // 외부에서 CurrentSuccess값을 조작할 수 있는 함수
    public void ReceieveReport(int successCount)
    {
        // Logic을 실행한 결과값을 반환해 줌
        // 첫번째 인자값이 Task인 이유는 이런 Module이나 event들은 실행한 주체가 누구인지 전달해주는게 여러모로 편리
        CurrentSuccess = action.Run(this, CurrentSuccess, successCount);
    }
}
