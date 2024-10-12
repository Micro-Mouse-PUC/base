using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.Events;

public class MenuController : MonoBehaviour
{
    [SerializeField] private Button NewGame;
    [SerializeField] private Button LoadGame;
    [SerializeField] private Button Options;
    [SerializeField] private Button Exit;



    private void Awake(){
        NewGame.onClick.AddListener(OnButtonNewGameClick);
        LoadGame.onClick.AddListener(OnButtonLoadGameClick);
        Options.onClick.AddListener(OnButtonOptionsClick);
        Exit.onClick.AddListener(OnButtonExitClick);
    }

    private void OnButtonNewGameClick(){
        Debug.Log("New Game");
    }

    private void OnButtonLoadGameClick(){
        Debug.Log("Load Game");
    }

    private void OnButtonOptionsClick(){
        SceneManager.LoadScene("Options");
    }

    private void OnButtonExitClick(){
        Debug.Log("Exit");
        Application.Quit();
    }
}
