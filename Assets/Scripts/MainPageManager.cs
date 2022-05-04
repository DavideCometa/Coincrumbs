using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Globalization;
using System;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using AwesomeCharts;

public class MainPageManager : MonoBehaviour
{

    const float TIME_BETWEEN_UPDATES = 100f; // Each 100 seconds update data
    const float TRANSITION_SPEED = 0.5f;
    const float SEARCH_SUGGESTION_LIMIT = 10;

    private TouchScreenKeyboard keyboard;

    public GameObject SplashCanvas;
    public GameObject MainCanvas;
    public GameObject NoDataAvailablePanel;

    [Header("Containers")]
    public GameObject svMarket;
    public GameObject topCoinsContainer;
    public GameObject worstCoinsContainer;
    public GameObject trendingCoinsContainer;

    [Header("Prefabs")]
    public GameObject coinCardPrefab;
    public GameObject smallCoinCardPrefab;
    public GameObject searchResultPrefab;
    public GameObject chartPopupPrefab;

    [Header("UI Elems")]
    public GameObject globalMarketCapText;
    public Button AllSelector;
    public Button FavSelector; 
    public Button FilterSelector; 
    public GameObject MainTitle;
    public GameObject SearchBar;
    public GameObject TrendingPanel;
    public GameObject LoadingPanel;
    public GameObject CoinDetailsPanel;
    public GameObject MenuButton;
    public GameObject BackButton;
    public GameObject CoinTitle;
    public GameObject PortfolioTitle;
    public GameObject SentimentBox;
    public GameObject LineChart;
    public GameObject BarChart;
    public GameObject chartSliderGameObject;
    public GameObject sliderLabels;
    public GameObject Toast;
    public GameObject MainPageTicker;
    public GameObject CoinPageTicker;
    public TextMeshProUGUI descriptionContent;
    public VerticalLayoutGroup descContainerVLG;
    public Transform globalMarketCapPercentage;
    public GameObject CryptoMarketPriceChange;
    public Image HomeButton;
    public Image PortfolioButton;
    public Image MiscButton;

    [Header("Main Page Sections")]
    public GameObject TopPanel;
    public GameObject MarketsScrollView;
    public GameObject CoinInfoPanel;
    public GameObject SearchPanel;
    public GameObject PortfolioPanel;
    public GameObject AddCoinWindow;
    public GameObject PortfolioCoinMenuPanel;
    public GameObject CoinDescriptionPanel;
    public GameObject CoinInfoPanel2;
    public GameObject OrderPanel;

    public Transform CoinfInfo1Transform;

    [Header("Components")]
    public WebDataCrawler wdCrawler;
    public PortfolioManager portfolioManager;

    [Header("Textures")]
    public Texture starDisabled;
    public Texture starEnabled;
    public Texture imagePlaceholder;
    public Texture chartGradient;
    public Sprite positiveToast;
    public Sprite negativeToast;

    private LineChart lineChart;
    private BarChart barChart;
    private Slider chartSlider;
    private Text[] sliderLabelsTexts;

    List<GameObject> extraFavCards = new List<GameObject>();

    double totalPerc = 0;

    public enum Sections {
        HomePage,
        SearchPage,
        CoinInfoPage,
        PortfolioPage,
        AddCoinPortfolio,
        PortfolioCoinMenu,
        ConfirmRemoveCoinPanel,
        CoinDescriptionPanel
    };

    Sections currentSection = Sections.HomePage;

    Color32 bullish = new Color32(133,255,0,255);
    string bullishHex = "#85FF00";
    Color32 bearish = new Color32(255, 0, 33, 255);
    string bearishHex = "#F8251A";
    public Color32 accentColor = new Color32(123, 255, 239, 255);
    public Color32 disabledColor = new Color32(191, 191, 191, 200);

    [Header("Data Structures")]
    DataStructures.Coin[] allCoins = { };
    public Dictionary<string, DataStructures.Coin> coinsList = new Dictionary<string, DataStructures.Coin>();
    public Dictionary<string, DataStructures.Coin> topCoinsList = new Dictionary<string, DataStructures.Coin>();
    DataStructures.Coin[] marketCoins = { }; //last update coin markets data
    DataStructures.Coin[] topPerformingCoins = { new DataStructures.Coin(), new DataStructures.Coin(), new DataStructures.Coin() }; //Top 3 Performing Coins
    DataStructures.Coin[] worstPerformingCoins = { new DataStructures.Coin(), new DataStructures.Coin(), new DataStructures.Coin() }; //Worst 3 Performing Coins
    DataStructures.Coin[] trendingCoins = { }; //10 trending coins (most searched)
    GameObject[] searchResults = new GameObject[10];

    private double currTime;
    bool showAll = true;
    bool updateWhenBackToHome = false;

    public UserData user;

    void Start()
    {
        Application.targetFrameRate = 60;

        currTime = 0.0f;
        AllSelector.GetComponent<Image>().color = accentColor;
        FavSelector.GetComponent<Image>().color = disabledColor;
        FilterSelector.GetComponent<Image>().color = disabledColor;

        user = UserData.GetInstance;

        JumpToSection(Sections.HomePage);

        GetAllComponents();
    }

    void GetAllComponents() {
        lineChart = LineChart.GetComponent<LineChart>();
        barChart = BarChart.GetComponent<BarChart>();
        chartSlider = chartSliderGameObject.GetComponent<Slider>();

        sliderLabelsTexts = sliderLabels.transform.GetComponentsInChildren<Text>();
    }

