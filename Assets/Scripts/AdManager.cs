using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;

public class AdManager : MonoBehaviour
{
    private BannerView bannerAd;
    // Start is called before the first frame update
    void Start()
    {
        MobileAds.Initialize(InitializationStatus => {  });
        this.RequestBanner();
    }

    private void RequestBanner() {
        string adUnitId = "ca-app-pub-3940256099942544/6300978111";
        this.bannerAd = new BannerView(adUnitId, AdSize.SmartBanner, AdPosition.Bottom);
        AdRequest request = new AdRequest.Builder().Build();
        this.bannerAd.LoadAd(request);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
