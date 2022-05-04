using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;
using SimpleJSON;
using MiniJSON;
using System.Collections.Generic;
using UnityEngine.UI;
using AwesomeCharts;

public class WebDataCrawler : MonoBehaviour {

    public const double MAX_CHART_SAMPLES = 300f;

    const string cryptonews_API_key = "627caed51d1ac2b485a361d4c6cb1daa17355d24";

    public MainPageManager mainPageManager;
    public PortfolioManager portfolioManager;
    private UserData user;

    DataStructures.Coin[] topCoins = { }; //Top 100 coins
    DataStructures.Coin[] trendingCoins = { new DataStructures.Coin(), new DataStructures.Coin(), new DataStructures.Coin(), new DataStructures.Coin(), new DataStructures.Coin(), new DataStructures.Coin(), new DataStructures.Coin(), new DataStructures.Coin(), new DataStructures.Coin(), new DataStructures.Coin() };

    public Dictionary<string, DataStructures.CoinDetails> coinDetails = new Dictionary<string, DataStructures.CoinDetails>();

    void Start() {
        user = UserData.GetInstance;

        TestNetwork();
 
    }

    public void TestNetwork() {
        StartCoroutine(PingCoinGecko("https://api.coingecko.com/api/v3/ping"));
    }

    public void RetrieveMainMarketsData() {
        StartCoroutine(GetMainMarketsData("https://api.coingecko.com/api/v3/coins/markets?vs_currency=usd"));
        RetrieveCoinsList();
    }

    public void RetrieveGlobalData() {
        StartCoroutine(GetGlobalData("https://api.coingecko.com/api/v3/global"));
    }

    public void RetrieveTrendingCoins() {
        StartCoroutine(GetTrendingCoins("https://api.coingecko.com/api/v3/search/trending"));
    }

    public void RetrieveCoinsList() {
        StartCoroutine(GetCoinsList("https://api.coingecko.com/api/v3/coins/list"));
    }

    public void RetrieveCoinData(string id, bool tryLoadImage = false, Func<Coroutine> callback = null) {
        StartCoroutine(GetCoinInfo("https://api.coingecko.com/api/v3/coins/" + id, tryLoadImage, callback));
    }
    public void RetrieveCoinHistory(string id, int range) {
        StartCoroutine(GetCoinHistory("https://api.coingecko.com/api/v3/coins/" + id + "/market_chart?vs_currency=usd&days=" + ConvertRangeToDays(range)));
    }

    public void RetrieveSimplePrice(string id, Transform[] fields, double amount, string symbol) {
        StartCoroutine(UpdateCoinPriceForPortfolio(id, "https://api.coingecko.com/api/v3/simple/price?ids=" + id + "&vs_currencies=usd&include_24hr_change=true&include_last_updated_at=true", fields, amount, symbol));
    }

    public void RetrieveCoinsSimplePrice(string IDs, List<GameObject> cards, string names) {
        StartCoroutine(GetCoinsSimplePrice("https://api.coingecko.com/api/v3/simple/price?ids=" + IDs + "&vs_currencies=usd&include_24hr_change=true&include_market_cap=true", IDs, cards, names));
    }

    public void RetrieveHotNews() {
        StartCoroutine(GetHotNews("https://cryptopanic.com/api/v1/posts/?auth_token="+ cryptonews_API_key +"&filter=hot"));
    }

    public void RetrieveCoinNews(string symbol) {
        StartCoroutine(GetHotNews("https://cryptopanic.com/api/v1/posts/?auth_token=" + cryptonews_API_key + "&currencies=" + symbol.ToUpper() + "&kind=news"));
    }

