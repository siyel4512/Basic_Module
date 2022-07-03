using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ���� �����ؾ��� �������̱⶧���� ScirptableObject�� ��ӹ޾ƾ���
[CreateAssetMenu(menuName = "Quest/Task/Task", fileName = "Task_")]
public class Task : ScriptableObject
{
    [Header("Text")]
    [SerializeField]
    private string codeName; // �ܺο��� �������� �̸��� �ƴ� ���α׷��Ӱ� �˻��� ���� ��� ����� ���� ���������� ����ϴ� �̸�. ������ ID ���� �������� �����ص� �ɵ�

    [SerializeField]
    private string description; // �ش� Task�� � Task ������ �˷��� Description�̴�.

    [Header("Action")]
    [SerializeField]
    private TaskAction action;

    [Header("Settings")]
    [SerializeField]
    private InitialSuccessValue initialSuccessValue;

    [SerializeField]
    private int needSuccessToComplete; // Task�� �����ϱ� ���� �ʿ��� ���� Ƚ���� ������ ����� �� (��ǥ)

    // ������Ƽ
    public int CurrentSuccess { get; private set; } // ���� ������ Ƚ�� (������Ƽ)
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

    // �ܺο��� CurrentSuccess���� ������ �� �ִ� �Լ�
    public void ReceieveReport(int successCount)
    {
        // Logic�� ������ ������� ��ȯ�� ��
        // ù��° ���ڰ��� Task�� ������ �̷� Module�̳� event���� ������ ��ü�� �������� �������ִ°� ������� ��
        CurrentSuccess = action.Run(this, CurrentSuccess, successCount);
    }
}
