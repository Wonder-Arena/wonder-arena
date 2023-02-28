using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Util;
using System.IO;
using System.Threading.Tasks;

public class LoginManager : MonoBehaviour
{
    #region Classes
    // Classes for registration
    [System.Serializable]
    public class Registration
    {
        // Data to send to server from user
        [System.Serializable]
        public class RegisterUser
        {
            public string name;
            public string email;
            public string password;
        }

        // Data from server about Register Request
        [System.Serializable]
        public class RegisterResponse
        {
            public bool status;
            public string message;
            public UserData data;
        }

        // Data from server about User
        [System.Serializable]
        public class UserData
        {
            public string name;
            public string email;
            public string password;
            public string accessToken;
        }
    }

    // Classes for login
    [System.Serializable]
    public class Login
    {
        // Data to send to server from user
        [System.Serializable]
        public class LoginUser
        {
            public string email;
            public string password;
        }

        // Data from server about Login Request
        [System.Serializable]
        public class LoginResponse
        {
            public bool status;
            public string message;
            public UserData data;
        }

        // Data from server about User
        [System.Serializable]
        public class UserData
        {
            public int id;
            public string email;
            public string name;
            public bool claimedBBs;
            public string accessToken;
            public FlowAccount flowAccount;
        }

        [System.Serializable]
        public class FlowAccount
        {
            public string address;
        }
    }

    // Here's a data that we get from FrontEnd
    public class UserInputData
    {
        public string name;
        public string email;
        public string password;
    }
    #endregion

    #region Variables
    [Header("All UIs necceseries")]
    [SerializeField] GameObject registrationObject;
    [SerializeField] GameObject loginObject;
    [SerializeField] TMP_Text errorField;
    [SerializeField] GameObject videoStartingScreen;
    [SerializeField] GameObject nonVideoStartingScreen;
    [SerializeField] GameObject loginButtons;
    private bool isVideoClicked;
    public float alphaVelocity;

    //Serialized Fields from front end
    [Header("Input Fields for Registration")]
    [SerializeField] TMP_InputField usernameRegistrationField;
    [SerializeField] TMP_InputField emailRegistrationField;
    [SerializeField] TMP_InputField passwordRegistrationField;
    [SerializeField] TMP_InputField confirmPasswordField;

    [Header("Input Fields for Login")]
    //[SerializeField] TMP_InputField usernameLoginField;
    [SerializeField] TMP_InputField emailLoginField;
    [SerializeField] TMP_InputField passwordLoginField;

    [Header("Json Files")]
    [SerializeField] TextAsset RegisterTxn;
    [SerializeField] TextAsset LoginTxn;
    #endregion

    private void Awake()
    {
        isVideoClicked = false;
        nonVideoStartingScreen.SetActive(false);
        usernameRegistrationField = usernameRegistrationField.GetComponent<TMP_InputField>();
        emailRegistrationField = emailRegistrationField.GetComponent<TMP_InputField>();
        passwordRegistrationField = passwordRegistrationField.GetComponent<TMP_InputField>();
        confirmPasswordField = confirmPasswordField.GetComponent<TMP_InputField>();

        emailLoginField = emailLoginField.GetComponent<TMP_InputField>();
        passwordLoginField = passwordLoginField.GetComponent<TMP_InputField>();

        errorField = errorField.GetComponent<TMP_Text>();
    }


    // Registrating new user
    private IEnumerator Register(string url, string bodyJsonString)
    {
        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJsonString);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();

        Debug.Log("Status Code: " + request.responseCode);
        Debug.Log(request.downloadHandler.text);

        Registration.RegisterResponse registerResponse = JsonUtility.FromJson<Registration.RegisterResponse>(request.downloadHandler.text);

        if (registerResponse.status == true)
        {
            errorField.text = "";
            PlayerPrefs.SetString("Email", registerResponse.data.email);
            PlayerPrefs.SetString("Username", registerResponse.data.name);
            PlayerPrefs.Save();
            registrationObject.SetActive(false);
            OnSecondLoginButtonClicked();
        }
        else
        {
            errorField.text = registerResponse.message;
        }