    IEnumerator GetMainMarketsData(string uri) {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri)) {
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            if (webRequest.isNetworkError) {
                Debug.Log(pages[page] + ": Error: " + webRequest.error);
            } else {
                //Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                topCoins = JsonHelper.GetArray<DataStructures.Coin>(webRequest.downloadHandler.text);
                mainPageManager.UpdateCoinsData(topCoins);

                user.startupPhase++;
            }
        }
    }

    IEnumerator PingCoinGecko(string uri) {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri)) {
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            if (webRequest.isNetworkError) {
                Debug.Log(pages[page] + ": Error: " + webRequest.error);
            } else {
                var N = JSON.Parse(webRequest.downloadHandler.text);
                string result = N["gecko_says"].ToString();

                if(result == "\"(V3) To the Moon!\"") {
                    RetrieveMainMarketsData();
                    RetrieveHotNews();
                    RetrieveTrendingCoins();
                    RetrieveGlobalData();
                } else {
                    Debug.Log("No connection!");
                    mainPageManager.OpenNoDataAvailablePanel();
                }

            }
        }
    }

    

    IEnumerator GetTrendingCoins(string uri) {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri)) {
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            if (webRequest.isNetworkError) {
                Debug.Log(pages[page] + ": Error: " + webRequest.error);
            } else {
                Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);

                var N = JSON.Parse(webRequest.downloadHandler.text);

                int i;
                for (i=0; i<3; i++) {
                    var coinData = N["coins"][i]["item"];
                    StartCoroutine(GetCoinInfo("https://api.coingecko.com/api/v3/coins/" + coinData["id"]));
                    DataStructures.Coin c = new DataStructures.Coin(coinData["id"], coinData["name"], coinData["symbol"], coinData["thumb"], 0f, 0f);
                    trendingCoins[i] = c;
                }
                for (int j = i; j < 7; j++) {
                    var coinData = N["coins"][j]["item"];
                    DataStructures.Coin c = new DataStructures.Coin(coinData["id"], coinData["name"], coinData["symbol"], coinData["thumb"], 0f, 0f);
                    trendingCoins[j] = c;
                }


                mainPageManager.UpdateTrendingCoins(trendingCoins);

                user.startupPhase++;
            }
        }
    }

    IEnumerator GetCoinsList(string uri) {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri)) {

            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            if (webRequest.isNetworkError) {
                Debug.Log(pages[page] + ": Error: " + webRequest.error);
            } else {
                //Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);

                mainPageManager.UpdateCoinsList(JsonHelper.GetArray<DataStructures.Coin>(webRequest.downloadHandler.text));
            }
        }
    }

    IEnumerator GetGlobalData(string uri) {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri)) {

            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            if (webRequest.isNetworkError) {
                Debug.Log(pages[page] + ": Error: " + webRequest.error);
            } else {
                Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);

                var dict = Json.Deserialize(webRequest.downloadHandler.text) as Dictionary<string, object>;
                Dictionary<string, object> marketData = dict["data"] as Dictionary<string, object>;
                double totMarketCap = (double)((dict["data"] as Dictionary<string, object>)["total_market_cap"] as Dictionary<string, object>)["usd"];
                double marketCapChange = (double)(dict["data"] as Dictionary<string, object>)["market_cap_change_percentage_24h_usd"];

                mainPageManager.updateGlobalData(totMarketCap, marketCapChange);

                user.startupPhase++;
            }
        }
    }

    IEnumerator GetCoinHistory(string uri) {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri)) {

            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            if (webRequest.isNetworkError) {
                Debug.Log(pages[page] + ": Error: " + webRequest.error);
            } else {
                Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);

                var N = JSON.Parse(webRequest.downloadHandler.text);
                var pricesArray = N["prices"];
                var volumesArray = N["total_volumes"];

                bool lastVal = false;
                int samplesNum = pricesArray.Count;
                int samplesSpan = Mathf.CeilToInt((float) samplesNum / (float) MAX_CHART_SAMPLES);

                var i = 0;
                LineDataSet set = new LineDataSet();
                BarDataSet volumesSet = new BarDataSet();
                double minValue = double.MaxValue, maxValue = double.MinValue;
                while (pricesArray[i] != null) {
                    //priceHistory.AddTuple(pricesArray[i][0], pricesArray[i][1]);
                    double currPrice = pricesArray[i][1].AsDouble;
                    if (currPrice < minValue)
                        minValue = currPrice;
                    if (currPrice > maxValue)
                        maxValue = currPrice;

                    long unixTimeStamp = pricesArray[i][0].AsLong;
                    DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    dtDateTime = dtDateTime.AddMilliseconds(unixTimeStamp).ToLocalTime();
                    string dateString = dtDateTime.Day.ToString("00") + "/" + dtDateTime.Month.ToString("00") + " " + dtDateTime.Hour.ToString("00") + ":" + dtDateTime.Minute.ToString("00");
                    double volume = volumesArray[i][1].AsLong;
                    string volumeString = Utils.ReturnVolumeString(volumesArray[i][1].AsLong);



                    set.AddEntry(new LineEntry(float.Parse(i.ToString()), (float) currPrice, dateString+"&"+ volumeString));
                    volumesSet.AddEntry(new BarEntry(long.Parse(i.ToString()), Mathf.RoundToInt((float) volume / 100000)));

                    if (i + samplesSpan > samplesNum - 1) {
                        if (lastVal)
                            break;

                        i = samplesNum - 1;
                        lastVal = true;
                    }
                    else
                        i += samplesSpan;

                }

                //i - total number of entries
                string[] horizontal_values = {"", "", "", "", "", "" };
                int step = Mathf.RoundToInt(i / 5);
                var index = 0;
                DateTime currDate = DateTime.Now;
                for (int j = 0; j < 6; j++) {

                   while(pricesArray[index] == null)
                        index--;

                    long unixTimeStamp = pricesArray[index][0].AsLong;
                    DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    dtDateTime = dtDateTime.AddMilliseconds(unixTimeStamp).ToLocalTime();

                    if(user.historyRange < 2 )
                        horizontal_values[j] = dtDateTime.Day.ToString("00") + "/" + dtDateTime.Month.ToString("00") + " " + dtDateTime.Hour.ToString("00") + ":" + dtDateTime.Minute.ToString("00");
                    else
                        horizontal_values[j] = dtDateTime.Day.ToString("00") + "/" + dtDateTime.Month.ToString("00") + " " + dtDateTime.Year.ToString("00");

                    index += step;
                }

                mainPageManager.UpdateChartData(set, minValue, maxValue, horizontal_values, volumesSet);

            }
        }
    }

    

    IEnumerator GetCoinInfo(string uri, bool tryLoadImage = false, Func<Coroutine> callback = null) {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri)) {

            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            if (webRequest.isNetworkError) {
                Debug.Log(pages[page] + ": Error: " + webRequest.error);
            } else {
                Debug.Log(pages[page] + ":\nGET COIN INFO RECEIVED: " + webRequest.downloadHandler.text);

                DataStructures.CoinDetails coin = new DataStructures.CoinDetails();
                var N = JSON.Parse(webRequest.downloadHandler.text);
                //var market_data = N["market_data"];
                var dict = Json.Deserialize(webRequest.downloadHandler.text) as Dictionary<string, object>;
                Dictionary<string, object> marketData = dict["market_data"] as Dictionary<string, object>;
                Dictionary<string, object> current_price = marketData["current_price"] as Dictionary<string, object>;
                Dictionary<string, object> market_cap = marketData["market_cap"] as Dictionary<string, object>;
                /*var root = Json.Deserialize(webRequest.downloadHandler.text) as Dictionary<string, object>;
                Dictionary<string, object> market_data = root["market_data"] as Dictionary<string, object>;*/

                coin.id = (string) dict["id"];
                coin.name = (string) dict["name"];
                coin.symbol = (string) dict["symbol"];
                
                coin.price = double.Parse(current_price["usd"].ToString());
                coin.market_cap = double.Parse(market_cap["usd"].ToString());
                coin.asset_platform_id = (string)dict["asset_platform_id"];
                coin.block_time_in_minutes = int.Parse(dict["block_time_in_minutes"]?.ToString() ?? "N/A");
                coin.image = N["image"]["small"];
                coin.country_origin = (string)dict["country_origin"];
                coin.sentiment_votes_up_percentage = dict["sentiment_votes_up_percentage"]?.ToString() ?? "N/A";
                coin.sentiment_votes_down_percentage = dict["sentiment_votes_down_percentage"]?.ToString() ?? "N/A";
                coin.market_cap_rank = int.Parse(dict["market_cap_rank"]?.ToString() ?? "0");

                coin.price_change_percentage_24h = double.Parse(marketData["price_change_percentage_24h"]?.ToString() ?? "N/A");
                coin.price_change_percentage_7d = double.Parse(marketData["price_change_percentage_7d"]?.ToString() ?? "N/A");
                coin.price_change_percentage_14d = double.Parse(marketData["price_change_percentage_14d"]?.ToString() ?? "N/A");
                coin.price_change_percentage_30d = double.Parse(marketData["price_change_percentage_30d"]?.ToString() ?? "N/A");
                coin.price_change_percentage_60d = double.Parse(marketData["price_change_percentage_60d"]?.ToString() ?? "N/A");
                coin.price_change_percentage_200d = double.Parse(marketData["price_change_percentage_200d"]?.ToString() ?? "N/A");
                coin.price_change_percentage_1y = double.Parse(marketData["price_change_percentage_1y"]?.ToString() ?? "N/A");

                coin.ath = double.Parse((marketData["ath"] as Dictionary<string, object>)["usd"]?.ToString() ?? "0");
                coin.atl = double.Parse((marketData["atl"] as Dictionary<string, object>)["usd"]?.ToString() ?? "0");
                coin.ath_change_percentage = double.Parse((marketData["ath_change_percentage"] as Dictionary<string, object>)["usd"]?.ToString() ?? "N/A");
                coin.atl_change_percentage = double.Parse((marketData["atl_change_percentage"] as Dictionary<string, object>)["usd"]?.ToString() ?? "N/A");
                coin.ath_date = (marketData["ath_date"] as Dictionary<string, object>)["usd"].ToString();
                coin.atl_date = (marketData["atl_date"] as Dictionary<string, object>)["usd"].ToString();

                coin.total_supply = marketData["total_supply"]?.ToString() ?? "Unlimited";
                coin.circulating_supply = marketData["circulating_supply"]?.ToString() ?? "N/A";

                coin.total_volume = double.Parse((marketData["total_volume"] as Dictionary<string, object>)["usd"]?.ToString() ?? "N/A");

                coin.description = Utils.RemoveRichText((string)((Dictionary<string, object>)dict["description"])["en"]);

                coin.high24 = ((Dictionary<string, object>)marketData["high_24h"]).ContainsKey("usd") ? double.Parse(((Dictionary<string, object>)marketData["high_24h"])["usd"].ToString()) : 0;
                coin.low24 = ((Dictionary<string, object>)marketData["low_24h"]).ContainsKey("usd") ? double.Parse(((Dictionary<string, object>)marketData["low_24h"])["usd"].ToString()) : 0;

                if (!coinDetails.ContainsKey(coin.id))
                    coinDetails.Add(coin.id, coin);
                else
                    coinDetails[coin.id] = coin;

                if (tryLoadImage) {
                    callback.Invoke();
                } else {
                    string id = N["id"];
                    string name = N["name"];
                    string symbol = N["symbol"];
                    string image_url = N["image"]["large"];

                    double price = (double)current_price["usd"];

                    double price_change_24 = (double)marketData["price_change_percentage_24h"];

                    mainPageManager.addCardToTrendingCoins(id, name, image_url, price, price_change_24, symbol);
                }

            }
        }
    }

    public IEnumerator LoadImageCoroutine(string id, string url, GameObject container) {

        //print(Application.persistentDataPath);

        if (System.IO.File.Exists(Application.persistentDataPath + "/" + id + ".png")) {
            //var cached = (new UnityWebRequest("file:///" + Application.persistentDataPath + "/" + coinName + ".png")).texture;
            var cached = System.IO.File.ReadAllBytes(Application.persistentDataPath + "/" + id + ".png");
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(cached);
            container.GetComponent<RawImage>().texture = texture;
            yield return null;
        } else {

            container.GetComponent<RawImage>().texture = mainPageManager.imagePlaceholder;

            if (url == null) {
                DataStructures.CoinDetails coin = new DataStructures.CoinDetails();
                if (coinDetails.ContainsKey(id)) {
                    coinDetails.TryGetValue(id, out coin);
                    StartCoroutine(LoadImageCoroutine(id, coin.image, container));
                } else {
                    StartCoroutine(GetCoinInfo("https://api.coingecko.com/api/v3/coins/" + id, true, () => StartCoroutine(LoadImageCoroutine(id, coin.image, container))));
                }

            } else {

                WWW wwwLoader = new WWW(url);   // create WWW object pointing to the url
                yield return wwwLoader;         // start loading whatever in that url ( delay happens here )

                if (wwwLoader.bytes != null) {
                    System.IO.File.WriteAllBytes(Application.persistentDataPath + "/" + id + ".png", wwwLoader.bytes);
                }

                container.GetComponent<RawImage>().texture = wwwLoader.texture;
            }
        }

    }

    public IEnumerator UpdateCoinPriceForPortfolio(string id, string uri, Transform[] fields, double amount, string symbol) {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri)) {

            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            if (webRequest.isNetworkError) {
                Debug.Log(pages[page] + ": Error: " + webRequest.error);
            } else {
                Debug.Log(pages[page] + ":\nGET COIN SIMPLE PRICE RECEIVED: " + webRequest.downloadHandler.text);

                var N = Json.Deserialize(webRequest.downloadHandler.text) as Dictionary<string, object>;
                var data = N[id] as Dictionary<string, object>;
                var price = double.Parse(data["usd"].ToString());
                var price_change_24H = double.Parse(data["usd_24h_change"].ToString());
                var last_update_at = double.Parse(data["last_updated_at"].ToString());

                if (price_change_24H > 0) {
                    fields[0].GetChild(2).gameObject.SetActive(false);
                } else {
                    fields[0].GetChild(3).gameObject.SetActive(false);
                }

                PortfolioCoinData pData = fields[0].parent.GetComponent<PortfolioCoinData>();

                double value = (amount * price);
                user.portfolioValue -= pData.value;
                user.portfolioValue += value;

                fields[0].GetChild(0).GetComponent<Text>().text = price + " USD";
                fields[0].GetChild(1).GetChild(0).GetComponent<Text>().text = Math.Round(price_change_24H, 2).ToString() + "%";
                fields[1].GetComponent<Text>().text = value + " USD";

                pData.price = price;
                pData.value = value;

                if(value != 0)
                    portfolioManager.AddCoinToPieChart(pData.name, float.Parse(pData.value.ToString()));
            }
        }
    }

    
    public IEnumerator GetCoinsSimplePrice(string uri, string ids, List<GameObject> cards, string names) {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri)) {

            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            if (webRequest.isNetworkError) {
                Debug.Log(pages[page] + ": Error: " + webRequest.error);
            } else {
                Debug.Log(pages[page] + ":\nGET COIN SIMPLE PRICE RECEIVED: " + webRequest.downloadHandler.text);

                var N = Json.Deserialize(webRequest.downloadHandler.text) as Dictionary<string, object>;
                string[] IDs = ids.Split(',');
                string[] Names = names.Split(',');

                int i = 0;
                foreach(string id in IDs) {

                    if (id == "")
                        continue;

                    var data = N[id] as Dictionary<string, object>;
                    var price = double.Parse(data["usd"].ToString());
                    var price_change_24H = double.Parse(data["usd_24h_change"].ToString());
                    var market_cap = double.Parse(data["usd_market_cap"].ToString());

                    DataStructures.Coin coin = new DataStructures.Coin();
                    coin.id = id;
                    coin.name = Names[i];
                    coin.current_price = price;
                    coin.market_cap = market_cap.ToString();
                    coin.price_change_percentage_24h = price_change_24H;

                    mainPageManager.SetCoinCardData(cards[i], coin, 200);
                    i++;
                }


            }
        }
    }

    public IEnumerator GetHotNews(string uri) {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri)) {

            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            if (webRequest.isNetworkError) {
                Debug.Log(pages[page] + ": Error: " + webRequest.error);
            } else {
                Debug.Log(pages[page] + ":\nGET NEWS RECEIVED: " + webRequest.downloadHandler.text);

                var N = Json.Deserialize(webRequest.downloadHandler.text) as Dictionary<string, object>;
                List<object> results = N["results"] as List<object>;

                int i = 0;
                string[] newsTitles = {"","","","","","","", "", "", "", "","" };
                int newsNumber = 10;
                string prevNews = "";
                while (i<newsNumber && i < results.Count) {
                    string currNews = (results[i] as Dictionary<string, object>)["title"] as string;
                    if (prevNews != "") {
                        if (prevNews == currNews) {
                            newsNumber++;
                            i++;
                            continue;
                        }

                    }
                    newsTitles[i] = "'<i>" + ((results[i] as Dictionary<string,object>)["title"] as string + "</i>' - <size=40><color=#7BFFEF>" + ((results[i] as Dictionary<string, object>)["source"] as Dictionary<string, object>)["title"] as string)+ "</color></size>";
                    prevNews = currNews;
                    i++;
                }            

                mainPageManager.AddNewsToTicker(newsTitles);
            }
        }
    }

    public void ReloadMainMarketsData() {
        Debug.Log("[WB] Reloading main markets data");
        StartCoroutine(GetMainMarketsData("https://api.coingecko.com/api/v3/coins/markets?vs_currency=usd"));
        RetrieveGlobalData();
    }

    private string ConvertRangeToDays(int range) {
        switch (range) {
            case 0:
                return "1";
            case 1:
                return "7";
            case 2:
                return "14";
            case 3:
                return "30";
            case 4:
                return "60";
            case 5:
                return "90";
            case 6:
                return "180";
            case 7:
                return "365";
            case 8:
                return "max";
            default:
                return "1";
        }
    }
}

