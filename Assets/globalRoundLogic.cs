using System.Collections.Generic;
using UnityEngine;
using System.Collections;
public class globalRoundLogic : MonoBehaviour
{
    public static int playerDraftNo = 1;
    private static int maxDraftNo = 1;
    public static List<dummyUnit> allUnits = new List<dummyUnit>();

    public static void addMe(dummyUnit d){
        d.draftNo = playerDraftNo;
        playerDraftNo++;
        allUnits.Add(d);
        if (playerDraftNo > maxDraftNo){
            maxDraftNo = playerDraftNo;
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        resetRound();
    }
    static void resetRound(){
        playerDraftNo = 1;
        allUnits.Clear();
        maxDraftNo = 1;
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
        int turnNo = 1;
        while (turnNo <= maxDraftNo) {
            moveTurn(turnNo); // Start all unit actions
            yield return StartCoroutine(waitForAllDone()); // Wait here until they’re done
            turnNo++;
        }

        Debug.Log("Round finished!");
        // Reset everyone
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
