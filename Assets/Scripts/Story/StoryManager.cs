using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class StoryManager : MonoBehaviour
{
    [SerializeField] List<CharacterMotion> characters;
    // [SerializedField] List<> events; // 추후 CSV에서 파생해온 데이터

    IEnumerator Start()
    {
        characters[0].CharacterWalk(new Vector2(0, 0), new Vector2(3, 3), 3f);
        yield return new WaitForSeconds(3.1f);
        characters[0].CharacterLookAt(new Vector2(0, -1));
    }

    // void Start()
    // {
    //     StartCoroutine(PlayStory());
    // }

    // CSV를 파싱할 때 charID,type(eventType), startPos, endPos, direction, duration, background
    // 대사는 별도 파싱

    // IEnumerator PlayStory()
    // {
    //     foreach (var evt int2 events){
    //         switch (evt.type)
    //         {
    //             case "MOVE":
    //                 characters[evt.charID].CharacterWalk(evt.startPos, evt.endPos, evt.duration);
    //                 yield return new WaitForSeconds(evt.duration + 0.2f);
    //                 break;

    //             case "LOOK":
    //                 characters[evt.charID].CharacterLookAt(evt.direction);
    //                 break;

    //             case "WAIT":
    //                 yield return new WaitForSeconds(evt.duration);
    //                 break;

    //             case "DIALOGUE":
    //                 // 대화 재생 로직 추가해주세요!
    //                 break;
    //         }

    //         yield return new WaitForSeconds(0.05f);
    //     }
    // }

    // 캐릭터 아이디에 맞춰서 캐릭터 배열 반환하는 함수 필요함.
}
