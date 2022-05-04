using UnityEngine;

public class GraphLoader : MonoBehaviour
{

    public WebDataCrawler wdCrawler;
    UserData user;
    bool started = false;

    void Start()
    {
        user = UserData.GetInstance;
        started = true;
        wdCrawler.RetrieveCoinHistory(user.lastCoinId, user.historyRange);
    }

    void OnEnable() {
        if(started)
            wdCrawler.RetrieveCoinHistory(user.lastCoinId, user.historyRange);
    }

    void Update()
    {
        
    }
}
