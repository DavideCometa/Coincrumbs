using System;
using UnityEngine;
using UnityEngine.EventSystems;
using AwesomeCharts;

public class LineChartPress : MonoBehaviour, IDragHandler, IPointerUpHandler {

    private Vector2 fingerDown;
    private DateTime fingerDownTime;
    private Vector2 fingerUp;
    private DateTime fingerUpTime;

    Vector2 pos = new Vector2();
    Vector2 oldPos = new Vector2(0,0);

    private LineChart lineChart;
    int i = 0;

    float lcWidth;

    int index;

    void Start() {
        lineChart = gameObject.GetComponent<LineChart>();
        lcWidth = lineChart.transform.GetComponent<RectTransform>().rect.width;
    }

    void Update() {
        if (!ispressed)
            return;

        if (Input.GetMouseButton(0)) {
            this.fingerDown = Input.mousePosition;
            this.fingerUp = Input.mousePosition;
            this.fingerDownTime = DateTime.Now;
            pos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        }

        foreach (Touch touch in Input.touches) {
            if (touch.phase == TouchPhase.Moved) {
                this.fingerDown = touch.position;
                this.fingerUp = touch.position;
                this.fingerDownTime = DateTime.Now;
                pos = new Vector2(touch.position.x, touch.position.y);
            }

        }

        if(oldPos.x != 0) {
            if (oldPos.x != pos.x) {
                oldPos = pos;
                this.ShowValueIndicator();
            }
        } else {
            this.ShowValueIndicator();
            oldPos = pos;
        }
           

    }
    bool ispressed = false;

    public void OnDrag(PointerEventData eventData) {
        ispressed = true;
    }

    public void OnPointerUp(PointerEventData eventData) {
        ispressed = false;
        lineChart.ShowHideValuePopup(lineChart.entryIdicators[index]);
    }

    protected void ShowValueIndicator() {

        Vector2 localMousePosition = lineChart.transform.GetComponent<RectTransform>().InverseTransformPoint(Input.mousePosition);
        
        if (Mathf.Abs(localMousePosition.x) < lcWidth/2) {

            float pxPerPoint = lcWidth / lineChart.entryIdicators.Count;
            int tempIndex = Mathf.FloorToInt((localMousePosition.x + lcWidth / 2) / pxPerPoint);

            if(tempIndex != index) {
                index = tempIndex;
                lineChart.ShowHideValuePopup(lineChart.entryIdicators[index]);
            }

            

        }

    }
}
