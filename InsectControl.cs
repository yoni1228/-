using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InsectControl : MonoBehaviour
{
    [SerializeField] float _eraseTime = 1.8f;                       //죽은 벌레가 사라질 시간
    [SerializeField] float _turnDelayTime = 2;                  //벌레가 방향전환을 할 시간
    [SerializeField] float _changeDirStartTime = 5;             //Insect방향전환 InvokeRepeating  시작 시간

    DefineHelper.eInsectKind _kind;                     //이 벌레의 종류가 담긴 열거형 변수
    Animator _aniControl;                                       //Animator 컴포넌트 참조변수
    float _moveSpeed = 0;                                      //벌레 이동속도
    bool _isDead = false;                                       //벌레가 죽었는지 체크
    float _angle = 0;                                               //벌레가 회전할 회전방향
    //float _passTime = 0;

    //임시
    //float time = 0;
    AudioSource audioSource;
    //==
    private void Awake()
    {
        _aniControl = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        InvokeRepeating("ChangeDirection", _changeDirStartTime, _turnDelayTime);        //방향전환하는 함수 반복호출
        IngameManager._instance.highestScore += DefineHelper._baseScorePerInsect[(int)_kind];
    }
    private void Update()
    {
        if(_isDead) return;     //벌레가 죽었다면 Update실행끝
        
        //time += Time.deltaTime;
        //if(time > 0.5f)
        //{
        //    GetComponent<CircleCollider2D>().enabled = false;       //죽은 벌레는 다시 클릭되지않게 Collider를 꺼준다.
        //    _isDead = true;
        //    _aniControl.SetBool("isDead", true);
        //    CancelInvoke("ChangeDirection");                //방향전환 멈추기
        //    IngameManager._instance.AddKillCount(_kind);        //이 벌레의 종류와 같은 벌레를 죽인 횟수를 더하는 함수 호출     (이 벌레의 종류를 보내줌)
        //    Destroy(gameObject);
        //}
        transform.Translate(Vector3.up * _moveSpeed * Time.deltaTime);          //벌레 이동시키기
    }

    //이 벌레가 클릭되었을 때 호출되는 코루틴 함수?
    IEnumerator OnMouseDown()
    {
        GetComponent<CircleCollider2D>().enabled = false;       //죽은 벌레는 다시 클릭되지않게 Collider를 꺼준다.
        _isDead = true;
        _aniControl.SetBool("isDead", true);
        CancelInvoke("ChangeDirection");                //방향전환 멈추기
        IngameManager._instance.AddKillCount(_kind);        //이 벌레의 종류와 같은 벌레를 죽인 횟수를 더하는 함수 호출     (이 벌레의 종류를 보내줌)
        yield return new WaitForSeconds(_eraseTime);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("ItemBomb"))
        {
            StartCoroutine(OnMouseDown());
        }
            

    }

    //벌레가 방향을 전환하도록 하는 함수
    void ChangeDirection()
    {
        {
            //int _dir = Random.Range(0, 3); //0 , 1, 2
            //if (_dir == 0)
            //{
            //    transform.Rotate(Vector3.forward * _angle);
            //}
            //else if (_dir == 1)
            //{
            //    transform.Rotate(Vector3.forward * -_angle);
            //}
            //else return;
            //_passTime = 0;
        }   //내가 쓴 Turn 코드

        int _dir = Random.Range(0, 3);          //회전할 방향을 랜덤으로 받음
        float angle = 0;
        switch (_dir)
        {
            case 0:
                angle = _angle;
                break;
            case 1:
                angle = -_angle;
                break;
            default:
                break;
        }
        transform.Rotate(Vector3.forward * angle);              //Vector3.forward를 사용하여 "local축" 으로 회전 시킴.
        //_passTime = 0;
    }

    //벌레가 생성될때 벌레의 기본값 초기화
    public void InitializeData(DefineHelper.eInsectKind k)
    {
        _kind = k;          //이 벌레의 종류 대입

        _moveSpeed = DefineHelper._standardSpeed * DefineHelper._standardSpeedScalePerInsect[(int)k];       //벌레의 일반적인 속도 * 벌레 종류의 따른 속도값
        _angle = DefineHelper._standardAngle * DefineHelper._standardAngleScalePerInsect[(int)k];                   //벌레의 일반적인 회전값 * 벌레 종류의 따른 회전값
        _aniControl.speed = DefineHelper._standardSpeedScalePerInsect[(int)k];                                                    //애니메이션의 재생속도도 이동속도비율로 대입
    }
}