        request.Dispose();
    }


    // Login User
    private IEnumerator LoginPost(string url, string bodyJsonString)
    {
        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJsonString);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();

        Debug.Log("Status Code: " + request.responseCode);
        Debug.Log(request.downloadHandler.text);

        Login.LoginResponse loginResponse = JsonUtility.FromJson<Login.LoginResponse>(request.downloadHandler.text);

        if (loginResponse.status == true)
        {
            errorField.text = "";
            PlayerPrefs.SetString("Email", loginResponse.data.email);
            PlayerPrefs.SetString("Username", loginResponse.data.name);
            NetworkManager.Instance.userAccessToken = loginResponse.data.accessToken;
            NetworkManager.Instance.userFlowAddress = loginResponse.data.flowAccount.address;
            loginObject.SetActive(false);
            registrationObject.SetActive(false);
            LevelManager.Instance.LoadScene("MainMenu");
        }
        else
        {
            errorField.text = loginResponse.message;
            PlayerPrefs.DeleteAll();
            nonVideoStartingScreen.SetActive(true);
            OnLoginClicked();
        }

        request.Dispose();
    }

    public void OnRegisterButtonClicked()
    {
        string _name = usernameRegistrationField.text;
        string _email = emailRegistrationField.text;
        string _password = passwordRegistrationField.text;
        string _confirmPassword = confirmPasswordField.text;

        if (_name.Length < 3)
        {
            errorField.text = "Username is too short!";
        }
        else if (_password.Length < 8)
        {
            errorField.text = "Password is too short!";
        }
        else if (_password != _confirmPassword)
        {
            errorField.text = "Passwords are not the same!";
        }
        else if (!IsEmail(_email))
        {
            errorField.text = "Please, write email";
        }
        else
        {        
            UserInputData user = JsonUtility.FromJson<UserInputData>(RegisterTxn.text);

            user.name = _name;
            user.email = _email;
            user.password = _password;

            PlayerPrefs.DeleteAll();
            PlayerPrefs.SetString("Password", _password);
            PlayerPrefs.Save();

            string newJson = JsonUtility.ToJson(user);
            string path = NetworkManager.Instance.endpointPATH + NetworkManager.Instance.registerPATH;

            StartCoroutine(Register(path, newJson));
        }
    }

    public void OnLoginEnable()
    {
        if (PlayerPrefs.HasKey("Email"))
        {
            emailLoginField.text = PlayerPrefs.GetString("Email");
            passwordLoginField.text = PlayerPrefs.GetString("Password");
        }
    }

    public void OnLoginButtonClicked()
    {
        string _email = emailLoginField.text;
        string _password = passwordLoginField.text;

        UserInputData user = JsonUtility.FromJson<UserInputData>(LoginTxn.text);

        user.email = _email;
        user.password = _password;

        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetString("Password", _password);
        PlayerPrefs.Save();

        string newJson = JsonUtility.ToJson(user);
        string path = NetworkManager.Instance.endpointPATH + NetworkManager.Instance.loginPATH;

        StartCoroutine(LoginPost(path, newJson));
    }

    public void OnSecondLoginButtonClicked()
    {
        string _email = PlayerPrefs.GetString("Email");
        string _password = PlayerPrefs.GetString("Password");

        UserInputData user = JsonUtility.FromJson<UserInputData>(LoginTxn.text);

        user.email = _email;
        user.password = _password;

        string newJson = JsonUtility.ToJson(user);
        string path = NetworkManager.Instance.endpointPATH + NetworkManager.Instance.loginPATH;

        StartCoroutine(LoginPost(path, newJson));
    }


    // For enabling fields
    public void OnLoginClicked()
    {      
        loginObject.SetActive(true);
        registrationObject.SetActive(false);
        loginButtons.SetActive(false);
        OnLoginEnable();
    }

    public void OnRegisterClicked()
    {
        loginObject.SetActive(false);
        registrationObject.SetActive(true);
        loginButtons.SetActive(false);
    }

    public void OnGoogleAuthClicked()
    {

    }

    public class GoogleSignIn : MonoBehaviour
    {
        // Replace with your Google Client ID
        private string clientId = "YOUR_GOOGLE_CLIENT_ID";

        // Replace with your API endpoint for Google Log In
        private string apiPath = "http://your-api-endpoint.com/auth/google_login";

        private async void GoogleSignIn()
        {
            try
            {
                // Create the Google Authorization Code Flow with client ID and secret
                GoogleAuthorizationCodeFlow flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets
                    {
                        ClientId = clientId
                    },
                    Scopes = new[] { "email", "profile" }
                });

                // Get the authorization URL
                string authUrl = flow.CreateAuthorizationCodeRequest("urn:ietf:wg:oauth:2.0:oob").Build().AbsoluteUri;

                // Open the URL in the default browser to sign in to Google account
                Application.OpenURL(authUrl);

                // Wait for the user to sign in to their Google account and grant permission
                string authCode = await WaitForGoogleAuthorizationCode();

                // Exchange the authorization code for a token
                TokenResponse token = await flow.ExchangeCodeForTokenAsync("user", authCode, "urn:ietf:wg:oauth:2.0:oob");

                // Send the token to the server
                await SendTokenToServer(token);
            }
            catch (System.Exception e)
            {
                Debug.Log("Google Sign-In Error: " + e.Message);
            }
        }

        private async Task<string> WaitForGoogleAuthorizationCode()
        {
            // Wait for the authorization code to be received from the server
            while (true)
            {
                string authorizationCode = PlayerPrefs.GetString("GoogleAuthorizationCode");
                if (!string.IsNullOrEmpty(authorizationCode))
                {
                    PlayerPrefs.DeleteKey("GoogleAuthorizationCode");
                    return authorizationCode;
                }
                await Task.Delay(1000);
            }
        }

        private async Task SendTokenToServer(TokenResponse token)
        {
            // Create the JSON data to send to the server
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Content-Type", "application/json");
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("access_token", token.AccessToken);
            data.Add("id_token", token.IdToken);

            // Convert the JSON data to a byte array
            byte[] jsonData = System.Text.Encoding.UTF8.GetBytes(Json.Serialize(data));

            // Send a POST request to the server with the token data
            using (UnityWebRequest request = UnityWebRequest.Post(apiPath, ""))
            {
                foreach (KeyValuePair<string, string> header in headers)
                {
                    request.SetRequestHeader(header.Key, header.Value);
                }
                request.uploadHandler = new UploadHandlerRaw(jsonData);
                request.downloadHandler = new DownloadHandlerBuffer();
                await request.SendWebRequest();

                // Check if the request succeeded
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log("Google Sign-In Error: " + request.error);
                    return;
                }

                // Handle the server response
                string response = request.downloadHandler.text;
                if (response == "Success")
                {
                    Debug.Log("Google Sign-In Successful!");
                }
            }
        }

        public void OnVideoClicked()
        {
            isVideoClicked = true;

            StartCoroutine(WaitForAlpha());
        }

        private IEnumerator WaitForAlpha()
        {
            while (videoStartingScreen.transform.Find("ClickGameStart").GetComponent<CanvasGroup>().alpha > 0.001f)
            {
                yield return null;
            }
            videoStartingScreen.transform.Find("ClickGameStart").gameObject.SetActive(false);
            if (PlayerPrefs.HasKey("Email"))
            {
                OnSecondLoginButtonClicked();
            }
            else
            {
                nonVideoStartingScreen.SetActive(true);
                OnRegisterClicked();
            }
        }

        private void Update()
        {
            if (isVideoClicked)
            {
                UpdateAlphaVideo();
            }      
        }
    }

    private void UpdateAlphaVideo()
    {
        float alphaTime = 0.05f;
        float newAlpha = Mathf.SmoothDamp(videoStartingScreen.transform.Find("ClickGameStart").GetComponent<CanvasGroup>().alpha, 
            0f, ref alphaVelocity, alphaTime);
        videoStartingScreen.transform.Find("ClickGameStart").GetComponent<CanvasGroup>().alpha = newAlpha;
    }

    #region EmailCheck

    public const string MatchEmailPattern =
            @"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@"
            + @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\."
              + @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|"
            + @"([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})$";

    public static bool IsEmail(string email)
    {
        if (email != null) return Regex.IsMatch(email, MatchEmailPattern);
        else return false;
    }

    #endregion
}
