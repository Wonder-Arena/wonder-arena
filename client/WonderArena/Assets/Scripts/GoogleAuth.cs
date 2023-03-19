//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;
//using GooglePlayGames;
//using GooglePlayGames.BasicApi;
//using GooglePlayGames.BasicApi.SavedGame;

//public class GoogleAuth : MonoBehaviour
//{
//    private bool isSignedIn = false;

//    public string clientId; // add a public string field to hold the client ID

//    public void SignIn()
//    {
//        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
//            .RequestIdToken()
//            .RequestEmail()
//            .SetClientId(clientId) // set the client ID here
//            .Build();   
//        PlayGamesPlatform.InitializeInstance(config);
//        PlayGamesPlatform.Activate();
//        Social.localUser.Authenticate((bool success) => {
//            isSignedIn = success;
//            if (success)
//            {
//                Debug.Log("Authentication successful!");
//                Debug.Log("Username: " + Social.localUser.userName);
//                Debug.Log("User ID: " + Social.localUser.id);
//                Debug.Log("Token: " + PlayGamesPlatform.Instance.GetIdToken());
//            }
//            else
//            {
//                Debug.Log("Authentication failed.");
//            }
//        });
//    }
//}
