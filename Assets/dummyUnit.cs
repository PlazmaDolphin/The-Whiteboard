using TMPro;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using System.Collections.Generic;
using Unity.VisualScripting;

public enum UnitClassType {Dummy, Stabber, Sniper, Hunter}
public class dummyUnit : MonoBehaviour
{
    public GameObject deadSprite;
    public draftLine thePen;
    public TextMeshPro numText;
    public Vector3 targetAI;
    public NavMeshAgent agent;
    private NavMeshHit hit;
    private NavMeshPath path;
    private float maxSampleDistance = 1f;

    public UnitClassType unitClass = UnitClassType.Dummy;
    public int draftNo;
    public bool moving;
    public bool dead = false;
    public bool isEnemy;
    public float maxRange = 5f;

    private float moveTime = 2f;
    public float moveProgress = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.enabled = false; //disable agent
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
        if (!NavMesh.SamplePosition(targetAI, out hit, maxSampleDistance, NavMesh.AllAreas)) return false;
        targetAI = hit.position;
        agent.SetDestination(targetAI);
        path = new NavMeshPath();
        if (!agent.CalculatePath(targetAI, path)){
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
    int totalPoints = 50;
    float totalDistance = 0f;
    var corners = path.corners;
    int cornerCount = corners.Length;

    // Calculate total path distance
    for (int i = 0; i < cornerCount - 1; i++){
        totalDistance += Vector3.Distance(corners[i], corners[i + 1]);
    }

    // Cap total distance to maxRange
    float cappedDistance = Mathf.Min(totalDistance, maxRange);

    // Build cumulative distance array
    float[] cumDist = new float[cornerCount];
    cumDist[0] = 0f;
    for (int i = 1; i < cornerCount; i++){
        cumDist[i] = cumDist[i-1] + Vector3.Distance(corners[i-1], corners[i]);
    }

    List<Vector3> smoothPath = new List<Vector3>(totalPoints);

    for (int p = 0; p < totalPoints; p++){
        float targetDistance = cappedDistance * p / (totalPoints - 1);

        if (targetDistance <= 0f){
            smoothPath.Add(corners[0]);
            continue;
        }

        // If targetDistance exceeds available path, clamp to last corner
        if (targetDistance >= totalDistance){
            smoothPath.Add(corners[cornerCount-1]);
            continue;
        }

        // Find which segment we're in
        int i = 0;
        while (i < cornerCount - 1 && !(cumDist[i] <= targetDistance && targetDistance <= cumDist[i+1])){
            i++;
        }

        if (i >= cornerCount - 1){
            smoothPath.Add(corners[cornerCount-1]);
            continue;
        }

        float segmentStartDist = cumDist[i];
        float segmentLength = cumDist[i+1] - segmentStartDist;
        float t = (targetDistance - segmentStartDist) / segmentLength;

        smoothPath.Add(Vector3.Lerp(corners[i], corners[i+1], t));
    }

    return smoothPath;
}

    public void genEnemyPenPath(){
        //activate agent
        agent.enabled = true;
        calcAiPath();
        thePen.setPoints(smoothPath());
        thePen.arrowImage.SetActive(false);
        agent.enabled = false; //disable agent
    }
    public void moveIt(){
        moving = true;
        moveProgress = 0f;
        thePen.arrowImage.SetActive(false);
    }
    private void softDie(){
        //spawn a dead sprite then disable self
        GameObject imdead = Instantiate(deadSprite, transform.position, Quaternion.identity);
        imdead.transform.localScale = transform.localScale;
        SpriteRenderer sr = imdead.GetComponent<SpriteRenderer>();
        if (sr != null){
            sr.sprite = GetComponent<SpriteRenderer>().sprite;
            sr.color = GetComponent<SpriteRenderer>().color;
            sr.sortingLayerID = GetComponent<SpriteRenderer>().sortingLayerID;
            sr.sortingOrder = GetComponent<SpriteRenderer>().sortingOrder;
            //this.gameObject.SetActive(false); //disable self
            moving = false;
        }
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
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("unit")){
            dummyUnit otherUnit = collision.gameObject.GetComponent<dummyUnit>();
            if (otherUnit != null && otherUnit.isEnemy != isEnemy){
                //things are going to get bloody
                if (otherUnit.unitClass == UnitClassType.Stabber && otherUnit.moving){
                    //die
                    Invoke("softDie", 0.1f);
                    dead = true;
                }
            }
        }
    }
}
