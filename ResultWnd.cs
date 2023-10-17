using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ResultWnd : MonoBehaviour
{
    [SerializeField] RectTransform _contant;
    [SerializeField] Text _txtTotalScore;

    float _baseSize = 120;              //ScoreCounting의 Y 사이즈 값
    float _startPos = 10;               //
    List<ScoreCounting> _perScores;     //스코어카운팅 만들걸 저장
    int _totalScore = 0;
    float _drawScore = 0;
    float _countingTime = 2;
    DefineHelper.eResultCounting _state;    //현재 상태

    int _useItemScore = 0;



    //private void Start()
    //{
    //    //임시
    //    Dictionary<DefineHelper.eInsectKind, int> scoreData = new Dictionary<DefineHelper.eInsectKind, int>();
    //    scoreData.Add(DefineHelper.eInsectKind.RedAnt, 102);
    //    scoreData.Add(DefineHelper.eInsectKind.GreenInsect, 219);
    //    OpenResultWindow(scoreData);
    //    //===
    //}
    private void LateUpdate()
    {
        switch (_state)
        {
            case DefineHelper.eResultCounting.IndividualScore:
                _state = DefineHelper.eResultCounting.TotalScore;       //토탈스코어로 넘아가게 함
                for (int n = 0; n < _perScores.Count; n++)        //토탈스코어로 넘어가기전 스코어카운팅들을 모두 끝났는지 검사
                {
                    if (!_perScores[n]._endCounting)         //아직 카운팅이 끝나지않은 스코어카운팅은 여기서 토탈스코어로 넘어가지않게 한다.
                    {
                        _state = DefineHelper.eResultCounting.IndividualScore;
                    }
                }
                break;
            case DefineHelper.eResultCounting.TotalScore:
                if(_totalScore <= _drawScore)       //드로우스코어가 총점보다 크거나 같다면.
                {
                    _txtTotalScore.text = string.Format("{0:#,###}", _totalScore);
                    _state = DefineHelper.eResultCounting.Complete;
                }
                else                //드로우스코어가 아직 총점보다 작다면 드로우스코어를 더 올려준다.
                {
                    _drawScore += _totalScore * (Time.deltaTime / _countingTime);           //Time.deltaTime을 카운트하고 싶은 시간으로 나눈값과 총점을 곱하면 원하는 시간동안 스코어를 올릴 수 있다.
                    _txtTotalScore.text = string.Format("{0:#,###}", (int)_drawScore);      //0 = 첫번째 인자를 의미? #,###은 데이터를 나타낼 형식지정?
                }
                break;
            
        }

    }
    public void OpenResultWindow(Dictionary<DefineHelper.eInsectKind, int> scoreData, DefineHelper.eSelectItemKind itemKind,int itemUsseCnt)
    {
        int cnt = 0;
        _perScores = new List<ScoreCounting>();
        _contant.sizeDelta = new Vector2(_contant.sizeDelta.x, _startPos + (_baseSize * (scoreData.Count  + 1)));      //contant 사이즈 조절.   RectTransform.sizeDelta = RectTransform의 width와 height를 수정할 수 있다.
        GameObject props = ResoucePoolManager._instance.GetUIPropsPrefabFromType(DefineHelper.eUIPropsType.ResultProps);
        foreach(DefineHelper.eInsectKind key in scoreData.Keys)     //벌레종류만큼
        {
            GameObject go = Instantiate(props, _contant);
            RectTransform rtf = go.GetComponent<RectTransform>();
            rtf.anchoredPosition = new Vector2(0, -_startPos - (_baseSize * cnt++));
            ScoreCounting scg = go.GetComponent<ScoreCounting>();
            scg.InitDataSet(key, scoreData[key],this);          //(벌레의 종류, 벌레의 Value값, this)
            _perScores.Add(scg);                    //만들걸 리스트에 저장

            _totalScore += scg._calceScore;         //모든 점수들을 다 _totalScore에 저장
        }
        GameObject itemProp = Instantiate(props, _contant);
        RectTransform rectTrans = itemProp.GetComponent<RectTransform>();
        rectTrans.anchoredPosition = new Vector2(0, -_startPos - (_baseSize * cnt++));
        ScoreCounting sc = itemProp.GetComponent<ScoreCounting>();
        sc.InitDataSet(itemKind, itemUsseCnt, this);
        _perScores.Add(sc);
        _totalScore += sc._calceScore;




        _txtTotalScore.text = "0";              //토탈스코어 텍스트 시작전 0으로 초기화
        _state = DefineHelper.eResultCounting.IndividualScore;



        ResoucePoolManager._instance.CheckStageClearScore(IngameManager._instance.highestScore,_totalScore);
        if(UserInfoManager._Instance.gameData.clearStage < UserInfoManager._Instance._nowStageNumber)
        {
            ResoucePoolManager._instance.CheckOpenNextStage();
        }
    }
    
    public void CommonDownButton(RectTransform rtf)
    {
        rtf.anchoredPosition = new Vector2(rtf.anchoredPosition.x, rtf.anchoredPosition.y - 25);
    }
    public void CommonUpButton(RectTransform rtf)
    {
        rtf.anchoredPosition = new Vector2(rtf.anchoredPosition.x, rtf.anchoredPosition.y + 25);
    }

    public void ClickHomeButton(string ex)
    {
        SceneControlManager._instance.StartMainScene();
    }
    public void ClickRegameButton()
    {
        //SceneManager.LoadScene("IngameScene");
        //Destroy(gameObject);
        SceneControlManager._instance.StartIngameScene(IngameManager._instance.ItemType);
    }
    void ClickQuitButton()
    {
        //Debug.Log("나가기버튼 클릭했어요~~");
#if UNITY_EDITOR            //전처리기 = 컴파일할때 걸러질지 말지 검사?체크?
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    //public void CheckCounting()
    //{
    //    _state = DefineHelper.eResultCounting.TotalScore;
    //    for (int n = 0; n < _perScores.Count; n++)        //토탈스코어로 넘어가기전 스코어카운팅들을 모두 끝났는지 검사
    //    {
    //        if (!_perScores[n]._endCounting)         //아직 카운팅이 끝나지않은 스코어카운팅은 여기서 토탈스코어로 넘어가지않게 한다.
    //        {
    //            _state = DefineHelper.eResultCounting.IndividualScore;
    //        }
    //    }
    //}

}
