using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AwesomeCharts;

public class PortfolioManager : MonoBehaviour
{
    public MainPageManager mainPageManager;
    public WebDataCrawler wdCrawler;
    UserData userData;

    public GameObject portfolioCoinPrefab;
    public GameObject portfolioSv;
    public GameObject transactionWindow;
    public GameObject pieChartWindow;
    public GameObject buyContainer;
    public GameObject sellContainer;
    public GameObject confirmationContainer;
    public GameObject coinMenuContainer;

    public GameObject portfolioTip;

    public Button buySelector;
    public Button sellSelector;

    public Button buyConfirm;
    public Button sellConfirm;

    public Text titleText;

    public TMP_InputField buyPrice;
    public TMP_InputField buyQuantity;
    public TMP_InputField buyDate;

    public TMP_InputField sellPrice;
    public TMP_InputField sellQuantity;
    public TMP_InputField sellDate;

    public Text totalSpent;
    public Text totalSold;

    public TextMeshProUGUI portfolioValueTxt;

    GameObject addingTransactionTo;
    PortfolioCoinData selectedCoinData;
    GameObject selectedCoinCard;

    public PieChart pieChart;
    PieDataSet pieDataSet;
    Dictionary<string, PieEntry> pieEntryDict = new Dictionary<string, PieEntry>();

    float tempPortfolioValue = 0f;
    float smoothSpeed = 0.0f;

    float currentTime = 0f;

    public Color[] pieChartColors;
    int colorIndex = 0;

    const float TIME_BETWEEN_UPDATES = 100f;

    void Awake() {
        userData = UserData.GetInstance;
    }

    void Start() {

        pieDataSet = new PieDataSet();

        if (userData.portfolio != null && userData.portfolio.Count != 0) {
            portfolioTip.SetActive(false);
            foreach (Dictionary<string, object> c in userData.portfolio.Values) {
                InstantiatePortfolioCoin(c);
            } 
        } else {
            Debug.Log("There is no data in portfolio.");
        }

        Debug.Log("[PORTFOLIO] onStart");

    }

    void Update() {
        if (tempPortfolioValue != userData.portfolioValue) {
            tempPortfolioValue = Mathf.SmoothDamp(tempPortfolioValue, (float) userData.portfolioValue, ref smoothSpeed, 0.8f);
            portfolioValueTxt.text = Math.Round(tempPortfolioValue, 2).ToString("F2") + " <size=40> USD </size>";
        }

        currentTime += Time.deltaTime;
        if(currentTime > TIME_BETWEEN_UPDATES) {
            UpdatePortfolioCoinPrice();
            currentTime = 0f;
        }

    }

    public void AddCoin(string coinID, string coinName, string symbol) {

        portfolioTip.SetActive(false);

        bool coinAdded = false;

        Dictionary<string, object> portfolioCoin = new Dictionary<string, object>();
        portfolioCoin.Add("coinID", coinID);
        portfolioCoin.Add("coinName", coinName);
        portfolioCoin.Add("symbol", symbol);
        portfolioCoin.Add("Amount", "0.00");

        if (userData.portfolio != null) {
            if(!userData.portfolio.ContainsKey(coinID)) {            
                userData.portfolio.Add(coinID, portfolioCoin);
                userData.SaveUserData();
                coinAdded = true;
            } else {
                Debug.Log("Key is already in portfolio");
                //Show message
            }
        } else {
            userData.portfolio = new Dictionary<string, object>();
            userData.portfolio.Add(coinID, portfolioCoin);
            userData.SaveUserData();
            coinAdded = true;
        }

        if (coinAdded) {
            InstantiatePortfolioCoin(portfolioCoin);
        }
    }

    public void RemoveCoin(string coinID) {
        if (userData.portfolio != null) {
            if (!userData.portfolio.ContainsKey(coinID)) {
                Debug.Log("Coin is not in portfolio");
            } else {
                userData.portfolio.Remove(coinID);
                userData.SaveUserData();
            }
        }
    }