    void Update()
    {
        currTime += Time.deltaTime;
        if(currTime > TIME_BETWEEN_UPDATES) {
            if (currentSection == Sections.HomePage) {
                wdCrawler.ReloadMainMarketsData();
            } else {
                updateWhenBackToHome = true;
            }
            currTime = 0.0f;
        }

        if(SplashCanvas.activeSelf && currTime > 5f) {
            if(user.startupPhase == 3) {
                MainCanvas.SetActive(true);
                SplashCanvas.SetActive(false);
                SetActiveButton(0);
            }
        }
    }

    public void OpenNoDataAvailablePanel() {
        NoDataAvailablePanel.SetActive(true);
    }

    public void UpdateCoinsList(DataStructures.Coin[] coins) {
        allCoins = coins;

        for(int i=0; i<allCoins.Length; i++) {
            if(!coinsList.ContainsKey(allCoins[i].name.ToLower()))
                coinsList.Add(allCoins[i].name.ToLower(), allCoins[i]);
            if (!coinsList.ContainsKey(allCoins[i].symbol.ToLower()))
                coinsList.Add(allCoins[i].symbol.ToLower(), allCoins[i]);
        }
        
    }

    public void UpdateCoinsData(DataStructures.Coin[] topCoins) {

        InitTopInfoPanelCoins(topCoins);

        bool firstLoad = false;

        if (marketCoins.Length == 0)
            firstLoad = true;

        totalPerc = 0;

        for (int i = 0; i < topCoins.Length; i++) {

            if (firstLoad) {

                GameObject coinCard = Instantiate(coinCardPrefab);
                coinCard.transform.SetParent(svMarket.transform);
                SetCoinCardData(coinCard, topCoins[i], i + 1);

                if (!topCoinsList.ContainsKey(topCoins[i].name.ToLower()))
                    topCoinsList.Add(topCoins[i].name.ToLower(), topCoins[i]);
                if(!topCoinsList.ContainsKey(topCoins[i].symbol.ToLower()))
                    topCoinsList.Add(topCoins[i].symbol.ToLower(), topCoins[i]);

            } else if (marketCoins[i].name != topCoins[i].name || marketCoins[i].last_updated != topCoins[i].last_updated) {

                GameObject coinCard = svMarket.transform.GetChild(i).gameObject;
                SetCoinCardData(coinCard,topCoins[i], i+1);

            }

            totalPerc += topCoins[i].price_change_percentage_24h;

            /* Find top coins */
            for (int j=0; j<3; j++) {
                if(topCoins[i].price_change_percentage_24h > topPerformingCoins[j].price_change_percentage_24h) {

                    DataStructures.Coin temp = new DataStructures.Coin();
                    DataStructures.Coin temp2 = new DataStructures.Coin();
                    temp = topPerformingCoins[j];
                    topPerformingCoins[j] = topCoins[i];

                    for (int z=j+1; z<3; z++) {
                        temp2 = topPerformingCoins[z];
                        topPerformingCoins[z] = temp;
                        temp = temp2;
                    }

                    break;
                }
            }

            /* Find worst coins */
            for (int j = 0; j < 3; j++) {
                if (topCoins[i].price_change_percentage_24h < worstPerformingCoins[j].price_change_percentage_24h) {

                    DataStructures.Coin temp = new DataStructures.Coin();
                    DataStructures.Coin temp2 = new DataStructures.Coin();
                    temp = worstPerformingCoins[j];
                    worstPerformingCoins[j] = topCoins[i];

                    for (int z = j + 1; z < 3; z++) {
                        temp2 = worstPerformingCoins[z];
                        worstPerformingCoins[z] = temp;
                        temp = temp2;
                    }

                    break;
                }
            }

        }

        Debug.Log("Market is on a " + totalPerc/100 + " change in last 24H");

        marketCoins = topCoins;

        for (int i = 0; i < 3; i++) {
            var name = topPerformingCoins[i].name;
            var id = topPerformingCoins[i].id;
            var symbol = topPerformingCoins[i].symbol;
            GameObject smallCoinCard = Instantiate(smallCoinCardPrefab);
            smallCoinCard.transform.SetParent(topCoinsContainer.transform);
            StartCoroutine(wdCrawler.LoadImageCoroutine(topPerformingCoins[i].id, topPerformingCoins[i].image, smallCoinCard.transform.GetChild(0).gameObject));
            smallCoinCard.transform.GetChild(1).GetComponent<Text>().text = name;
            smallCoinCard.transform.GetChild(2).GetComponent<Text>().text = "$ " + Utils.ReturnFormattedPrice(topPerformingCoins[i].current_price);
            smallCoinCard.transform.GetChild(3).GetComponent<Text>().text = Math.Round(topPerformingCoins[i].price_change_percentage_24h, 2).ToString("F2") + "%";
            smallCoinCard.transform.GetChild(3).GetComponent<Text>().color = bullish;
            smallCoinCard.GetComponent<Button>().onClick.AddListener(() => OnCoinTap(id, name, symbol));
            smallCoinCard.transform.localScale = new Vector3(1, 1, 1);

            var name2 = worstPerformingCoins[i].name;
            var id2 = worstPerformingCoins[i].id;
            var symbol2 = worstPerformingCoins[i].symbol;
            GameObject smallCoinCard2 = Instantiate(smallCoinCardPrefab);
            smallCoinCard2.transform.SetParent(worstCoinsContainer.transform);
            StartCoroutine(wdCrawler.LoadImageCoroutine(worstPerformingCoins[i].id, worstPerformingCoins[i].image, smallCoinCard2.transform.GetChild(0).gameObject));
            smallCoinCard2.transform.GetChild(1).GetComponent<Text>().text = name2;
            smallCoinCard2.transform.GetChild(2).GetComponent<Text>().text = "$ " + Utils.ReturnFormattedPrice(worstPerformingCoins[i].current_price);
            smallCoinCard2.transform.GetChild(3).GetComponent<Text>().text = Math.Round(worstPerformingCoins[i].price_change_percentage_24h, 2).ToString("F2") + "%";
            smallCoinCard2.transform.GetChild(3).GetComponent<Text>().color = bearish;
            smallCoinCard2.GetComponent<Button>().onClick.AddListener(() => OnCoinTap(id2, name2, symbol2));
            smallCoinCard2.transform.localScale = new Vector3(1, 1, 1);
        }

    }

