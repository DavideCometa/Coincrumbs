using UnityEngine;
using System;
using System.Collections.Generic;

public class DataStructures : MonoBehaviour {
    [Serializable]
    public struct Coin {
        public string id;
        public string symbol;
        public string name;
        public string image;
        public double current_price;
        public string market_cap;
        public int market_cap_rank;
        public int fully_diluted_valuation;
        public int total_volume;
        public double high_24h;
        public double low_24h;
        public double price_change_24h;
        public double price_change_percentage_24h;
        public int market_cap_change_24h;
        public double circulating_supply;
        public double total_supply;
        public double max_supply;
        public double ath;
        public double ath_change_percentage;
        public string ath_date;
        public double atl;
        public double atl_change_percentage;
        public string atl_date;
        public string roi;
        public string last_updated;

        public Coin(string id, string name, string symbol, string image, double currPrice, double price_change_24) {
            this.id = id;
            this.symbol = symbol;
            this.name = name;
            this.image = image;
            this.current_price = currPrice;
            market_cap = null;
            market_cap_rank = 0;
            fully_diluted_valuation = 0;
            total_volume = 0;
            high_24h = 0;
            low_24h = 0;
            price_change_24h = 0;
            this.price_change_percentage_24h = price_change_24;
            market_cap_change_24h = 0;
            circulating_supply = 0;
            total_supply = 0;
            max_supply = 0;
            ath = 0;
            ath_change_percentage = 0;
            ath_date = null;
            atl = 0;
            atl_change_percentage = 0;
            atl_date = null;
            roi = null;
            last_updated = null;
        }
    }

    [Serializable]
    public class CoinPriceHistory {
        public List<Tuple<int, double>> priceData;

        public CoinPriceHistory() {
            priceData = new List<Tuple<int, double>>();
        }

        public CoinPriceHistory(List<Tuple<int,double>> t) {
            priceData = t;
        }

        public void AddTuple(int t, double p) {
            priceData.Add(new Tuple<int,double>(t,p));
        }
    }

    [Serializable]
    public struct CoinID {
        public string id;
        public string symbol;
        public string name;
    }

    public class PortfolioCoin {

        public PortfolioCoin(string coinID, string coinName, double amount = 0.0f) {
            this.coinID = coinID;
            this.coinName = coinName;
            this.amount = amount;
        }

        public string coinID;
        public string coinName;
        public double amount;
        Dictionary<string, object> history;

    }


    public class CoinDetails {
        public string id { get; set; }
        public string symbol { get; set; }
        public string name { get; set; }
        public string asset_platform_id { get; set; }
        public double price { get; set; }
        public double market_cap { get; set; }
        public int block_time_in_minutes { get; set; }
        //public object hashing_algorithm { get; set; }
        //public object[] categories { get; set; }
        public object public_notice { get; set; }
        public string additional_notices { get; set; }
        //public Localization localization { get; set; }
        public string description { get; set; }
        //public Links links { get; set; }
        //public Image image { get; set; }
        public string image { get; set; }
        public string country_origin { get; set; }
        //public object genesis_date { get; set; }
        public string contract_address { get; set; }
        public string sentiment_votes_up_percentage { get; set; }
        public string sentiment_votes_down_percentage { get; set; }
        public int market_cap_rank { get; set; }
        public double price_change_percentage_24h { get; set; }
        public double price_change_percentage_7d { get; set; }
        public double price_change_percentage_14d { get; set; }
        public double price_change_percentage_30d { get; set; }
        public double price_change_percentage_60d { get; set; }
        public double price_change_percentage_200d { get; set; }
        public double price_change_percentage_1y { get; set; }
        public double high24 { get; set; }
        public double low24 { get; set; }
        public double ath { get; set; }
        public double ath_change_percentage { get; set; }
        public string ath_date { get; set; }
        public double atl { get; set; }
        public double atl_change_percentage { get; set; }
        public string atl_date { get; set; }
        public double total_volume { get; set; }
        public string total_supply { get; set; }
        public string circulating_supply { get; set; }



    }
    
}