    public void InstantiatePortfolioCoin(Dictionary<string, object> c) {
        GameObject portfolioCard = Instantiate(portfolioCoinPrefab);
        portfolioCard.transform.SetParent(portfolioSv.transform);
        SetPortfolioCardData(portfolioCard, c);
        portfolioCard.transform.localScale = new Vector3(1, 1, 1);

        Debug.Log("[PORTFOLIO] Instantiated coin");

    }

    public void UpdatePortfolioCoinPrice() {
        foreach(Transform coinCard in portfolioSv.transform) {

            if (coinCard.gameObject.name == "PortfolioTipText")
                continue;

            PortfolioCoinData pData = coinCard.transform.GetComponent<PortfolioCoinData>();
            Transform[] fields = { coinCard.GetChild(1), coinCard.GetChild(3) };
            StartCoroutine(wdCrawler.UpdateCoinPriceForPortfolio(pData.id, "https://api.coingecko.com/api/v3/simple/price?ids=" + pData.id + "&vs_currencies=usd&include_24hr_change=true&include_last_updated_at=true", fields, pData.amount, pData.symbol));
        }

        Debug.Log("[PORTFOLIO] Coin prices updated");

    }

    public void SetPortfolioCardData(GameObject portfolioCard, Dictionary<string, object> c = null, PortfolioCoinData pData = null) {
        object name;
        object id;
        object symbol;
        object amount;
        if (pData == null && c != null) {

            c.TryGetValue("coinID", out id);
            c.TryGetValue("coinName", out name);
            c.TryGetValue("symbol", out symbol);
            c.TryGetValue("Amount", out amount);

            pData = portfolioCard.GetComponent<PortfolioCoinData>();
            pData.name = name != null ? name.ToString() : " ";
            pData.id = id.ToString();
            pData.symbol = symbol != null ? symbol.ToString().ToUpper() : " ";
            pData.amount = float.Parse(amount.ToString());
        } else {
            name = pData.name;
            symbol = pData.symbol;
            id = pData.id;
            amount = pData.amount;
        }
        portfolioCard.transform.GetChild(0).GetComponent<Text>().text = pData.name;
        Transform[] fields = { portfolioCard.transform.GetChild(1), portfolioCard.transform.GetChild(3) };
        StartCoroutine(wdCrawler.UpdateCoinPriceForPortfolio(pData.id, "https://api.coingecko.com/api/v3/simple/price?ids=" + pData.id + "&vs_currencies=usd&include_24hr_change=true&include_last_updated_at=true", fields, pData.amount, pData.symbol));

        portfolioCard.transform.GetChild(4).GetComponent<Text>().text = amount + " " + pData.symbol;
        portfolioCard.transform.GetChild(5).GetComponent<Button>().onClick.AddListener(() => AddTransaction(pData, portfolioCard));
        portfolioCard.transform.GetComponent<Button>().onClick.AddListener(() => ShowPortfolioCoinMenu(pData, portfolioCard));

        pData.card = portfolioCard;

        StartCoroutine(wdCrawler.LoadImageCoroutine(pData.id, null, portfolioCard.transform.GetChild(2).gameObject));
    }

    public void AddCoinToPieChart(string name, float value) {

        if (colorIndex == pieChartColors.Length)
            colorIndex = 1;

        PieEntry pE = new PieEntry(value, name, pieChartColors[colorIndex]);

        if (pieEntryDict.ContainsKey(name)) {
            pieEntryDict.TryGetValue(name, out pE);
            
            List<PieEntry> pESet = pieDataSet.Entries;
            int index = pESet.IndexOf(pE);

            pE.Value = value;
            pieEntryDict[name] = pE;

            pieDataSet.RemoveEntry(index);    
            pieDataSet.AddEntry(pE);

        } else {
            pieEntryDict.Add(name, pE);
            pieDataSet.AddEntry(pE);
            colorIndex++;
        }
        

        pieChart.GetChartData().DataSet = pieDataSet;
        pieChart.SetDirty();
        
    }

