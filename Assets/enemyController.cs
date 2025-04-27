using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class enemyController : MonoBehaviour
{
    private List<dummyUnit> allEnemies = new List<dummyUnit>();
    private List<Vector3> playerTargets = new List<Vector3>();
    void Start()
    {
        // Find all enemy units in the scene and add them to the list
        dummyUnit[] enemies = FindObjectsByType<dummyUnit>(FindObjectsSortMode.None);
        foreach (dummyUnit enemy in enemies)
        {
            if (enemy.isEnemy)
            {
                allEnemies.Add(enemy);
            }
        }
    }
    void findPlayerLocations(){
        // Find all player units in the scene and add their positions to the list
        playerTargets.Clear();
        dummyUnit[] players = FindObjectsByType<dummyUnit>(FindObjectsSortMode.None);
        foreach (dummyUnit player in players){
            if (!player.isEnemy){
                playerTargets.Add(player.transform.position);
            }
        }
    }
    void setEnemyTargets(){
        // Set the target positions for each enemy unit (change this for more complex AI)
        foreach (dummyUnit enemy in allEnemies){
            //pick a random player location
            int randomIndex = Random.Range(0, playerTargets.Count);
            Vector3 targetPosition = playerTargets[randomIndex];
            enemy.targetAI = targetPosition;
        }
    }
    public IEnumerator drawAllPaths(){
        findPlayerLocations();
        setEnemyTargets();
        // For each enemy, calculate their path, draw it, and wait for it to finish before moving to the next one
        foreach (dummyUnit enemy in allEnemies){
            enemy.addSelfEnemy();
            enemy.genEnemyPenPath();
            //wait until its pen is done drawing
            yield return new WaitUntil(() => enemy.thePen.isAnimating == false);
        }
    }
    public void cleanupList(){
        //remove enemies that killed themselves
        for (int i = allEnemies.Count - 1; i >= 0; i--){
            if (allEnemies[i] == null){
                allEnemies.RemoveAt(i);
            }
        }
    }
}
