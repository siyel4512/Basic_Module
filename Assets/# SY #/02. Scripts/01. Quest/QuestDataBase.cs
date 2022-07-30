using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

// Quest�� ������ Ž���� ���� �����ͺ��̽�
[CreateAssetMenu(menuName = "Quest/QuestDatabase")]
public class QuestDataBase : ScriptableObject
{
    [SerializeField]
    private List<Quest> quests;

    public IReadOnlyList<Quest> Quests => quests;

    // codeName�� ���� Quest�� ã�ƿ��� �Լ�
    // ���� ��α� https://afsdzvcx123.tistory.com/entry/C-%EB%AC%B8%EB%B2%95-C-LINQ-First-FirstOrDefault-Single-SingleOrDefault-%EC%B0%A8%EC%9D%B4%EC%A0%90
    public Quest FindQuestBy(string codeName) => quests.FirstOrDefault(x => x.CodeName == codeName); // .FirstOrDefault()�� ������ �����ϴ� ù��° ��� ��ȯ, ���ٸ� default�� ��ȯ

#if UNITY_EDITOR
    // AssetDatabase Ŭ������ Unity Editor�󿡼� ��� ���� ���� ���� ó���� �Ҷ� ����ϴ� Ŭ������ �⺻ ���� ����º��� Material, Prefab, AnimationClip, CustomAsset�� ������ ����� ������ �� ����
    // �������� ������Ʈ ���� ���� ��� �� ������ ã�Ƽ� ������ �ϰų�, ���� ���� �����ϴ� ������Ʈ���� �ڵ����� ���������� ����� �ִ� ����� ���� �� �ִ�.
    // But AssetDatabase Ŭ����Ʈ Editor �󿡼��� �ۿ��ϴ� �ڵ��̱� ������ #if UNITY_EDITOR ���� �������� �ɾ�� �����Ҷ� �������� ����
    // ���� ��α� https://darkcatgame.tistory.com/95


    // ContextMenu �����
    // ConTextMenu�� �ν�����â���� �Լ��� ������ �� �ְ� �������
    // ���� ��α� https://shhouse.tistory.com/7?category=452932
    [ContextMenu("FindQuests")]
    private void FindQuests()
    {
        FindQuestBy<Quest>();
    }

    [ContextMenu("FindAchievement")]
    private void FindAchievement()
    {
        FindQuestBy<Achievement>();
    }


    // Generic �Լ��� ������ ���� Code�� Quest�� Achievement�� ���� ã�� ���ؼ� �̴�.
    private void FindQuestBy<T>() where T : Quest
    {
        quests = new List<Quest>();

        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T)}"); // FideAsset�Լ��� Asset�������� Filter�� �´� Asset�� GUID�� �������� �Լ��̴�.
                                                                     // GUID��°��� Unity�� Asset�� �����ϱ� ���ؼ� ���������� ����ϴ� ID�̴�.
        
        foreach(var guid in guids)
        {
            // GUID�� �̿��� Asset�� ����Ǿ� �ִ� ��θ� ������
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var quest = AssetDatabase.LoadAssetAtPath<T>(assetPath);

            // if������ Type Check�� ���ִ� ������
            // ���࿡ T�� Quest Ŭ������� Achievement Ŭ������ Quest Ŭ������ ��ӹް� �ֱ� ������
            // FindAssets���� Quest��ü�� Achievement ��ü�� �� ã�ƿ͹���
            // �׷��� �ѹ��� ��ü�� Type�� T�� ������ Ȯ���������
            if (quest.GetType() == typeof(T ))
            {
                quests.Add(quest);
            }

            // SetDirty��°� QuestDatabase��ü�� ���� Serialize ������ ��ȭ�� �������� 
            // Asset�� ������ �� �ݿ��϶�� �ǹ�
            EditorUtility.SetDirty(this);

            // Asset ����
            // ������ ���� �ʴ´ٸ� �� ����� �̿��ؼ� List�� Quest�� ä���־ Editor�� ���� Ű�� �ʱ�ȭ��
            AssetDatabase.SaveAssets();
        }
    }
#endif
}