    public void RemoveCoinFromPieChart(string name) {
        if (pieEntryDict.ContainsKey(name)) {
            PieEntry pE;
            pieEntryDict.TryGetValue(name, out pE);

            if (pE == null)
                return;

            List<PieEntry> pESet = pieDataSet.Entries;
            int index = pESet.IndexOf(pE);
            pieDataSet.RemoveEntry(index);
            
        }

        pieChart.GetChartData().DataSet = pieDataSet;
        pieChart.SetDirty();

    }

    public void ShowPortfolioCoinMenu(PortfolioCoinData pData, GameObject card) {
        selectedCoinData = pData;
        selectedCoinCard = card;
        confirmationContainer.SetActive(false);
        coinMenuContainer.SetActive(true);
        mainPageManager.JumpToSection(MainPageManager.Sections.PortfolioCoinMenu);
    }

    public void AddTransaction(PortfolioCoinData pData, GameObject card) {

        DateTime currDate = DateTime.Now;

        addingTransactionTo = card;
        selectedCoinData = pData;

        transactionWindow.SetActive(true);
        buyContainer.SetActive(true);
        sellContainer.SetActive(false);
        buyConfirm.interactable = false;
        sellConfirm.interactable = false;

        buySelector.GetComponent<Image>().color = mainPageManager.accentColor;
        sellSelector.GetComponent<Image>().color = mainPageManager.disabledColor;
        mainPageManager.JumpToSection(MainPageManager.Sections.AddCoinPortfolio);

        titleText.text = "New transaction for " + pData.name + " (" + pData.symbol + ")";
        buyPrice.text = Utils.ReturnFormattedPrice(pData.price);
        buyQuantity.text = "0";
        buyDate.text = currDate.Day + "-" + currDate.Month + "-" + currDate.Year;

        sellPrice.text = Utils.ReturnFormattedPrice(pData.price);
        sellQuantity.text = "0";
        sellDate.text = currDate.Day + "-" + currDate.Month + "-" + currDate.Year;
    }

    public void OnSellSelectorPressed() {
        buySelector.GetComponent<Image>().color = mainPageManager.disabledColor;
        sellSelector.GetComponent<Image>().color = mainPageManager.accentColor;
        buyContainer.SetActive(false);
        sellContainer.SetActive(true);
    }

    public void OnBuytSelectorPressed() {
        buySelector.GetComponent<Image>().color = mainPageManager.accentColor;
        sellSelector.GetComponent<Image>().color = mainPageManager.disabledColor;
        buyContainer.SetActive(true);
        sellContainer.SetActive(false);
    }

    public void OnBuyConfirm() {
        //Write transaction in coin transaction history
        selectedCoinData.amount += Utils.ParseInvariantCulture(buyQuantity.text);
        SetPortfolioCardData(addingTransactionTo, null, selectedCoinData);

        UpdateCoin();
    }

    public void OnSellConfirm() {
        //Write transaction in coin transaction history
        selectedCoinData.amount -= Utils.ParseInvariantCulture(sellQuantity.text);
        SetPortfolioCardData(addingTransactionTo, null, selectedCoinData);

        UpdateCoin();
    }

    public void UpdateCoin() {
        Dictionary<string, object> portfolioCoin = new Dictionary<string, object>();
        portfolioCoin.Add("coinID", selectedCoinData.id);
        portfolioCoin.Add("coinName", selectedCoinData.name);
        portfolioCoin.Add("symbol", selectedCoinData.symbol);
        portfolioCoin.Add("Amount", selectedCoinData.amount.ToString());

        userData.portfolio[selectedCoinData.id] = portfolioCoin;
        userData.SaveUserData();

        mainPageManager.JumpToSection(MainPageManager.Sections.PortfolioPage);

        mainPageManager.ShowToastMessage(true, "Transaction added!");
    }

