using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TextFlashMovement : MonoBehaviour
{
    float offset = 600f;

    bool justStarted = false;

    float initPosX, initPosY, initPosZ;
    public GameObject mainPanel;

    private void Awake() {
        justStarted = true;
    }

    public 


    void Start() {
        initPosX = mainPanel.transform.TransformPoint(this.transform.localPosition).x;
        initPosY = mainPanel.transform.TransformPoint(this.transform.localPosition).y;
        initPosZ = mainPanel.transform.TransformPoint(this.transform.localPosition).z;
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

        float speed = Random.Range(15f, 30f);

        transform.localPosition = new Vector3(initPosX, initPosY, initPosZ);

        if (isActiveAndEnabled)
            transform.DOMoveX(initPosX + offset, speed).SetEase(Ease.InOutSine).OnComplete(() => { ShakeElement(); });

    }
}
