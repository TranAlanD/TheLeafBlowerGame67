using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject confirmMenu;
    public AudioClip onClickClip;
    public AudioSource audioSource;
    
    void Update()
    {
        if(Input.GetKey(KeyCode.P)){
            Pause();
        }
    }

    public void Pause(){
        pauseMenu.SetActive(true);
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Resume(){
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ClickAndLoadMainMenu(){
        audioSource.PlayOneShot(onClickClip);
        Time.timeScale = 1;
        Invoke("LoadMainMenu", 0.3f);
    }
    public void LoadMainMenu(){
        SceneManager.LoadScene("Main Menu");
    }

    public void BackToPauseMenu(){
        confirmMenu.SetActive(false);
        pauseMenu.SetActive(true);
    }

    public void GoToConfirmMenu(){
        pauseMenu.SetActive(false);
        confirmMenu.SetActive(true);
    }

    public void playClickSound(){
        audioSource.PlayOneShot(onClickClip);
    }

    public void RestartGame(){
        audioSource.PlayOneShot(onClickClip);
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}
