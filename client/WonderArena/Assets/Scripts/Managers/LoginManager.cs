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

    //Serialized Fields from front end
    [Header("Input Fields")]
    [SerializeField] TMP_InputField usernameField;
    [SerializeField] TMP_InputField emailField;
    [SerializeField] TMP_InputField passwordField;
    [SerializeField] TMP_InputField confirmPasswordField;

    [Header("Json Files")]
    [SerializeField] TextAsset RegisterTxn;
    [SerializeField] TextAsset LoginTxn;

    [Header("URLs")]
    [SerializeField] string endpointPATH = "https://wonder-arena-production.up.railway.app";
    [SerializeField] string registerPATH = "/auth";
    [SerializeField] string loginPATH = "/auth/login";

    private void Awake()
    {
        usernameField = usernameField.GetComponent<TMP_InputField>();
        emailField = emailField.GetComponent<TMP_InputField>();
        passwordField = passwordField.GetComponent<TMP_InputField>();
        confirmPasswordField = confirmPasswordField.GetComponent<TMP_InputField>();
    }


    // Registrating user
    private IEnumerator Register(string url, string bodyJsonString)
    {
        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJsonString);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();

        Debug.Log("Status Code: " + request.responseCode);

        Registration.RegisterResponse registerResponse = JsonUtility.FromJson<Registration.RegisterResponse>(request.downloadHandler.text);

        if (registerResponse.status == true)
        {
            Debug.Log(registerResponse.data.accessToken);
        }
        else
        {
            Debug.Log(registerResponse.message);
        }
    }

    public void OnRegisterButtonClicked()
    {
        string _name = usernameField.text;
        string _email = emailField.text;
        string _password = passwordField.text;
        string _confirmPassword = confirmPasswordField.text;

        if (_name.Length == 0 || _email.Length == 0 || _password.Length == 0)
        {
            Debug.Log("Too short!");
        }
        else if (_password != _confirmPassword)
        {
            Debug.Log("Passwords are not the same!");
        }
        else if (!IsEmail(_email))
        {
            Debug.Log("Please, write email");
        }
        else
        {
            UserInputData user = JsonUtility.FromJson<UserInputData>(RegisterTxn.text);

            user.name = _name;
            user.email = _email;
            user.password = _password;

            string newJson = JsonUtility.ToJson(user);

            StartCoroutine(Register(endpointPATH + registerPATH, newJson));
        }
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
