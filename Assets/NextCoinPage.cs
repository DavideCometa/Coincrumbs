using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class NextCoinPage : MonoBehaviour
{

    float initPosX;
    float offset = 20f;

    public GameObject mainPanel;
    public TabSwipe scrollbar;

    bool justStarted = false;

    private void Awake() {
        justStarted = true;        
    }

    void Start()
    {
            initPosX = mainPanel.transform.TransformPoint(this.transform.localPosition).x;
            ShakeElement();
    }

    void Update()
    {
        
    }

    void OnEnable() {
        if(!justStarted)
            ShakeElement();

        justStarted = false;
    }

    void OnDisable() {

    }

    private void ShakeElement() {

        if (isActiveAndEnabled)
            transform.DOMoveX(initPosX+offset, 1f).SetEase(Ease.OutElastic).OnComplete(    () => { this.transform.DOMoveX(initPosX, 0.1f ).SetEase(Ease.Linear).OnComplete( () => {ShakeElement();} ); }    );  //StartCoroutine(Utils.ExecuteAfterWait(2, () => {/* Toast.transform.DOLocalMoveX(1000f, 2f).SetEase(Ease.InSine); */})));
    }

    public void OnTap() {

        scrollbar.ForceSwipe(1f);

    }
}
