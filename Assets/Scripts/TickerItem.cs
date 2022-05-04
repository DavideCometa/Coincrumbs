using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TickerItem : MonoBehaviour
{

    float tickerWidth;
    float pixelsPerSeconds;
    RectTransform rt;
    float initX;

    public float GetXPosition { get { return rt.anchoredPosition.x; } }
    public float GetInitX{ get { return initX; } }
    public float GetWidth { get { return rt.rect.width; } }

    public void Initialize(float tickerWidth, float pixelsPerSeconds, string message) {
        this.tickerWidth = tickerWidth;
        this.pixelsPerSeconds = pixelsPerSeconds;
        this.initX = transform.position.x;
        rt = GetComponent<RectTransform>();
        GetComponent<Text>().text = message;
    }

    void Update()
    {

        rt.position += Vector3.left * pixelsPerSeconds * Time.deltaTime;
        if (GetXPosition <= 0 - tickerWidth - GetWidth) {
            Destroy(gameObject);
        }
    }
}
