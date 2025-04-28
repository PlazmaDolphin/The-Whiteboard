using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

public class popupLogic : MonoBehaviour
{
    // List of hardcoded strings to hold info needed for the round
    public TextMeshProUGUI tipTxt;
    List<string> popupTips = new List<string> {
    "you aint supposed to see this",
    "Click and drag on your units to draft them.\nPress the FIGHT! button when done.\nGet to the exit!",
    "Look out for the enemy stabber!\n Both player and enemy drafts move at the same time.\nDodge the stabber and get to the exit!",
    "You control a stabber now!\nNo more exits, instead leave no enemies standing!",
    "More units to control!\n Order matters, so think wisely!\n2 Stabbers running into each other will kill each other!",
    "Don't get left with only the dummy, they can't kill anything!\nStabbers are only dangerous on their turn.\nNow get killing!",
    "You've learned enough to be dangerous!\nFinish this last fight and be promoted to Whiteboard General!",
    "you aint supposed to see this"
    };
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tipTxt.text = popupTips[SceneManager.GetActiveScene().buildIndex];   
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void closeMe(){
        gameObject.SetActive(false);
    }
    public void restartLevel(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void nextLevel(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    public void mainMenu(){
        SceneManager.LoadScene(0);
    }
    public void quitGame(){
        Application.Quit();
    }
    public void firstLevel(){
        SceneManager.LoadScene(1);
    }
}
