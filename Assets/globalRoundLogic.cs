using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Unity.Mathematics;
public class globalRoundLogic : MonoBehaviour
{
    public static int playerDraftNo = 1;
    public static int enemyDraftNo = 1;
    public static List<dummyUnit> allUnits = new List<dummyUnit>();
    public enemyController enemyController;

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
    IEnumerator playRound() {
        //show AI moves
        Debug.Log("Num units: " + allUnits.Count);
        yield return StartCoroutine(enemyController.drawAllPaths());
        // Wait for the enemy to finish drawing their paths
        int turnNo = 1;
        while (turnNo <= math.max(playerDraftNo, enemyDraftNo)) {
            moveTurn(turnNo); // Start all unit actions
            yield return StartCoroutine(waitForAllDone()); // Wait here until they’re done
            turnNo++;
        }

        Debug.Log("Round finished!");
        // Reset everyone
        Debug.Log("Num units: " + allUnits.Count);
        foreach (dummyUnit d in allUnits){
            d.resetSelf();
        }
        resetRound();
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
