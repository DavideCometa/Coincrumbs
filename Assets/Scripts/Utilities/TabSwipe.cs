using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabSwipe : MonoBehaviour {

    public GameObject scrollbar;
    public float scroll_pos = 0;
    float[] pos;
    bool swipe;

    public float start_pos;


    void Start() {
        scroll_pos = start_pos;
        scrollbar.GetComponent<Scrollbar>().value = start_pos;
        swipe = false;
    }


    void Update() {
        pos = new float[transform.childCount];
        float distance = 1f / (pos.Length - 1f);
        for (int i = 0; i < pos.Length; i++) {
            pos[i] = distance * i;
        }

        if (Input.GetMouseButton(0)) {
            scroll_pos = scrollbar.GetComponent<Scrollbar>().value;
            swipe = true;
        } else if(swipe) {
            for (int i = 0; i < pos.Length; i++) {
                if (scroll_pos < pos[i] + (distance / 2) && scroll_pos > pos[i] - (distance / 2)) {
                    scrollbar.GetComponent<Scrollbar>().value = Mathf.Lerp(scrollbar.GetComponent<Scrollbar>().value, pos[i], 0.1f);
                }
            }
        }

    }

    public void ForceSwipe(float pos) {
        scrollbar.GetComponent<Scrollbar>().value = pos;
        swipe = true;
    }

}