    public void OnInputFieldEditEnd() {

        if(buyContainer.activeSelf) {
            if (buyPrice.text.Length < 1) {
                buyConfirm.interactable = false;
                buyPrice.text = selectedCoinData.price.ToString();
                totalSpent.text = "0.00 USD";
                return;
            }
            if (buyQuantity.text.Length < 1 || buyQuantity.text == "0") {
                buyConfirm.interactable = false;
                totalSpent.text = "0.00 USD";
                return;
            }
            buyConfirm.interactable = true;
            float bPrice = Utils.ParseInvariantCulture(buyPrice.text.TrimEnd(' ', '$').Replace(",",".")) * Utils.ParseInvariantCulture(buyQuantity.text);
            totalSpent.text = Utils.ReturnFormattedPrice(Math.Round(bPrice, 2));
        }

        if (sellContainer.activeSelf) {
            if (sellPrice.text.Length < 1) {
                sellConfirm.interactable = false;
                sellPrice.text = selectedCoinData.price.ToString();
                totalSold.text = "0.00 USD";
                return;
            }
            if (sellQuantity.text.Length < 1 || sellQuantity.text == "0") {
                sellConfirm.interactable = false;
                totalSold.text = "0.00 USD";
                return;
            }
            sellConfirm.interactable = true;  
            float sPrice = Utils.ParseInvariantCulture(sellPrice.text.TrimEnd(' ', '$').Replace(",", ".")) * Utils.ParseInvariantCulture(sellQuantity.text);
            totalSold.text = Utils.ReturnFormattedPrice(Math.Round(sPrice, 2));
        }

    }

    public void OnInputFieldEdit() {

        if (buyContainer.activeSelf) { 
            if(buyPrice.text.Split('.').Length-1 > 1) {
                buyPrice.text = buyPrice.text.Substring(0, buyPrice.text.Length - 1);
            }

            ///buyPrice.text = Utils.ReturnFormattedPrice(double.Parse(buyPrice.text));
        }

        if (sellContainer.activeSelf) {
            if (sellPrice.text.Split('.').Length - 1 > 1) {
                sellPrice.text = sellPrice.text.Substring(0, sellPrice.text.Length - 1);
            }

            //sellPrice.text = Utils.ReturnFormattedPrice(double.Parse(sellPrice.text));
        }



    }

    public void OnSellQuantityInputFieldEditEnd() {
        if (Utils.ParseInvariantCulture(sellQuantity.text) > selectedCoinData.amount) {
            sellConfirm.interactable = false;
            totalSold.text = "0.00 USD";
            return;
        }

        sellConfirm.interactable = true;
        float sPrice = Utils.ParseInvariantCulture(sellPrice.text.TrimEnd(' ', '$').Replace(",", ".")) * Utils.ParseInvariantCulture(sellQuantity.text);
        totalSold.text = Utils.ReturnFormattedPrice(Math.Round(sPrice, 2));
        
    }

    public void OnRemoveCoin() {
        mainPageManager.JumpToSection(MainPageManager.Sections.ConfirmRemoveCoinPanel);
        confirmationContainer.SetActive(true);
        coinMenuContainer.SetActive(false);
    }

    public void OnYesPress() {
        userData.portfolioValue -= selectedCoinData.value;
        userData.portfolio.Remove(selectedCoinData.id);
        userData.SaveUserData();
        Destroy(selectedCoinData.card);
        mainPageManager.JumpToSection(MainPageManager.Sections.PortfolioPage);
        mainPageManager.ShowToastMessage(true, "Coin removed from portfolio!");

        RemoveCoinFromPieChart(selectedCoinData.name);

        selectedCoinData = null;

        if (userData.portfolio != null && userData.portfolio.Count == 0) {
            portfolioTip.SetActive(true);
        }
    }

    public void OnNoPress() {
        confirmationContainer.SetActive(false);
        coinMenuContainer.SetActive(true);
        mainPageManager.OnBackTap();
    }

    public void OnShowCoinDetail() {
        coinMenuContainer.SetActive(false);
        mainPageManager.OnCoinTap(selectedCoinData.id, selectedCoinData.name, selectedCoinData.symbol);
    }

    public void OnAddTransactionFromMenu() {
        AddTransaction(selectedCoinData, selectedCoinCard);
    }

    public void OnPieChartTap() {
        pieChartWindow.SetActive(true);
    }

    public void OnExitPieChart() {
        pieChartWindow.SetActive(false);
    }

}
