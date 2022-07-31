using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// Quest System 은 Quest들을 관리하고 제어함
// 진행중인 Quest와 완료한 Quest를 목록으로 관리하고
// Quest 등록 및 보고, 저장과 같은 전반적인 역할을 담당함
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
                // 오브젝트 찾기
                // 참고 블로그 https://rucira-tte.tistory.com/115
                instance = FindObjectOfType<QuestSystem>();
                
                if (instance == null)
                {
                    // 찾아도 없다면 게임 오브젝트 생성
                    instance = new GameObject("Quest System").AddComponent<QuestSystem>();
                    DontDestroyOnLoad(instance.gameObject);
                }
            }
            return instance;
        }
    }

    // Quest들을 담아줄 List
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

        // Database에 있는 업적들을 등록
        foreach (var aachievement in achievementDatabase.Quests)
        {
            Debug.Log("ddd");
            Register(aachievement);
        }
    }

    // Quest를 System에 등록하는 함수
    // 해당 함수를 통해 Quest는 등록 과정을 거쳐 activeQuest 혹은 activeAchievement List에 들어감
    public Quest Register(Quest quest)
    {
        var newQuest = quest.Clone();

        // newQuest 타입이 Achievement일 경우
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

    // Receive Report 함수 (외부용)
    public void ReceiveReport(string category, object target, int successCount)
    {
        ReceiveReport(activeQuests, category, target, successCount);
        ReceiveReport(activeAchievements, category, target, successCount);
    }

    // 편의성을 위해 오버라이드
    public void ReceiveReport(Category category, TaskTarget target, int successCount)
        => ReceiveReport(category.CodeName, target.Value, successCount);

    // Receive Report 함수 (내부용)
    private void ReceiveReport(List<Quest> quests, string category, object target, int successCount)
    {
        // for문을 사용하는 이유는
        // for문이 돌아가는 와중에 Quest가 Complete되어 목록에서 빠질 수도 있기 때문
        // 에러가 발생할 수 있기 때문에 ToArray로 원본이 아닌 사본사용
        foreach (var quest in quests.ToArray())
        {
            quest.ReceiveReport(category, target, successCount);
        }
    }

    // Quest가 목록에 있는지 확인하는 함수들
    public bool ContainsActiveQuest(Quest quest) => activeQuests.Any(x => x.CodeName == quest.CodeName);
    public bool ContainsCompleteQuest(Quest quest) => completeQuests.Any(x => x.CodeName == quest.CodeName);
    public bool ContainsActiveAchievementQuest(Quest quest) => activeAchievements.Any(x => x.CodeName == quest.CodeName);
    public bool ContainsCompleteAchievementQuest(Quest quest) => completeAchievement.Any(x => x.CodeName == quest.CodeName);

    #region Callback
    // 두 OnQuestCompleted(), OnQuestCanceled() 두 콜백은 Quest event에 등록해주면
    // Quest가 완료되거나 취소되면 따로 System에서 확인을 해주지 않아도 알아서 목록에서 빼고 추가하는 작업이 이루어짐
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