    public void UpdateTrendingCoins(DataStructures.Coin[] trending) {
        trendingCoins = trending;
    }

    private void InitTopInfoPanelCoins(DataStructures.Coin[] topCoins) {

        foreach (Transform child in topCoinsContainer.transform) {
            GameObject.Destroy(child.gameObject);
        }

        foreach (Transform child in worstCoinsContainer.transform) {
            GameObject.Destroy(child.gameObject);
        }

        for (int i = 0; i < 3; i++) {
            topPerformingCoins[i].name = topCoins[i].name;
            topPerformingCoins[i].image = topCoins[i].image;
            topPerformingCoins[i].current_price = topCoins[i].current_price;
            topPerformingCoins[i].price_change_percentage_24h = topCoins[i].price_change_percentage_24h;

            worstPerformingCoins[i].name = topCoins[i].name;
            worstPerformingCoins[i].image = topCoins[i].image;
            worstPerformingCoins[i].current_price = topCoins[i].current_price;
            worstPerformingCoins[i].price_change_percentage_24h = topCoins[i].price_change_percentage_24h;
        }

    }

    /**
     * TOP 100 COINS
     */
    public void SetCoinCardData(GameObject card, DataStructures.Coin coin, int rank) {
        StartCoroutine(wdCrawler.LoadImageCoroutine(coin.id, coin.image, card.transform.GetChild(3).gameObject));

        card.GetComponent<Button>().onClick.RemoveAllListeners();

        var cardChild2 = card.transform.GetChild(2);
        var cardPriceChangeText = cardChild2.GetChild(1).GetChild(0).GetComponent<Text>();
        card.name = coin.name;
        card.transform.GetChild(0).GetComponent<Text>().text = Utils.CheckStringEllipsis(coin.name, 18);
        card.transform.GetChild(1).GetComponent<Text>().text = "#" + rank;
        cardChild2.GetChild(0).GetComponent<Text>().text = Utils.ReturnFormattedPrice(coin.current_price) + " $";// Utils.ReturnFormattedPrice(coin.current_price.ToString()) + " $";
        cardPriceChangeText.text = Math.Round(coin.price_change_percentage_24h, 2).ToString("F2") + "%";
        cardPriceChangeText.color = (GetTrendColor(coin.price_change_percentage_24h)) ? bullish : bearish;

        if (GetTrendColor(coin.price_change_percentage_24h)) {
            cardChild2.GetChild(2).gameObject.SetActive(false);
            cardChild2.GetChild(3).gameObject.SetActive(true);
        } else {
            cardChild2.GetChild(2).gameObject.SetActive(true);
            cardChild2.GetChild(3).gameObject.SetActive(false);
        }

        //CAPITAL
        card.transform.GetChild(4).GetComponent<Text>().text = "Cap: $" + Utils.ReturnFormattedUSNumber(coin.market_cap);

        //FAVORITES
        card.transform.GetChild(6).GetComponent<Button>().onClick.AddListener(() => OnFavoriteTap(coin.name, coin.id, card));
        card.GetComponent<Button>().onClick.AddListener(() => OnCoinTap(coin.id, coin.name, coin.symbol));
        if (user.favorites.ContainsKey(coin.name)) {
            RawImage img = card.transform.GetChild(6).GetComponent<RawImage>();
            img.texture = starEnabled;
            img.color = Color.white;
        }
            
        card.transform.localScale = new Vector3(1, 1, 1); //Used because layout scales up automatically
    }

    public void updateGlobalData(double globalMarketCap, double marketCapChange) {
        globalMarketCapText.GetComponent<Text>().text = "$ " + Utils.ReturnFormattedUSNumber(globalMarketCap.ToString());
        globalMarketCapPercentage.GetChild(1).GetComponent<Text>().text = marketCapChange.ToString("F2") + "%";
        globalMarketCapPercentage.GetChild(1).GetComponent<Text>().color = GetTrendColor(marketCapChange) ? bullish : bearish;

        if (GetTrendColor(marketCapChange)) {
            globalMarketCapPercentage.GetChild(0).GetChild(0).gameObject.SetActive(false);
            globalMarketCapPercentage.GetChild(0).GetChild(1).gameObject.SetActive(true);
        } else {
            globalMarketCapPercentage.GetChild(0).GetChild(0).gameObject.SetActive(true);
            globalMarketCapPercentage.GetChild(0).GetChild(1).gameObject.SetActive(false);
        }

        string marketChange = "";
        if (GetTrendColor(totalPerc / 100)) {
            marketChange = "<color=" + bullishHex + "> " + (totalPerc / 100).ToString("F2") + "%</color>";
        } else {
            marketChange = "<color=" + bearishHex + ">" + (totalPerc / 100).ToString("F2") + "%</color>";
        }

        CryptoMarketPriceChange.GetComponent<Text>().text = "Market is on a " + marketChange + " change in last 24 hours";

    }

