using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

// Quest의 보관과 탐색을 위한 데이터베이스
[CreateAssetMenu(menuName = "Quest/QuestDatabase")]
public class QuestDataBase : ScriptableObject
{
    [SerializeField]
    private List<Quest> quests;

    public IReadOnlyList<Quest> Quests => quests;

    // codeName을 통해 Quest를 찾아오는 함수
    // 참고 블로그 https://afsdzvcx123.tistory.com/entry/C-%EB%AC%B8%EB%B2%95-C-LINQ-First-FirstOrDefault-Single-SingleOrDefault-%EC%B0%A8%EC%9D%B4%EC%A0%90
    public Quest FindQuestBy(string codeName) => quests.FirstOrDefault(x => x.CodeName == codeName); // .FirstOrDefault()은 조건을 만족하는 첫번째 요소 반환, 없다면 default값 반환

#if UNITY_EDITOR
    // AssetDatabase 클래스는 Unity Editor상에서 모든 에셋 관련 파일 처리를 할때 사용하는 클래스로 기본 파일 입출력부터 Material, Prefab, AnimationClip, CustomAsset등 파일을 만들고 제어할 수 있음
    // 응용으로 프로젝트 파일 내의 모든 씬 파일을 찾아서 수정을 하거나, 현재 씬에 존재하는 오브젝트들을 자동으로 프리팹으로 만들어 주는 기능을 만들 수 있다.
    // But AssetDatabase 클래스트 Editor 상에서만 작용하는 코드이기 때문에 #if UNITY_EDITOR 조건 컴파일을 걸어야 빌드할때 에러나지 않음
    // 참고 블로그 https://darkcatgame.tistory.com/95


    // ContextMenu 만들기
    // ConTextMenu란 인스펙터창에서 함수를 실행할 수 있게 만들어줌
    // 참고 블로그 https://shhouse.tistory.com/7?category=452932
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


    // Generic 함수인 이유는 같은 Code로 Quest와 Achievement를 따로 찾기 위해서 이다.
    private void FindQuestBy<T>() where T : Quest
    {
        quests = new List<Quest>();

        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T)}"); // FideAsset함수는 Asset폴더에서 Filter에 맞는 Asset의 GUID를 가져오는 함수이다.
                                                                     // GUID라는것은 Unity가 Asset을 관리하기 위해서 내부적으로 사용하는 ID이다.
        
        foreach(var guid in guids)
        {
            // GUID를 이용해 Asset이 저장되어 있는 경로를 가져옴
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var quest = AssetDatabase.LoadAssetAtPath<T>(assetPath);

            // if문으로 Type Check를 해주는 이유는
            // 만약에 T가 Quest 클래스라면 Achievement 클래스가 Quest 클래스를 상속받고 있기 때문에
            // FindAssets에서 Quest객체와 Achievement 객체를 다 찾아와버림
            // 그래서 한번더 객체의 Type이 T와 같은지 확인해줘야함
            if (quest.GetType() == typeof(T ))
            {
                quests.Add(quest);
            }

            // SetDirty라는건 QuestDatabase객체가 가진 Serialize 변수가 변화가 생겼으니 
            // Asset을 저장할 때 반영하라는 의미
            EditorUtility.SetDirty(this);

            // Asset 저장
            // 저장을 하지 않는다면 이 기능을 이용해서 List에 Quest를 채워넣어도 Editor를 껏다 키면 초기화됨
            AssetDatabase.SaveAssets();
        }
    }
#endif
}