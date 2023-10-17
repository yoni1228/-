using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public void InitializeData(DefineHelper.eSelectItemKind type)
    {
        if(type == DefineHelper.eSelectItemKind.Bomb)
        {
            StartCoroutine(ExplosionBomb());
        }
        else
        {
            GetLimitPlayTime();
        }
    }

    IEnumerator ExplosionBomb()
    {
        GameObject go = transform.GetChild(0).gameObject;
        go.SetActive(true);
        StartCoroutine(ChangeBombColor(go));
        yield return new WaitForSeconds(2);
        go = transform.GetChild(1).gameObject;
        go.SetActive(true);
        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        collider.enabled = true;
        yield return null;
        Destroy(gameObject,0.2f);
    }

    IEnumerator ChangeBombColor(GameObject go)
    {
        SpriteRenderer render = go.GetComponent<SpriteRenderer>();
        while(true)
        {
            render.color = Color.white;
            yield return new WaitForSeconds(0.3f);
            render.color = Color.red;
            yield return new WaitForSeconds(0.3f);
        }    }

    void GetLimitPlayTime()
    {
        IngameManager._instance.LimitTime += 10;
        Destroy(gameObject, 0.5f);
        //+10 UI생성
    }
}
