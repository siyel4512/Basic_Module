using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// Quest System �� Quest���� �����ϰ� ������
// �������� Quest�� �Ϸ��� Quest�� ������� �����ϰ�
// Quest ��� �� ����, ����� ���� �������� ������ �����
public class QuestSystem : MonoBehaviour
{
    #region Events
    public delegate void QuestRegisteredHandler(Quest newQuest);
    public delegate void QuestCompletedHandler(Quest quest);
    public delegate void QuestCanceledHandler(Quest quest);
    #endregion

    // Singleton
    private static QuestSystem instance;
    private static bool isApplicationQuitting;

    public static QuestSystem Instance
    {
        get
        {
            if (!isApplicationQuitting && instance == null)
            {
                // ������Ʈ ã��
                // ���� ��α� https://rucira-tte.tistory.com/115
                instance = FindObjectOfType<QuestSystem>();
                
                if (instance == null)
                {
                    // ã�Ƶ� ���ٸ� ���� ������Ʈ ����
                    instance = new GameObject("Quest System").AddComponent<QuestSystem>();
                    DontDestroyOnLoad(instance.gameObject);
                }
            }
            return instance;
        }
    }

    // Quest���� ����� List
    private List<Quest> activeQuests = new List<Quest>();
    private List<Quest> completeQuests = new List<Quest>();

    private List<Quest> activeAchievements = new List<Quest>();
    private List<Quest> completeAchievement = new List<Quest>();

    private QuestDataBase guestDatabase;
    private QuestDataBase achievementDatabase;

    public event QuestRegisteredHandler onQuestRegistered;
    public event QuestCompletedHandler onQuestCompleted;
    public event QuestCanceledHandler onQuestCanceled;

    public event QuestRegisteredHandler onAchievementRegistered;
    public event QuestCompletedHandler onAchievementCompleted;

    public IReadOnlyList<Quest> ActiveQuests => activeQuests;
    public IReadOnlyList<Quest> CompleteQuests => completeQuests;

    public IReadOnlyList<Quest> ActiveAchievementQuests => activeAchievements;
    public IReadOnlyList<Quest> CompleteAchievementQuests => completeAchievement;

    private void Awake()
    {
        guestDatabase = Resources.Load<QuestDataBase>("Quest Database");
        achievementDatabase = Resources.Load<QuestDataBase>("AchievementDatabase");

        // Database�� �ִ� �������� ���
        foreach (var aachievement in achievementDatabase.Quests)
        {
            Debug.Log("ddd");
            Register(aachievement);
        }
    }

    // Quest�� System�� ����ϴ� �Լ�
    // �ش� �Լ��� ���� Quest�� ��� ������ ���� activeQuest Ȥ�� activeAchievement List�� ��
    public Quest Register(Quest quest)
    {
        var newQuest = quest.Clone();

        // newQuest Ÿ���� Achievement�� ���
        if (newQuest is Achievement)
        {
            newQuest.onCompleted += OnAchievementCompleted;

            activeAchievements.Add(newQuest);

            newQuest.OnRegister();
            onAchievementRegistered?.Invoke(newQuest);
        }
        else
        {
            newQuest.onCompleted += OnQuestCompleted;
            newQuest.onCanceled += OnQuestCanceled;

            activeQuests.Add(newQuest);

            newQuest.OnRegister();
            onQuestRegistered?.Invoke(newQuest);
        }

        return newQuest;
    }

    // Receive Report �Լ� (�ܺο�)
    public void ReceiveReport(string category, object target, int successCount)
    {
        ReceiveReport(activeQuests, category, target, successCount);
        ReceiveReport(activeAchievements, category, target, successCount);
    }

    // ���Ǽ��� ���� �������̵�
    public void ReceiveReport(Category category, TaskTarget target, int successCount)
        => ReceiveReport(category.CodeName, target.Value, successCount);

    // Receive Report �Լ� (���ο�)
    private void ReceiveReport(List<Quest> quests, string category, object target, int successCount)
    {
        // for���� ����ϴ� ������
        // for���� ���ư��� ���߿� Quest�� Complete�Ǿ� ��Ͽ��� ���� ���� �ֱ� ����
        // ������ �߻��� �� �ֱ� ������ ToArray�� ������ �ƴ� �纻���
        foreach (var quest in quests.ToArray())
        {
            quest.ReceiveReport(category, target, successCount);
        }
    }

    // Quest�� ��Ͽ� �ִ��� Ȯ���ϴ� �Լ���
    public bool ContainsActiveQuest(Quest quest) => activeQuests.Any(x => x.CodeName == quest.CodeName);
    public bool ContainsCompleteQuest(Quest quest) => completeQuests.Any(x => x.CodeName == quest.CodeName);
    public bool ContainsActiveAchievementQuest(Quest quest) => activeAchievements.Any(x => x.CodeName == quest.CodeName);
    public bool ContainsCompleteAchievementQuest(Quest quest) => completeAchievement.Any(x => x.CodeName == quest.CodeName);

    #region Callback
    // �� OnQuestCompleted(), OnQuestCanceled() �� �ݹ��� Quest event�� ������ָ�
    // Quest�� �Ϸ�ǰų� ��ҵǸ� ���� System���� Ȯ���� ������ �ʾƵ� �˾Ƽ� ��Ͽ��� ���� �߰��ϴ� �۾��� �̷����
    private void OnQuestCompleted(Quest quest)
    {
        activeQuests.Remove(quest);
        completeQuests.Add(quest);

        onQuestCompleted?.Invoke(quest);
    }

    private void OnQuestCanceled(Quest quest)
    {
        activeQuests.Remove(quest);
        onQuestCanceled?.Invoke(quest);

        Destroy(quest, Time.deltaTime);
    }

    public void OnAchievementCompleted(Quest achievement)
    {
        activeAchievements.Remove(achievement);
        completeAchievement.Add(achievement);

        onAchievementCompleted?.Invoke(achievement);
    }
    #endregion
}
