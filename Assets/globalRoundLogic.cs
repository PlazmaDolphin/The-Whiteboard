using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Unity.Mathematics;
using System.Numerics;
public class globalRoundLogic : MonoBehaviour
{
    public static int playerDraftNo = 1;
    public static int enemyDraftNo = 1;
    public static List<dummyUnit> allUnits = new List<dummyUnit>();
    public enemyController enemyController;
    public GameObject fightButton, winUI, loseUI;

    public static void addMe(dummyUnit d){
        Debug.Log("Drafted unit: " + playerDraftNo);
        d.draftNo = playerDraftNo;
        playerDraftNo++;
        allUnits.Add(d);
    }
    public static void addEnemy(dummyUnit d){
        d.draftNo = enemyDraftNo;
        enemyDraftNo++;
        allUnits.Add(d);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        killTheDead(); // Make sure theres no funky stuff from the last scene
    }
    static void resetRound(){
        playerDraftNo = 1;
        enemyDraftNo = 1;
        allUnits.Clear();
    }
    static void moveTurn(int turn){
        foreach (dummyUnit d in allUnits){
            if (d.draftNo == turn){
                d.moveIt();
                d.moving = true;
            }
        }
    }
    static void killTheDead(){
        //destroy and remove all who are dead
        for (int i = allUnits.Count - 1; i >= 0; i--){
            if (allUnits[i] == null || allUnits[i].dead){
                dummyUnit d = allUnits[i];
                allUnits.RemoveAt(i);
                Destroy(d.gameObject);
            }
        }
    }
    static bool didIWin(){
        //if any player unit is colliding with an exit point, they win (exit point may not exist in some stages)
        foreach (exitPoint exit in FindObjectsByType<exitPoint>(FindObjectsSortMode.None)){
            foreach (dummyUnit d in FindObjectsByType<dummyUnit>(FindObjectsSortMode.None)){
                if (!d.isEnemy && !d.dead && UnityEngine.Vector3.Distance(d.transform.position, exit.transform.position) < 1f){
                    return true;
                }
            }
        }
        //if an exit point exists, getting to it is the only way to win
        if (FindObjectsByType<exitPoint>(FindObjectsSortMode.None).Length > 0){
            return false;
        }   
        //total destruction is a loss
        if (FindObjectsByType<dummyUnit>(FindObjectsSortMode.None).Length == 0){
            return false;
        }
        //check if all enemies are dead
        foreach (dummyUnit d in FindObjectsByType<dummyUnit>(FindObjectsSortMode.None)){
            if (d.isEnemy && !d.dead){
                return false;
            }
        }
        return true;
    }
    static bool didILose(){
        //check if there is at least one lethal player unit left (not dummy)
        // get a list of every unit in the game
        bool exitExists = FindObjectsByType<exitPoint>(FindObjectsSortMode.None).Length > 0;

        foreach (dummyUnit d in FindObjectsByType<dummyUnit>(FindObjectsSortMode.None)){
            if (!d.isEnemy && !d.dead && (d.unitClass != UnitClassType.Dummy || exitExists)){
                return false;
            }
        }
        return true;
    }
    IEnumerator playRound() {
        fightButton.SetActive(false);
        //show AI moves
        Debug.Log("Num units: " + allUnits.Count);
        yield return StartCoroutine(enemyController.drawAllPaths());
        // Wait for the enemy to finish drawing their paths
        int turnNo = 1;
        while (turnNo <= math.max(playerDraftNo, enemyDraftNo)) {
            moveTurn(turnNo); // Start all unit actions
            yield return StartCoroutine(waitForAllDone()); // Wait here until they’re done
            turnNo++;
            killTheDead(); // Remove dead units from the list
        }

        Debug.Log("Round finished!");
        // Reset everyone
        Debug.Log("Num units: " + allUnits.Count);
        foreach (dummyUnit d in allUnits){
            d.resetSelf();
        }
        resetRound();
        enemyController.cleanupList();
        // Check if the player won or lost
        if (didIWin()){
            winUI.SetActive(true);
        } else if (didILose()){
            loseUI.SetActive(true);
        }
        else {
            // Keep going till someone dies!
            fightButton.SetActive(true);
        }
    }
    static IEnumerator waitForAllDone(){
        yield return new WaitUntil(() => allUnitsDone());
    }
    static bool allUnitsDone(){
        foreach (dummyUnit d in allUnits){
            if (d.moving){
                return false;
            }
        }
        return true;
    }
    public void playRoundButton(){
        StartCoroutine(playRound());
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
