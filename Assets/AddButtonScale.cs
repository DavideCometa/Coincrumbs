using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AddButtonScale : MonoBehaviour
{

    float offset = 20f;

    public GameObject mainPanel;
    public TabSwipe scrollbar;

    bool justStarted = false;

    private void Awake() {
        justStarted = true;
    }

    void Start() {
        //initPosX = mainPanel.transform.TransformPoint(this.transform.localPosition).x;
        ShakeElement();
    }

    void Update() {

    }

    void OnEnable() {
        if (!justStarted)
            ShakeElement();

        justStarted = false;
    }

    void OnDisable() {

    }

    private void ShakeElement() {

        if (isActiveAndEnabled)
            transform.DOScale(new Vector3(1.08f, 1.08f, 1.08f), 0.3f).SetEase(Ease.Linear).OnComplete(() => { transform.DOScale(new Vector3(1f, 1f, 1f), 0.3f).SetEase(Ease.Linear).OnComplete(() => { StartCoroutine(Utils.ExecuteAfterWait(5, () => { ShakeElement(); })); }); });
    }

}
