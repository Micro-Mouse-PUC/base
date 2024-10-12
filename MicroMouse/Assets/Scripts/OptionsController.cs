using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using Unity.VisualScripting;

public class OptionsController : MonoBehaviour
{
    [SerializeField] private Button Back;
    [SerializeField] private Button Graphics;
    [SerializeField] private Button Sound;
    [SerializeField] private Button Controls;

    private void Awake(){
        Back.onClick.AddListener(OnButtonBackClick);
        Graphics.onClick.AddListener(OnButtonGraphicsClick);
        Sound.onClick.AddListener(OnButtonSoundClick);
        Controls.onClick.AddListener(OnButtonControlsClick);
    }

    private void OnButtonBackClick(){
        SceneManager.LoadScene("MainMenu");
    }

    private void OnButtonGraphicsClick(){
        Debug.Log("Graphics");
    }

    private void OnButtonSoundClick(){
        Debug.Log("Sound");
    }

    private void OnButtonControlsClick(){
        Debug.Log("Sound");
    }

}
