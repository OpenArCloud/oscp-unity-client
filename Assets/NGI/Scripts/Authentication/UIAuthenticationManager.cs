using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAuthenticationManager : MonoBehaviour
{

    #region Fields

    [SerializeField] private Button _loginButton;
    [SerializeField] private Button _logoutButton;

    [SerializeField] private GameObject _authenticatedGUI;
    [SerializeField] private GameObject _unAuthenticatedGUI;

    #endregion



    #region Events

    #endregion

    #region LifeCycle

    private void OnEnable()
    {
        OAuth2Authentication.IsAuthenticated += HandleAuthenticated;
    }
    private void OnDisable()
    {
        OAuth2Authentication.IsAuthenticated -= HandleAuthenticated;
    }


    #endregion

    #region Public Methods

    #endregion

    #region Private Methods
    private void HandleAuthenticated(bool isAuthenticated)
    {
        _authenticatedGUI.SetActive(isAuthenticated);
        _unAuthenticatedGUI.SetActive(!isAuthenticated);

    }

    #endregion

}
