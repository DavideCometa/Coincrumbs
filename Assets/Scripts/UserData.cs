using System.Collections.Generic;
using UnityEngine;
using MiniJSON;
using System.IO;

public sealed class UserData {

    UserData() {
        portfolio = new Dictionary<string, object>() { };
        LoadUserData();
    }

    private static UserData instance = null;

    public static UserData GetInstance {
        get {
            if (instance == null) {
                instance = new UserData();
            }
            return instance;
        }
    }

    public Dictionary<string, object> userData = new Dictionary<string, object>();
    public Dictionary<string, object> favorites = new Dictionary<string, object>();
    public Dictionary<string, object> portfolio;

    public Dictionary<string, object> portfolioCoin = new Dictionary<string, object>();

    public string lastCoinId = "";
    public int historyRange = 0;
    public bool addingCoin = false;
    public double portfolioValue = 0f;
    public int startupPhase = 0;

    public MainPageManager.Sections previousSection = MainPageManager.Sections.HomePage;
    public Stack<MainPageManager.Sections> backstack = new Stack<MainPageManager.Sections>();

    public void SaveUserData() {

        userData.Clear();
        userData.Add("favorites", favorites);
        userData.Add("portfolio", portfolio);

        string jsonString = Json.Serialize(userData);
        Debug.Log("SAVE " + jsonString);
        string path = Path.Combine(Application.persistentDataPath, "user.json");
        File.WriteAllText(path, jsonString);
    }

    public void LoadUserData() {

        if(!File.Exists(Application.persistentDataPath + "/user.json")) {
            SaveUserData();
            return;
        }/* else {
            File.Delete(Application.persistentDataPath + "/user.json");
            return;
        }*/
        
        string jsonString = File.ReadAllText(Application.persistentDataPath + "/user.json");
        Debug.Log("LOAD " + jsonString);
        var userData = Json.Deserialize(jsonString) as Dictionary<string, object>;
        favorites = userData["favorites"] as Dictionary<string, object>;
        portfolio = userData["portfolio"] as Dictionary<string, object>;
    }

}