    public void UpdateChartData(LineDataSet set, double min, double max, string[] horizontal_values, BarDataSet volumes) {
        // Configure line
        set.LineColor = new Color32(42, 171, 147, 255);
        set.LineThickness = 5;
        set.UseBezier = false;
        set.FillTexture = chartGradient;
        set.FillColor = new Color32(42, 171, 147, 120);

        double extraSpan = (max - min) * 0.15f;
        lineChart.AxisConfig.VerticalAxisConfig.Bounds.Min = (float) min - (float) extraSpan;
        lineChart.AxisConfig.VerticalAxisConfig.Bounds.Max = (float) max + (float) extraSpan;

        lineChart.AxisConfig.HorizontalAxisConfig.ValueFormatterConfig.CustomValues.Clear();
        lineChart.AxisConfig.HorizontalAxisConfig.ValueFormatterConfig.CustomValues.AddRange(horizontal_values);

        if(min < 0.001f) {
            lineChart.AxisConfig.VerticalAxisConfig.ValueFormatterConfig.ValueDecimalPlaces = 6;
        } else if(min < 0f) {
            lineChart.AxisConfig.VerticalAxisConfig.ValueFormatterConfig.ValueDecimalPlaces = 5;
        } else if (min < 1f) {
            lineChart.AxisConfig.VerticalAxisConfig.ValueFormatterConfig.ValueDecimalPlaces = 4;
        } else if (min < 1000f) {
            lineChart.AxisConfig.VerticalAxisConfig.ValueFormatterConfig.ValueDecimalPlaces = 2;
        } else if (min < 10000f) {
            lineChart.AxisConfig.VerticalAxisConfig.ValueFormatterConfig.ValueDecimalPlaces = 1;
        } else {
            lineChart.AxisConfig.VerticalAxisConfig.ValueFormatterConfig.ValueDecimalPlaces = 0;
        }

        lineChart.Config.PopupPrefab = chartPopupPrefab.GetComponent<ChartValuePopup>();
        // Add data set to chart data
        lineChart.GetChartData().DataSets.Clear();
        lineChart.GetChartData().DataSets.Add(set);
        // Refresh chart after data change
        lineChart.SetDirty();

        volumes.BarColors.Clear();
        volumes.BarColors.Add(new Color32(95, 234, 226, 180));

        barChart.GetChartData().DataSets.Clear();
        barChart.GetChartData().DataSets.Add(volumes);
        barChart.SetDirty();
    }

    /**
     * TRENDING COINS
     */
    public void addCardToTrendingCoins(string id, string name, string img_url, double price, double price_change_24, string symbol) {
        GameObject smallCoinCard = Instantiate(smallCoinCardPrefab);
        smallCoinCard.transform.SetParent(trendingCoinsContainer.transform);
        StartCoroutine(wdCrawler.LoadImageCoroutine(id, img_url, smallCoinCard.transform.GetChild(0).gameObject));
        smallCoinCard.transform.GetChild(1).GetComponent<Text>().text = name;
        smallCoinCard.transform.GetChild(2).GetComponent<Text>().text = "$ " + Utils.ReturnFormattedPrice(double.Parse(price.ToString()));
        smallCoinCard.transform.GetChild(3).GetComponent<Text>().text = Math.Round(price_change_24, 2).ToString("F2") + "%";
        smallCoinCard.GetComponent<Button>().onClick.AddListener(() => OnCoinTap(id, name, symbol));
        smallCoinCard.transform.localScale = new Vector3(1, 1, 1);

        if(GetTrendColor(double.Parse(price_change_24.ToString()))) {
            smallCoinCard.transform.GetChild(3).GetComponent<Text>().color = bullish;
        } else {
            smallCoinCard.transform.GetChild(3).GetComponent<Text>().color = bearish;
        }
        
    }

    public bool GetTrendColor(double val) {
        if (val > 0)
            return true;
        else return false;
    }

    public void OnFavoriteTap(string coinName, string id, GameObject coinCard) {
        RawImage img = coinCard.transform.GetChild(6).GetComponent<RawImage>();
        if (user.favorites.ContainsKey(coinName)) {
            img.texture = starDisabled;
            img.color = disabledColor;
            user.favorites.Remove(coinName);
        } else {
            img.texture = starEnabled;
            img.color = Color.white;
            user.favorites.Add(coinName, id);
        }

        user.SaveUserData();
    }

    public void OnFavoriteTapFromCoinInfo(string coinName, string id, Transform favButton) {
        RawImage img = favButton.GetComponent<RawImage>();
        if (user.favorites.ContainsKey(coinName)) {
            img.texture = starDisabled;
            img.color = disabledColor;
            user.favorites.Remove(coinName);
        } else {
            img.texture = starEnabled;
            img.color = Color.white;
            user.favorites.Add(coinName, id);
        }

        user.SaveUserData();
    }

    public void OnCoinTap(string id, string name, string symbol) {

        user.lastCoinId = id;
        user.historyRange = 0;

        if (user.addingCoin) {
            JumpToSection(Sections.PortfolioPage);
            portfolioManager.AddCoin(id, name, symbol);
            return;
        } else {
            JumpToSection(Sections.CoinInfoPage);
        }

        CoinDetailsPanel.SetActive(false);
        LoadingPanel.SetActive(true);

        DataStructures.CoinDetails coin = new DataStructures.CoinDetails();
        //Check if info are already downloaded
        if (wdCrawler.coinDetails.ContainsKey(id)) {
            wdCrawler.coinDetails.TryGetValue(id, out coin); //Coin data must be updated: Price, price change, cap, news...
            StartCoroutine(OnCoinDataLoaded(id, coin.image, CoinfInfo1Transform.GetChild(0).gameObject, coin));
        } else {
            wdCrawler.RetrieveCoinData(id, true, () => StartCoroutine(OnCoinDataLoaded(id, coin.image, CoinfInfo1Transform.GetChild(0).gameObject)));
        }

        wdCrawler.RetrieveCoinNews(symbol);

        CoinTitle.transform.GetChild(0).GetComponent<Text>().text = "<color=7BFFEF>" + name + "</color>" + " (" + symbol + ")";

        /* TRANSITIONS */
        MainTitle.transform.DOLocalMoveY(100, TRANSITION_SPEED);
        MenuButton.transform.DOLocalMoveY(100, TRANSITION_SPEED);
        SearchBar.transform.DOLocalMoveY(100, TRANSITION_SPEED);
        BackButton.transform.DOLocalMoveY(-90, TRANSITION_SPEED);
        CoinTitle.transform.DOLocalMoveY(-90, TRANSITION_SPEED);

        chartSlider.value = 0;

    }

