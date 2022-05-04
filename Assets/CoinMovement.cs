using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CoinMovement : MonoBehaviour
{
    float offset = 20f;

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
        float spawnY = Random.Range(0, Screen.height);
        float spawnX = Random.Range(0, Screen.width);

        float speed = Random.Range(15f, 30f);

        if (isActiveAndEnabled)
            transform.DOMove(new Vector3(spawnX, spawnY, 0), speed).SetEase(Ease.InOutSine).OnComplete(() => { ShakeElement(); });

    }

}
