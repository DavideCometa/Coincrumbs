using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class LoaderScript : MonoBehaviour
{

    const string fixedContent = "Loading";

    public Image filler;
    public Text text;
    public GameObject loader;
    float initPos;

    int fillDirection = 1;
    string dots = "";

    void Start()
    {
        filler.fillClockwise = true;
        filler.fillAmount = 0.05f;
        text.text = fixedContent;
        dots = "";
        initPos = loader.transform.localPosition.y;
        Animate();
    }

    void Update()
    {
        filler.fillAmount += Time.deltaTime * 1.5f * fillDirection;
        if (filler.fillAmount >= 1 || filler.fillAmount <= 0) {
            filler.fillClockwise = !filler.fillClockwise;
            fillDirection *= -1;
            addDots();
        }
    }

    public void addDots() {
        dots += ".";
        if (dots.Length > 3)
            dots = "";

        text.text = fixedContent + dots;
    }

    void Animate() {
        loader.transform.DOLocalMoveY(initPos + 15f, 1).SetEase(Ease.Linear).OnComplete(() => {
            loader.transform.DOLocalMoveY(initPos, 1).SetEase(Ease.Linear).OnComplete(() => { Animate(); });
        });

    }

}
