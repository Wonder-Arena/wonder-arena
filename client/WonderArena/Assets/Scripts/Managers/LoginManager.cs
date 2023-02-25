using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Text;
using System.Text.RegularExpressions;

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
            loginObject.SetActive(true);
            registrationObject.SetActive(false);
            OnLoginEnable();         
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
            GameManager.Instance.userAccessToken = loginResponse.data.accessToken;
            GameManager.Instance.userFlowAddress = loginResponse.data.flowAccount.address;
            loginObject.SetActive(false);
            registrationObject.SetActive(false);
            LevelManager.Instance.LoadScene("MainMenu");
        }
        else
        {
            errorField.text = loginResponse.message;
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

            string newJson = JsonUtility.ToJson(user);
            string path = GameManager.Instance.endpointPATH + GameManager.Instance.registerPATH;

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

        string newJson = JsonUtility.ToJson(user);
        string path = GameManager.Instance.endpointPATH + GameManager.Instance.loginPATH;

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