    public void OnMoreInfo() {
        JumpToSection(Sections.CoinDescriptionPanel);
    }

    public IEnumerator OnCoinDataLoaded(string id, string imageUrl, GameObject container, DataStructures.CoinDetails coin = null) {
        StartCoroutine(wdCrawler.LoadImageCoroutine(id, imageUrl, container)); //Load Image

        if (coin == null)
            wdCrawler.coinDetails.TryGetValue(id, out coin);

        string color;
        if (GetTrendColor(double.Parse(coin.price_change_percentage_24h.ToString())))
            color = "<size=40><color=" + bullishHex + "> +";
        else
            color = "<size=40><color=" + bearishHex + ">";

        var bottomSectionPriceChangeTable = CoinfInfo1Transform.GetChild(7);

        CoinfInfo1Transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = Utils.ReturnFormattedPrice(double.Parse(coin.price.ToString()))+ "<size=30>USD</size> ";

        CoinfInfo1Transform.GetChild(2).GetComponent<Button>().onClick.RemoveAllListeners();
        CoinfInfo1Transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() => OnFavoriteTapFromCoinInfo(coin.name, coin.id, CoinfInfo1Transform.GetChild(2)));
        RawImage img = CoinfInfo1Transform.GetChild(2).GetComponent<RawImage>();
        if (user.favorites.ContainsKey(coin.name)) {
            img.texture = starEnabled;
            img.color = Color.white;
        } else { 
            img.texture = starDisabled;
            img.color = Color.white;
        }
        //TODO: Update star in main market cap cards

        CoinfInfo1Transform.GetChild(5).GetComponent<TextMeshProUGUI>().text = color + Math.Round(coin.price_change_percentage_24h, 2).ToString("F2") + "%</size></color>";
        CoinfInfo1Transform.GetChild(3).GetComponent<Text>().text = Utils.ReturnFormattedUSNumber(coin.market_cap.ToString()) + " <size=20>USD</size>";
        CoinfInfo1Transform.GetChild(4).GetComponent<Text>().text = "#"+ coin.market_cap_rank.ToString();

        CoinfInfo1Transform.GetChild(9).GetComponent<Text>().text = Utils.ReturnFormattedPrice(double.Parse(coin.total_volume.ToString())) + " <size=20>USD</size>";
        CoinfInfo1Transform.GetChild(10).GetComponent<Text>().text = Utils.ReturnFormattedPrice(double.Parse(coin.high24.ToString())) + " <size=20>USD</size>";
        CoinfInfo1Transform.GetChild(11).GetComponent<Text>().text = Utils.ReturnFormattedPrice(double.Parse(coin.low24.ToString())) + " <size=20>USD</size>";

        descriptionContent.text = coin.description;
        descContainerVLG.spacing = 29f;

        if(coin.total_supply != "Unlimited")
            CoinInfoPanel2.transform.GetChild(4).GetComponent<Text>().text = Utils.ReturnFormattedPrice(double.Parse(coin.total_supply));
        else
            CoinInfoPanel2.transform.GetChild(4).GetComponent<Text>().text = coin.total_supply;

        CoinInfoPanel2.transform.GetChild(5).GetComponent<Text>().text = Utils.ReturnFormattedPrice(double.Parse(coin.circulating_supply));
        CoinInfoPanel2.transform.GetChild(6).GetComponent<Text>().text = Utils.ReturnFormattedPrice(double.Parse(coin.ath.ToString())) + " <size=20>USD</size>"; ;
        CoinInfoPanel2.transform.GetChild(7).GetComponent<Text>().text = Utils.ReturnFormattedPrice(double.Parse(coin.atl.ToString())) + " <size=20>USD</size>";

