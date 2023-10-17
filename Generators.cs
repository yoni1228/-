using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generators : MonoBehaviour
{
    //상하좌우에서 2마리씩
    [SerializeField] GameObject[] _insects;                                     //Insect Prefab들을 담아 놓은 배열변수
    [SerializeField] float _spawndelayStartTime = 0.5f;                 //게임시작 후 벌레가 생성되기까지의 시간
    [SerializeField] float _spawnTime = 5;                                      //벌레생성이 반복될 시간
    [SerializeField] int _perCount = 2;                                            //한쪽 방향당 생성될 벌레 수?

    //참조 변수
    List<InsectControl> _genInsects;                                                //생성된 InsectObject들을 담아둘 List 변수
    DefineHelper.eInsectKind[] _insectKinds;                                    //Insect들의 종류를 담아둔 변수 { GreenInsect,   RedAnt }
    int[] _genPerRate;                                                                           //Insect의 종류별로 생성될 확률 - 첫번째 인덱스는 _insectKinds 의 첫번째 종류
    Transform _rootPool;                                                                     //생성된 Insect_Object들이 들어갈 Parent_Object의 위치

    //정보 변수
    float _checkListTime = 1;                                                             //NULL이 된 Insect Object를 List에서 지워줄 시간 [1초마다]


    private void Awake()
    {
        _genInsects = new List<InsectControl>();                                //List를 객체화?
    }
    void Start()
    {
        GameObject go = GameObject.FindGameObjectWithTag("InsectsPool");                //InsectsPool 오브젝트를 찾아서 가져온다.
        if( go != null)
        {
            _rootPool = go.transform;
        }
        else
        {
            Debug.Log("InsectsPool 게임 오브젝트를 찾지 못했습니다.");
        }
    }

    void LateUpdate()
    {
        
    }

    //벌레들을 생성시키는 함수
    void GenerateInsects()
    {
        //화면 크기구하는 법
        float ScreenHalfH = Camera.main.orthographicSize;                               //Camera.main.orthographicSize = 화면 높이의 절반 값
        float ScreenHalfW = ScreenHalfH * Camera.main.aspect;                       //Camera.main.aspect = width / height , (화면 비율) ex) 16 : 9 = "1.77"

        Vector3 Pos = new Vector3();                                //Insect가 생성될 위치를 담을 변수
        float angle = 0.0f;                                                    //생성된 Insect가 화면중앙을 바라보게 회전시킬 회전 값
        int cnt = _perCount * 4;                                        //생성할 벌레의 총 수
        for(int n = 0; n < cnt;n++)        //생성할 수만큼 반복
        {
            switch (n % 4)              //0,4 = 0            1,5 = 1         2,6 = 2        3,7 = 3
            {
                case 0: //좌측
                    Pos.x = -ScreenHalfW;           //화면 중앙 = 0, -이기때문에 좌측
                    Pos.y = Random.Range(-ScreenHalfH, ScreenHalfH);
                    angle = -90.0f;         //z축 회전
                    break;
                case 1: //우측
                    Pos.x = ScreenHalfW;
                    Pos.y = Random.Range(-ScreenHalfH, ScreenHalfH);
                    angle = 90.0f;
                    break;
                case 2: //상단
                    Pos.x = Random.Range(-ScreenHalfW, ScreenHalfW);
                    Pos.y = ScreenHalfH;
                    angle = 180.0f;
                    break;
                case 3: //하단
                    Pos.x = Random.Range(-ScreenHalfW, ScreenHalfW);
                    Pos.y = -ScreenHalfH;
                    angle = 0f;
                    break;
            }
            //반복문 안의 if문에서 rate를 계속 비교해서 맞는 걸 찾으면 만들고 빠진다..

            //GameObject go = Instantiate(_insects[(int)DefineHelper.eInsectKind.GreenInsect]);
            //int rd = Random.Range(0, _insectKinds.Length);
            
            int rand = Random.Range(0, 100);            //0~100사이의 랜덤한 값
            for(int m = 0; m < _insectKinds.Length;m++)         //Insects의 종류의 수만큼 반복
            {
                if(rand < _genPerRate[m])           //랜덤값이 첫번째벌레가 생성될 확률보다 적을 경우
                {
                    GameObject go = Instantiate(_insects[(int)_insectKinds[m]], _rootPool);         //_insectKinds[m] = DefineHelper.eInsectKind 값, 벌레종류의 int값을 넣어 Prefab[m]번째 생성
                    go.transform.position = Pos;
                    go.transform.Rotate(0, 0, angle);
                    InsectControl insect = go.GetComponent<InsectControl>();
                    Debug.Log(m);
                    insect.InitializeData(_insectKinds[m]);                 //DefineHelper.eInsectKind 종류를 보냄
                    _genInsects.Add((insect));                                  //생성된 Object를 객체 List에 추가
                    break;
                }
            }
        }
    }

    //InsectList를 주기적으로 정리해주는 함수
    void CheckInsectList()          
    {
        _genInsects.Remove(null);           //null값이 된것을 List에서 삭제
    }

    //Insect 생성 시작 함수,  IngameManager의 PlayGame함수에서 호출..
    public void StartInsectGenerate(DefineHelper.eInsectKind[] kinds, int[] rate)           
    {
        _insectKinds = kinds;           //IngameManager에 임시로 만들어놓은 "Insect 종류" 배열
        _genPerRate = rate;             ////IngameManager에 임시로 만들어놓은 Insect들의 "생성확률"을 담은 배열
        InvokeRepeating("GenerateInsects", _spawndelayStartTime, _spawnTime);           //Insect를 생성하는 함수를 반복 재생
        InvokeRepeating("CheckInsectList", 4, _checkListTime);          //4초후 _checkListTime초마다 null인 Insect를 List에서 지운다.
    }

    //벌레생성을 중단하는 함수
    public void EndInsectGenerate()
    {
        for(int n = 0; n < _genInsects.Count;n++)       //생성되있는 객체만큼 반복
        {
            if(_genInsects[n] != null)
            {
                Destroy(_genInsects[n].gameObject);     //생성되어있는 벌레들을 없애는 코드
            }
        }
        _genInsects.Clear();            //생성된 객체를 모두 삭제한 후, 리스트를 비워준다.   (리스트 내부 요소를 모두 지우는 함수)
        CancelInvoke("GenerateInsects");    //벌레 생성중지
        CancelInvoke("CheckInsectList");    //리스트비우기 중지
    }
}
