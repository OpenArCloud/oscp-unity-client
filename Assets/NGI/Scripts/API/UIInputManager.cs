using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;
using System;

// Manages all the text and button inputs
// Also acts like the main manager script for the game.
public class UIInputManager : MonoBehaviour
{
   public static string CachePath;

   public Button signupButton;
   public Button loginButton;
   public Button startButton;
   public Button logoutButton;
   public TMP_InputField emailFieldLogin;
   public TMP_InputField passwordFieldLogin;
   public TMP_InputField usernameField;
   public TMP_InputField emailField;
   public TMP_InputField passwordField;


   [SerializeField]private GameObject _unauthInterface;
    [SerializeField] GameObject _authInterface;

    [SerializeField] GameObject _loading;
    [SerializeField] GameObject _welcome;
    [SerializeField] GameObject _confirmEmail;
    [SerializeField] GameObject _signupContainer;

   private List<Selectable> _fields;
   private int _selectedFieldIndex = -1;

   private void displayComponentsFromAuthStatus(bool authStatus)
   {
      if (authStatus)
      {
         // Debug.Log("User authenticated, show welcome screen with options");
         _loading.SetActive(false);
         _unauthInterface.SetActive(false);
         _authInterface.SetActive(true);
         _welcome.SetActive(true);
      }
      else
      {
         // Debug.Log("User not authenticated, activate/stay on login scene");
         _loading.SetActive(false);
         _unauthInterface.SetActive(true);
         _authInterface.SetActive(false);
      }

      // clear out passwords
      passwordFieldLogin.text = "";
      passwordField.text = "";

      // set focus to email field on login form
      _selectedFieldIndex = -1;
   }

   private async void onLoginClicked()
   {
      _unauthInterface.SetActive(false);
      _loading.SetActive(true);
        // Debug.Log("onLoginClicked: " + emailFieldLogin.text + ", " + passwordFieldLogin.text);
        bool successfulLogin = false; //await _authenticationManager.Login(emailFieldLogin.text, passwordFieldLogin.text);
      displayComponentsFromAuthStatus(successfulLogin);
   }

   private async void onSignupClicked()
   {
      _unauthInterface.SetActive(false);
      _loading.SetActive(true);

        // Debug.Log("onSignupClicked: " + usernameField.text + ", " + emailField.text + ", " + passwordField.text);
        bool successfulSignup = false; //await _authenticationManager.Signup(usernameField.text, emailField.text, passwordField.text);

      if (successfulSignup)
      {
         // here we re-enable the whole auth container but hide the sign up panel
         _signupContainer.SetActive(false);

         _confirmEmail.SetActive(true);

         // copy over the new credentials to make the process smoother
         emailFieldLogin.text = emailField.text;
         passwordFieldLogin.text = passwordField.text;

         // set focus to email field on login form
         _selectedFieldIndex = 0;
      }
      else
      {
         _confirmEmail.SetActive(false);

         // set focus to email field on signup form
         _selectedFieldIndex = 3;
      }

      _loading.SetActive(false);
      _unauthInterface.SetActive(true);
   }

   private void onLogoutClick()
   {

      displayComponentsFromAuthStatus(false);
   }

   private void onStartClick()
   {
      SceneManager.LoadScene("GameScene");
      Debug.Log("Changed to GameScene");

      // call to lambda to demonstrate use of credentials

   }

   private async void RefreshToken()
   {

        // displayComponentsFromAuthStatus(successfulRefresh);
    }

   void Start()
   {
      Debug.Log("UIInputManager: Start");
      // check if user is already authenticated 
      // We perform the refresh here to keep our user's session alive so they don't have to keep logging in.
      RefreshToken();

      signupButton.onClick.AddListener(onSignupClicked);
      loginButton.onClick.AddListener(onLoginClicked);
      startButton.onClick.AddListener(onStartClick);
      logoutButton.onClick.AddListener(onLogoutClick);
   }

   void Update()
   {
      HandleInputTabbing();
   }

   // Handles tabbing between inputs and buttons
   private void HandleInputTabbing()
   {
      if (Input.GetKeyDown(KeyCode.Tab))
      {
         CheckForAndSetManuallyChangedIndex();

         // update index to where we need to tab to
         _selectedFieldIndex++;

         if (_selectedFieldIndex >= _fields.Count)
         {
            // reset back to first input
            _selectedFieldIndex = 0;
         }
         _fields[_selectedFieldIndex].Select();
      }
   }

   // If the user selects an input via mouse click, then the _selectedFieldIndex 
   // may not be accurate as the focused field wasn't change by tabbing. Here we
   // correct the _selectedFieldIndex in case they wish to start tabing from that point on.
   private void CheckForAndSetManuallyChangedIndex()
   {
      for (var i = 0; i < _fields.Count; i++)
      {
         if (_fields[i] is InputField && ((InputField)_fields[i]).isFocused && _selectedFieldIndex != i)
         {
            // Debug.Log("_selectedFieldIndex is : " + _selectedFieldIndex + ", Reset _selectedFieldIndex to: " + i);
            _selectedFieldIndex = i;
            break;
         }
      }
   }

   void Awake()
   {
      CachePath = Application.persistentDataPath;

     // _unauthInterface = GameObject.Find("UnauthInterface");
     // _authInterface = GameObject.Find("AuthInterface");
     // _loading = GameObject.Find("Loading");
      //_welcome = GameObject.Find("Welcome");
     // _confirmEmail = GameObject.Find("ConfirmEmail");
      //_signupContainer = GameObject.Find("SignupContainer");

      _unauthInterface.SetActive(false); // start as false so we don't just show the login screen during attempted token refresh
      _authInterface.SetActive(false);
      _welcome.SetActive(false);
      _confirmEmail.SetActive(false);
      _signupContainer.SetActive(true);

     

      _fields = new List<Selectable> { emailFieldLogin, passwordFieldLogin, loginButton, emailField, usernameField, passwordField, signupButton };
   }
}