        SentimentBox.transform.GetChild(0).GetComponent<Text>().text = coin.sentiment_votes_up_percentage + "%";
        SentimentBox.transform.GetChild(1).GetComponent<Text>().text = coin.sentiment_votes_down_percentage + "%";
        bottomSectionPriceChangeTable.GetChild(0).GetChild(1).GetComponent<Text>().text = (GetTrendColor(double.Parse(coin.price_change_percentage_7d.ToString()))) ? "<color=" + bullishHex + ">+" + Math.Round(coin.price_change_percentage_7d, 2).ToString("F2") +"%</color>" : "<color=" + bearishHex + ">" + Math.Round(coin.price_change_percentage_7d, 2).ToString("F2") + "%</color>";
        bottomSectionPriceChangeTable.GetChild(1).GetChild(1).GetComponent<Text>().text = (GetTrendColor(double.Parse(coin.price_change_percentage_14d.ToString()))) ? "<color=" + bullishHex + ">+" + Math.Round(coin.price_change_percentage_14d, 2).ToString("F2") + "%</color>" : "<color=" + bearishHex + ">" + Math.Round(coin.price_change_percentage_14d, 2).ToString("F2") + "%</color>";
        bottomSectionPriceChangeTable.GetChild(2).GetChild(1).GetComponent<Text>().text = (GetTrendColor(double.Parse(coin.price_change_percentage_30d.ToString()))) ? "<color=" + bullishHex + ">+" + Math.Round(coin.price_change_percentage_30d, 2).ToString("F2") + "%</color>" : "<color=" + bearishHex + ">" + Math.Round(coin.price_change_percentage_30d, 2).ToString("F2") + "%</color>";
        bottomSectionPriceChangeTable.GetChild(3).GetChild(1).GetComponent<Text>().text = (GetTrendColor(double.Parse(coin.price_change_percentage_60d.ToString()))) ? "<color=" + bullishHex + ">+" + Math.Round(coin.price_change_percentage_60d, 2).ToString("F2") + "%</color>" : "<color=" + bearishHex + ">" + Math.Round(coin.price_change_percentage_60d, 2).ToString("F2") + "%</color>";
        bottomSectionPriceChangeTable.GetChild(4).GetChild(1).GetComponent<Text>().text = (GetTrendColor(double.Parse(coin.price_change_percentage_200d.ToString()))) ? "<color=" + bullishHex + ">+" + Math.Round(coin.price_change_percentage_200d, 2).ToString("F2") + "%</color>" : "<color=" + bearishHex + ">" + Math.Round(coin.price_change_percentage_200d, 2).ToString("F2") + "%</color>";
        bottomSectionPriceChangeTable.GetChild(5).GetChild(1).GetComponent<Text>().text = (GetTrendColor(double.Parse(coin.price_change_percentage_1y.ToString()))) ? "<color=" + bullishHex + ">+" + Math.Round(coin.price_change_percentage_1y, 2).ToString("F2") + "%</color>" : "<color=" + bearishHex + ">" + Math.Round(coin.price_change_percentage_1y, 2).ToString("F2") + "%</color>";

        //string s = CoinTitle.transform.GetChild(0).GetComponent<Text>().text;
        CoinTitle.transform.GetChild(0).GetComponent<Text>().text = "<size=30>#" + coin.market_cap_rank.ToString() + "</size><color=7BFFEF>" + coin.name + "</color> (" + coin.symbol + ")";

