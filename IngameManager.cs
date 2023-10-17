using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IngameManager : MonoBehaviour
{
    static IngameManager _uniqueInstance;           //IngameManager를 어디서든 쉽게 참조할 수 있게
    public static IngameManager _instance       //프로퍼티로 생성
    {
        get { return _uniqueInstance; }
    }
    //참조 변수
    Generators _insectGenerator;
    MessageBox _msgBox;
    TimerBox _timerBox;
    CountBox _countBox;
    ItemCountBox _itemCountBox;
    [SerializeField] RectTransform _ClockItemSpawnPos;
    [SerializeField] Image _itemDelayImg;

    //정보 변수
    DefineHelper.eSelectItemKind _itemType;
    DefineHelper.eIngameState _currnetState;                        //게임의 현재 상태를 담은 열거형 변수
     Dictionary<DefineHelper.eInsectKind, int> _killCounts;          //벌레의 종류별 벌레를 죽인 수
    float _passTime = 0;                    //게임시작전 카운트를 체크하기 위한 시간
    int _startCount = 3;                    //게임시작 카운트다운 할 시간
    float _limitPlayTime = 0;        //플레이 제한 시간
    public float LimitTime
    {
        get { return _limitPlayTime; }
        set { _limitPlayTime = value; }
    }
    
    //=====
    bool _isClickItemUse = false;
    int _itemUseCount = 0;
    public int ItemUseCount
    {
        get { return _itemUseCount; }
    }
    float _useItemRate = 2;
    float _CheckRateTimer = 0;
    //=====

    float _endDelayTime = 2;
    [HideInInspector] public int highestScore;

    DefineHelper.eInsectKind[] _genderInsects;
    int[] _genRateValues;
    //임시
    //DefineHelper.eInsectKind[] k = { DefineHelper.eInsectKind.GreenInsect,
    //                                        DefineHelper.eInsectKind.RedAnt };          //벌레의 종류들을 가지고 있는 배열 변수
    //int[] r = { 75, 100 };                      //종류별 생성되는 확률을 담은 배열 변수 (벌레 종류의 순서와 이 배열의 순서가 연결된다.)
    //==
   public DefineHelper.eSelectItemKind ItemType
    {
        get { return _itemType; }
    }

    private void Awake()
    {
        _uniqueInstance = this;
        _killCounts = new Dictionary<DefineHelper.eInsectKind, int>();
        
    }

    private void Update()
    {
        switch(_currnetState)       //현재 게임의 상태에 따라 수행할 작업들을 나눈다.
        {
            case DefineHelper.eIngameState.COUNT:
                _passTime += Time.deltaTime;
                _msgBox.SetCounting((int)_passTime);        //시작카운트시작 함수 호출 (_passTime을 int형으로 반환하여 1전까지는 0을 반환
                if(_startCount - _passTime <= -1)           //시작 카운트가 다되면 게임시작 함수 호출
                {
                    PlayGame();
                }
                break;
            case DefineHelper.eIngameState.PLAY:
                _limitPlayTime -= Time.deltaTime;           //제한시간이 계속 줄어든다.
                if(_limitPlayTime <= 0)             //제한시간이 다되면 EndGame 함수 호출
                {
                    EndGame();
                }
                _timerBox.SettingTimer(_limitPlayTime);         //제한시간 카운트에 보내 남은 시간을 텍스트로 보여주게 한다.
                break;
            case DefineHelper.eIngameState.END:
                _passTime += Time.deltaTime;                //0으로 초기화한 _passTime에 게임이 끝난후 지난 시간을 가지게 한다.
                if (_passTime >= _endDelayTime)             //endDelayTime만큼 시간이 지나면 Result창을 띄어준다.
                {
                    ResultGame();
                }
                break;
        }
        _CheckRateTimer += Time.deltaTime;
        _itemDelayImg.fillAmount = (_CheckRateTimer / _useItemRate);
        if (_CheckRateTimer >= _useItemRate)
        {
            if (!_isClickItemUse)
            {
                if (Input.GetKeyDown(KeyCode.Z))
                {
                    if (ItemType == DefineHelper.eSelectItemKind.Bomb)
                        _isClickItemUse = true;
                    else
                        UseItem(ItemType);
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Vector2 mousePos = Input.mousePosition;
                    mousePos = Camera.main.ScreenToWorldPoint(mousePos);
                    UseItem(DefineHelper.eSelectItemKind.Bomb, mousePos);
                    _isClickItemUse = false;
                }
            }
        }
    }

    #region [State 함수들]
    public void InitializeSettings(DefineHelper.stStageInfo stageInfo, DefineHelper.eSelectItemKind ItemType)
    {
#if UNITY_EDITOR
        _limitPlayTime = 15;
#else
        _limitPlayTime = stageInfo._limitTime;
#endif
        _genderInsects = stageInfo._insectKinds;
        _genRateValues = stageInfo._insectGenRate;
        _itemType = ItemType;
        GameObject go = null;           //외부 오브젝트들을 가져와 담아놓은 변수를 만들어 놓는다.
        _insectGenerator = GetComponent<Generators>();
        go = GameObject.FindGameObjectWithTag("UIMessageBox");
        _msgBox = go.GetComponent<MessageBox>();
        go = GameObject.FindGameObjectWithTag("UITimerBox");
        _timerBox = go.GetComponent<TimerBox>();
        go = GameObject.FindGameObjectWithTag("UICountBox");
        _countBox = go.GetComponent<CountBox>();
        go = GameObject.FindGameObjectWithTag("UIItemCountBox");
        _itemCountBox = go.GetComponent<ItemCountBox>();

        _msgBox.CloseMessageBox();                   //게임시작 초기에는 MessageBox를 다 꺼놓는다.
        _timerBox.InitSetData(_limitPlayTime);      //timerBox에 제한시간을 알려준다.
        _countBox.InitSetData(_genderInsects);                   //CountBox에 카운트해야할 벌레종류의 수를 알려주기 위해 k를 넘겨 보낸다.
        _itemCountBox.InitSetData(_itemType);
        //임시
        StartGameCount(_startCount);        //게임시작카운트를 시작하는 함수호출
        //==

    }


    void StartGameCount(int count)          //게임시작 카운트를 하는 함수
    {
        _currnetState = DefineHelper.eIngameState.COUNT;        //현재 State에 COUNT상태를 넣어준다. 게임시작 초기에 한번 설정

        _msgBox.OpenMessageBox("Game Start!", DefineHelper.eMessageBoxKind.COUNTER, count);         //메시지의 종류를 COUNTER로 해서 카운트가 시작되게 한다.
        for(int n = 0; n < _genderInsects.Length; n++)       //벌레의 종류만큼
        {
            _killCounts.Add(_genderInsects[n], 0);       //벌레의 종류 만큼 킬카운트 Add
        }
    }

    void PlayGame()         //게임시작할때 호출하는 함수
    {
        _currnetState = DefineHelper.eIngameState.PLAY;     //게임의 현재 상태를 PLAY로 변경

        _msgBox.CloseMessageBox();              //게임시작할 때 메시지박스 꺼준다.
        
        _insectGenerator.StartInsectGenerate(_genderInsects, _genRateValues);         //벌레들을 생성하는 함수를 호출해 벌레 생성을 시작한다.    (벌레 종류와 종류별 생성 확률을 Generator에 알려준다.)
    }

    void EndGame()              //게임이 종료될 때 호출되는 함수
    {
        _currnetState = DefineHelper.eIngameState.END;          //게임상태를 END로 지정

        _passTime = 0;          //게임 끝난후 딜레이 계산을 위해 0으로 초기화
        _msgBox.OpenMessageBox("Time Over");            //메시지박스종류는 입력안하면 Message기 때문에 출력할 메시지만 입력하면 됀다.
        _insectGenerator.EndInsectGenerate();
        //UserInfoManager._Instance._clearStage = 
        //    (UserInfoManager._Instance._clearStage < UserInfoManager._Instance._nowStageNumber)
        //    ? ++UserInfoManager._Instance._clearStage : UserInfoManager._Instance._clearStage;
    }

    void ResultGame()
    {
        _currnetState = DefineHelper.eIngameState.RESULT;
        
        _msgBox.CloseMessageBox();
        //종료창을 생성
        GameObject go = Instantiate(ResoucePoolManager._instance.GetUIPrefabFromType(DefineHelper.eUIWindowType.ResultWnd));
        ResultWnd wnd = go.GetComponent<ResultWnd>();
        wnd.OpenResultWindow(_killCounts,ItemType, _itemUseCount);
    }
#endregion
    

    public void AddKillCount(DefineHelper.eInsectKind kind)     //킬 카운트를 더해주는 함수
    {
        _killCounts[kind]++;        //딕셔너리 Key값을 이용해 Value를 더해준다.
        //화면에 출력...
        _countBox.SetCount(kind, _killCounts[kind]);        //화면에 출력할 카운트를 
    }

    void UseItem(DefineHelper.eSelectItemKind type, Vector2 mousePosition = default)
    {
        if (type == DefineHelper.eSelectItemKind.NoItem) return;
        GameObject go = null;
        if (type == DefineHelper.eSelectItemKind.Bomb)
        {
            go = Instantiate(ResoucePoolManager._instance.GetItemPrefabFromType(type), mousePosition, Quaternion.identity);
        }
        else
        {
            go = Instantiate(ResoucePoolManager._instance.GetItemPrefabFromType(type), _ClockItemSpawnPos);
        }

        Item _item = go.GetComponent<Item>();
        _item.InitializeData(type);
        _itemUseCount++;
        _itemCountBox.UseCountUp(_itemUseCount);
        _CheckRateTimer = 0;
        _itemDelayImg.fillAmount = 0;
    }
}
