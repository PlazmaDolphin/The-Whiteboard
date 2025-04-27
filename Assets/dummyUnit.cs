using TMPro;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using System.Collections.Generic;

public class dummyUnit : MonoBehaviour
{
    public draftLine thePen;
    public TextMeshPro numText;
    public Transform targetAI;
    public NavMeshAgent agent;
    private NavMeshHit hit;
    private NavMeshPath path;
    private float maxSampleDistance = 1f;
    public int draftNo;
    public bool moving;
    public bool isEnemy;

    private float moveTime = 2f;
    public float moveProgress = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        thePen.isPlayer = !isEnemy;
    }
    void addSelfHuman(){
        globalRoundLogic.addMe(this);
        numText.text = draftNo.ToString();
    }
    public void addSelfEnemy(){
        globalRoundLogic.addEnemy(this);
        numText.text = draftNo.ToString();
    }
    bool calcAiPath(){
        if (!NavMesh.SamplePosition(targetAI.position, out hit, maxSampleDistance, NavMesh.AllAreas)) return false;
        targetAI.position = hit.position;
        agent.SetDestination(targetAI.position);
        path = new NavMeshPath();
        if (!agent.CalculatePath(targetAI.position, path)){
            Debug.Log("calculatePath failed!");
            return false;
        }
        if (path.status != NavMeshPathStatus.PathComplete){
            Debug.Log("Path not complete: " + path.status);
            return false;
        }
        if (path.corners.Length < 2) return false;
        return true;
    }
    List<Vector3> smoothPath(){
        // Make the total number of points constant (e.g., 50) for smoother animation
        int totalPoints = 50;
        float totalDistance = 0f;
        // Calculate total distance of the path
        for (int i = 0; i < path.corners.Length - 1; i++){
            totalDistance += Vector3.Distance(path.corners[i], path.corners[i + 1]);
        }
        var corners = path.corners;
        int cornerCount = corners.Length;

        // 1) build cumulative distance array
        float[] cumDist = new float[cornerCount];
        cumDist[0] = 0f;
        for (int i = 1; i < cornerCount; i++)
            cumDist[i] = cumDist[i-1] + Vector3.Distance(corners[i-1], corners[i]);

        // 2) for each desired sample distance, find where it sits
        List<Vector3> smoothPath = new List<Vector3>(totalPoints);
        for (int p = 0; p < totalPoints; p++){
            float targetDistance = totalDistance * p / (totalPoints - 1);  
            // clamp to final corner
            if (targetDistance <= 0f){
                smoothPath.Add(corners[0]);
                continue;
            }
            if (targetDistance >= totalDistance){
                smoothPath.Add(corners[cornerCount-1]);
                continue;
            }

            // 3) find segment index so that cumDist[i] <= targetDistance <= cumDist[i+1]
            int i = 0;
            while (!(cumDist[i] <= targetDistance && targetDistance <= cumDist[i+1]))
                i++;

            // 4) local t between corners[i] and corners[i+1]
            float segmentStartDist = cumDist[i];
            float segmentLength    = cumDist[i+1] - segmentStartDist;
            float t = (targetDistance - segmentStartDist) / segmentLength;

            smoothPath.Add(Vector3.Lerp(corners[i], corners[i+1], t));
        }
        return smoothPath;
    }
    public void genEnemyPenPath(){
        calcAiPath();
        thePen.setPoints(smoothPath());
        thePen.arrowImage.SetActive(false);
    }
    public void moveIt(){
        moving = true;
        moveProgress = 0f;
        thePen.arrowImage.SetActive(false);
        //follow the line of movement constructed by thePen, point to point
        //move to the first point of the line, then move to the next point, and so on
        //point: thePen.rawPoints

        
    }
    public void resetSelf(){
        thePen.resetPen();
        moving = false;
        draftNo = 0;
    }
    // Update is called once per frame
    void Update()
    {
        if (thePen.drafted && draftNo==0){
            if (isEnemy){
                addSelfEnemy();
            } else {
                addSelfHuman();
            }
        }
        if (moving){
            //Animate the movement
            moveProgress += Time.deltaTime/moveTime;
            if (moveProgress >= 1f){
                moving = false;
            }
            //move to the next point of the line, then move to the next point, and so on
            //point: thePen.rawPoints
            int progressIdx = Mathf.Clamp((int)(moveProgress * thePen.rawPoints.Count), 1, thePen.rawPoints.Count-1);
            transform.position = thePen.rawPoints[progressIdx];
        }
    }
    void OnMouseDown()
    {
        if(thePen.drafted || isEnemy) return;
        thePen.startDrawing();
    }
}