        LoadingPanel.SetActive(false);
        CoinDetailsPanel.SetActive(true);
        yield return null;
    }

    public void AddNewsToTicker(string[] news) {
        if(currentSection == Sections.HomePage) {
            if (news.Length != 0) {
                MainPageTicker.GetComponent<Ticker>().fillerItems = news;
                MainPageTicker.SetActive(true);
            }

        } else if(currentSection == Sections.CoinInfoPage) {
            if (news.Length != 0) {
                CoinPageTicker.GetComponent<Ticker>().fillerItems = news;
                CoinPageTicker.SetActive(true);
            }

        }

    }

    private void ShowFavoriteCoins() {
        if (!showAll) {
            Dictionary<string, object> favCopy = new Dictionary<string, object>(user.favorites);
            foreach (Transform transform in svMarket.transform) {
                GameObject card = transform.gameObject;
                if (!user.favorites.ContainsKey(card.name)) {
                    card.SetActive(false);
                } else {
                    favCopy.Remove(card.name);
                    card.SetActive(true);
                }
            }

            string queryIDs = "";
            string names = "";

            extraFavCards.Clear();

            foreach (string name  in favCopy.Keys) {
                names += name + ",";
                GameObject coinCard = Instantiate(coinCardPrefab);
                coinCard.transform.SetParent(svMarket.transform);
                object id;
                favCopy.TryGetValue(name, out id);
                queryIDs += id.ToString() + ",";
                extraFavCards.Add(coinCard); 
            }

            wdCrawler.RetrieveCoinsSimplePrice(queryIDs, extraFavCards, names);
        } else {

            foreach (Transform transform in svMarket.transform) {
                GameObject card = transform.gameObject;
                card.SetActive(true);
            }

            foreach (GameObject g in extraFavCards) {
                g.SetActive(false);
            }

        }
    }

    ///
    /// Listeners for UI interactions
    /// 

    public void OnSearchInput() {

        InputField searchInputField = SearchBar.GetComponent<InputField>();
        string input = searchInputField.text.ToLower();
        Dictionary<string, object> result = new Dictionary<string, object>();

        if (input.Length != 0) {

            if(TrendingPanel.activeSelf)
                TrendingPanel.SetActive(false);

            foreach (string s in ((new List<string>(topCoinsList.Keys)).FindAll(w => w.StartsWith(input)))) {
                DataStructures.Coin value;
                topCoinsList.TryGetValue(s, out value);

                if (value.name != null && !result.ContainsKey(value.name.ToLower()) && !result.ContainsKey(value.symbol.ToLower())) {
                        result.Add(s, value);
                }

            }
            

            foreach (string s in Utils.SortByLength((new List<string>(coinsList.Keys)).FindAll(w => w.StartsWith(input)))) {

                DataStructures.Coin value;
                coinsList.TryGetValue(s, out value);

                if (value.name != null && !result.ContainsKey(s) && !result.ContainsKey(value.name.ToLower()) && !result.ContainsKey(value.symbol.ToLower()))
                    result.Add(s, value);

            }

            int i = 0;
            foreach (DataStructures.Coin o in result.Values) {

                if (i > SEARCH_SUGGESTION_LIMIT - 1)
                    break;

                GameObject searchResult;

                if (searchResults.Length == 0 || searchResults[i] == null)
                    searchResult = Instantiate(searchResultPrefab);
                else {
                    searchResult = searchResults[i];
                    searchResult.SetActive(true);
                    searchResult.GetComponent<Button>().onClick.RemoveAllListeners();
                }

                searchResult.transform.SetParent(SearchPanel.transform.GetChild(0).transform);
                StartCoroutine(wdCrawler.LoadImageCoroutine(o.id, o.image, searchResult.transform.GetChild(2).gameObject));
                searchResult.transform.GetChild(0).GetComponent<Text>().text = o.name + " ("+o.symbol+")";
                searchResult.GetComponent<Button>().onClick.AddListener(() => OnCoinTap(o.id, o.name, o.symbol));

                searchResult.transform.localScale = new Vector3(1, 1, 1);

                searchResults[i] = searchResult;

                i++;
            }

            for (int j=i; j <= SEARCH_SUGGESTION_LIMIT - 1; j++)
                searchResults[j].SetActive(false);

            result.Clear();

        } else {
            for (int i = 0; i <= SEARCH_SUGGESTION_LIMIT - 1; i++)
                searchResults[i].SetActive(false);

            TrendingPanel.SetActive(true);
        }

        
            
    }

    public void OnSearchTap() {

        JumpToSection(Sections.SearchPage);

        InputField searchInputField = SearchBar.GetComponent<InputField>();
        searchInputField.text = "";
        searchInputField.Select();
        searchInputField.ActivateInputField();
        //keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);

        //Load trending searches
        if (TrendingPanel.transform.childCount < 2) {
            for (int i = 0; i < 7; i++) {
                var name = trendingCoins[i].name;
                var id= trendingCoins[i].id;
                var symbol = trendingCoins[i].symbol;
                GameObject trendingCoin = Instantiate(searchResultPrefab);
                trendingCoin.transform.SetParent(TrendingPanel.transform);
                trendingCoin.transform.GetChild(0).GetComponent<Text>().text = name + " (" + symbol + ")";
                StartCoroutine(wdCrawler.LoadImageCoroutine(trendingCoins[i].id, trendingCoins[i].image, trendingCoin.transform.GetChild(2).gameObject));
                trendingCoin.GetComponent<Button>().onClick.AddListener(() => OnCoinTap(id, name, symbol));
                trendingCoin.transform.localScale = new Vector3(1, 1, 1);
            }
        }
    }

    public void OnChartSliderChanged(int val = -1) {

        user.historyRange = Mathf.RoundToInt((val != -1 ? val : chartSlider.value));
        int value = 8 - user.historyRange;

        for(int i = 0; i < 9; i++) {
            if(i == value) {
                sliderLabelsTexts[i].color = accentColor;
                sliderLabelsTexts[i].fontSize = 40;
            } else {
                sliderLabelsTexts[i].color = Color.white;
                sliderLabelsTexts[i].fontSize = 30;
            }
        }

        wdCrawler.RetrieveCoinHistory(user.lastCoinId, user.historyRange);
    }

    public void OnBackTap() {
        JumpToSection(user.backstack.Pop(), true);
    }

    public void OnFilterSelected() {
        var go = EventSystem.current.currentSelectedGameObject;
        if (go != null) {
            switch (go.name) {
                case "All":
                    AllSelector.GetComponent<Image>().color = accentColor;
                    FavSelector.GetComponent<Image>().color = disabledColor;
                    FilterSelector.GetComponent<Image>().color = disabledColor;
                    showAll = true;
                    ShowFavoriteCoins();
                    break;
                case "Favorite":
                    AllSelector.GetComponent<Image>().color = disabledColor;
                    FavSelector.GetComponent<Image>().color = accentColor;
                    FilterSelector.GetComponent<Image>().color = disabledColor;
                    showAll = false;
                    ShowFavoriteCoins();
                    break;
                case "Order":
                    AllSelector.GetComponent<Image>().color = disabledColor;
                    FavSelector.GetComponent<Image>().color = disabledColor;
                    FilterSelector.GetComponent<Image>().color = accentColor;
                    showAll = true;
                    //OrderPanel.SetActive(true);
                    break;
                default:
                    break;
            }
        }           
    }

    /*public void OnOrderSelected(TMPro.TMP_Dropdown dropdown) {
        switch(dropdown.value) {
            case 0:
                break;
            case 1:

                quickSort

                break;
            case 2:
                break;
            case 3:
                break;
            case 4:
                break;
            case 5:
                break;
            default:
                break;
        }
    }

    public void QuickSort(DataStructures.Coin[] arr, int low, int high) {
        if (low < high) {

            int pi = Partition(arr, low, high);

            QuickSort(arr, low, pi - 1);  // Before pi
            QuickSort(arr, pi + 1, high); // After pi
        }
    }

    public int Partition(DataStructures.Coin[] array, int low, int high) {
        // pivot (Element to be placed at right position)

        DataStructures.Coin pivot = array[high].current_price;

        int i = (low - 1);  // Index of smaller element

    for (int j = low; j <= high - 1; j++) {
            // If current element is smaller than the pivot
            if (array[j] < pivot) {
                i++;    // increment index of smaller element
                swap array[i] and array[j]
            }
        }
        swap arr[i + 1] and arr[high])
    return (i + 1)
}*/

    public void OrderCoinsBy(int i) {

    }

    public void OnPortfolioSelected() {
        JumpToSection(Sections.PortfolioPage);
    }

    public void OnPortfolioAddCoin() {
        user.addingCoin = true;
        OnSearchTap();
    }
    public void OnHomeSelected() {
        JumpToSection(Sections.HomePage);
    }


    public void JumpToSection(Sections section, bool back = false) {

        if (section == currentSection)
            return;

        user.previousSection = currentSection;
        if (user.previousSection == Sections.CoinInfoPage)
            CoinPageTicker.GetComponent<Ticker>().resetTicker();

        if (user.previousSection == Sections.HomePage)
            MainPageTicker.GetComponent<Ticker>().resetTicker();

        if ((user.previousSection == Sections.AddCoinPortfolio || user.previousSection == Sections.PortfolioPage) && section != Sections.SearchPage) {
            user.addingCoin = false;
        }

        currentSection = section;
        switch (section) {
            case Sections.HomePage:
                EnablePanels(true, true, false, false, false, false);
                ShowUIElements(false, true, true, false, false, false);
                user.backstack.Clear();

                if(updateWhenBackToHome) {
                    wdCrawler.ReloadMainMarketsData();
                    currTime = 0.0f;
                    updateWhenBackToHome = false;
                }

                MainPageTicker.SetActive(true);

                SetActiveButton(0);

                break;
            case Sections.SearchPage:

                SetActiveButton(2);

                EnablePanels(false, false, true, true, false, false);
                ShowUIElements(true, false, false, false, false, true);
                if (!back)
                    user.backstack.Push(user.previousSection);
                else
                    OnSearchInput();
                break;
            case Sections.CoinInfoPage:
                EnablePanels(false, false, false, false, true, false);
                ShowUIElements(false, false, false, false, true, true);
                if ((user.backstack.Count == 0 || user.backstack.Peek() != Sections.CoinInfoPage) && user.previousSection != Sections.CoinDescriptionPanel) {
                    user.backstack.Push(user.previousSection);
                } else if (user.backstack.Peek() == Sections.CoinInfoPage) {
                    user.backstack.Pop();
                }
                CoinPageTicker.SetActive(true);
                break;
            case Sections.PortfolioPage:
                EnablePanels(false, false, false, false, false, true);
                ShowUIElements(false, true, false, true, false, false);
                user.backstack.Clear();
                SetActiveButton(1);
                break;
            case Sections.AddCoinPortfolio:
                EnablePanels(false, false, false, false, false, true, true);
                ShowUIElements(false, false, false, true, false, true);
                user.backstack.Push(user.previousSection);
                break;
            case Sections.PortfolioCoinMenu:
                EnablePanels(false, false, false, false, false, true, false, true);
                ShowUIElements(false, false, false, true, false, true);
                if(!back)
                    user.backstack.Push(user.previousSection);
                break;
            case Sections.ConfirmRemoveCoinPanel:
                EnablePanels(false, false, false, false, false, true, false, true);
                ShowUIElements(false, false, false, true, false, true);
                user.backstack.Push(user.previousSection);
                break;
            case Sections.CoinDescriptionPanel:
                EnablePanels(false, false, false, false, true, false, false, false, true);
                ShowUIElements(false, false, false, false, true, true);
                user.backstack.Push(user.previousSection);
                break;
            default:
                break;
        }
    }

    public void ShowToastMessage(bool positive, string content) {
        if(positive) {
            Toast.GetComponent<Image>().sprite = positiveToast;
        } else {
            Toast.GetComponent<Image>().sprite= negativeToast;
        }

        Toast.transform.GetChild(0).GetComponent<Text>().text = content;
        Toast.transform.DOLocalMoveX(0f, 1.5f).SetEase(Ease.OutSine).OnComplete(() => StartCoroutine(Utils.ExecuteAfterWait(2, () => { Toast.transform.DOLocalMoveX(1000f, 2f).SetEase(Ease.InSine); })));
    }

    public void ShowUIElements(bool searchBar, bool menuButton, bool mainTitle, bool portfolioTitle, bool coinTitle, bool backButton) {
        BackButton.transform.DOLocalMoveY(backButton ? -70 : 150, TRANSITION_SPEED);
        SearchBar.transform.DOLocalMoveY(searchBar ? -70 : 150, TRANSITION_SPEED);
        MainTitle.transform.DOLocalMoveY(mainTitle ? -70 : 150, TRANSITION_SPEED);
        MenuButton.transform.DOLocalMoveY(menuButton ? -70 : 150, TRANSITION_SPEED);
        CoinTitle.transform.DOLocalMoveY(coinTitle ? -70 : 150, TRANSITION_SPEED);
        PortfolioTitle.transform.DOLocalMoveY(portfolioTitle ? -70 : 150, TRANSITION_SPEED);
    }

    public void EnablePanels(bool TopPanelEnabled, bool MarketsScrollViewEnabled, bool TrendingPanelEnabled, bool SearchPanelEnabled, bool CoinInfoPanelEnabled, bool PortfolioPanelEnabled, bool AddCoinWindowEnabled = false, bool PortfolioCoinMenu = false, bool CoinDescription = false) {
        PortfolioPanel.SetActive(PortfolioPanelEnabled);
        SearchPanel.SetActive(SearchPanelEnabled);
        TrendingPanel.SetActive(TrendingPanelEnabled);
        TopPanel.SetActive(TopPanelEnabled);
        MarketsScrollView.SetActive(MarketsScrollViewEnabled);
        CoinInfoPanel.SetActive(CoinInfoPanelEnabled);
        AddCoinWindow.SetActive(AddCoinWindowEnabled);
        PortfolioCoinMenuPanel.SetActive(PortfolioCoinMenu);
        CoinDescriptionPanel.SetActive(CoinDescription);
    }

    private void SetActiveButton(int index) {
        switch(index) {
            case 0:
                HomeButton.color = accentColor;
                PortfolioButton.color = disabledColor;
                MiscButton.color = disabledColor;
                break;
            case 1:
                HomeButton.color = disabledColor;
                PortfolioButton.color = accentColor;
                MiscButton.color = disabledColor;
                break;
            case 2:
                HomeButton.color = disabledColor;
                PortfolioButton.color = disabledColor;
                MiscButton.color = accentColor;
                break;
        }
    }

}
