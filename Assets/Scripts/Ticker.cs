using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ticker : MonoBehaviour
{

    public TickerItem tickerItemPrefab;
    [Range(1f,10f)]
    public float itemDuration = 3.0f;
    public string[] fillerItems;

    float width;
    float pixelsPerSeconds;
    public float spaceBetweenTickers = 200;
    TickerItem currentItem;

    int index = 0;
 
    void Start()
    {
        width = GetComponent<RectTransform>().rect.width;
        pixelsPerSeconds = Screen.width/itemDuration;
        Debug.Log("Current pix per sec is " + pixelsPerSeconds);
        AddTickerItem("<b><size=40><color=#7BFFEF>Latest News:</color></size></b>  " + fillerItems[index]);
        index++;
    }


    void Update()
    {

        if (fillerItems.Length == 0)
            return;

        if (currentItem != null && Mathf.Abs(currentItem.GetXPosition) - spaceBetweenTickers >= currentItem.GetWidth) {

            if (index >= fillerItems.Length)
                index = 0;

            AddTickerItem(fillerItems[index]);
            index++;
        } else if(currentItem == null) {
            index = 0;
            AddTickerItem("<b><size=40><color=#7BFFEF>Latest News:</color></size></b>  " + fillerItems[index]);
        }
    }

    void AddTickerItem(string message) {
        currentItem = Instantiate(tickerItemPrefab, transform);
        currentItem.Initialize(width, pixelsPerSeconds, message);
    }

    public void resetTicker() {
        index = 0;
        if(currentItem != null)
            Destroy(currentItem.gameObject);
        gameObject.SetActive(false);
    }
}
