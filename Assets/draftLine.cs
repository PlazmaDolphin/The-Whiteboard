using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class draftLine : MonoBehaviour
{
    public float pointSpacing = 0.4f;
    public int smoothness = 100;
    public float maxRange = 5f;
    public float lineWidth = 0.05f;
    public GameObject handFollow;
    public SpriteRenderer playerHand, enemyHand;
    public GameObject arrowImage;
    public GameObject wallFinder;
    public float drawTime = 1f;
    public bool drafted = false; //Can only draw a line once
    public bool isPlayer = true; //True if the player is drawing, false if the enemy is drawing

    private LineRenderer lineRenderer;
    public List<Vector3> rawPoints = new List<Vector3>();
    private Camera cam;
    private bool isDrawing = false;
    private bool validLine = true;
    public bool isAnimating = false;
    private Vector3 originPoint;
    private float drawProgress = 0f;

    void Start()
    {
        cam = Camera.main;
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        wallFinder.SetActive(false);
        Invoke("setHand", 0.1f);
    }
    private void setHand(){
        if (isPlayer){
            playerHand.enabled = true;
            enemyHand.enabled = false;
        } else {
            playerHand.enabled = false;
            enemyHand.enabled = true;
        }
    }
    public void startDrawing(){
        if (drafted) return;
        validLine = true;
        isDrawing = true;
        rawPoints.Clear();
        originPoint = transform.position;
        AddPoint(originPoint);
        lineRenderer.positionCount = 0;
        SetLineColor(new Color(0f, 0f, 0f, 0.5f));
        arrowImage.SetActive(false);
    }
    // Used by AI instead of drawing, instantly animate after setting points
    public void setPoints(List<Vector3> points){
        if (drafted) return;
        validLine = true;
        isDrawing = false;
        rawPoints.Clear();
        rawPoints.AddRange(points);
        lineRenderer.positionCount = 0;
        arrowImage.SetActive(false);
        UpdateLine();
        isAnimating = true;
        handFollow.SetActive(true);
    }
    public void resetPen(){
        drafted = false;
        validLine = true;
        isDrawing = false;
        isAnimating = false;
        drawProgress = 0f;
        handFollow.SetActive(false);
        rawPoints.Clear();
        lineRenderer.positionCount = 0;
        //SetLineColor(new Color(0f, 0f, 0f, 0.5f));
        arrowImage.SetActive(false);
    }
    void Update()
    {
        if (isAnimating && rawPoints.Count > 1)
        {
            drawProgress += Time.deltaTime / drawTime;
            int visibleCount = Mathf.Clamp((int)(drawProgress * rawPoints.Count), 1, rawPoints.Count-1);
            if (drawProgress >= 1f)
            {
                isAnimating = false;
                handFollow.SetActive(false);
                arrowImage.SetActive(true);
                arrowImage.transform.position = rawPoints[rawPoints.Count - 1];
                Vector3 finalDir = rawPoints[rawPoints.Count - 1] - rawPoints[rawPoints.Count - 2];
                arrowImage.transform.rotation = Quaternion.LookRotation(Vector3.forward, finalDir);
                return;
            }
            lineRenderer.positionCount = visibleCount;
            lineRenderer.SetPositions(rawPoints.GetRange(0, visibleCount).ToArray());
            if(isPlayer) SetLineColor(new Color(0f, 0f, 0f, 1f));
            else SetLineColor(new Color(0f, 0f, 1f, 1f));
            handFollow.transform.position = rawPoints[visibleCount];
        }
        // END THE LINE
        if (Input.GetMouseButtonUp(0))
        {
            if (!isDrawing || !isPlayer) return;
            isDrawing = false;
            drawProgress = 0f;
            if (!validLine){
                rawPoints.Clear();
                lineRenderer.positionCount=0;
                arrowImage.SetActive(false);
                validLine = true;
                return;
            } 
            isAnimating = true;
            handFollow.SetActive(true);
            drafted = true;
        }
        // CANCEL THE LINE
        if (Input.GetMouseButtonDown(1))
        {
            if (drafted || !isPlayer) return;
            resetPen();
        }

        if (isDrawing)
        {
            float pathLength = GetPathLength(rawPoints);
            if (pathLength >= maxRange)
            {
                SetLineColor(new Color(0.5f, 0f, 0f, 0.4f));
                return;
            }

            Vector3 currentPos = GetMouseWorldPosition();
            if (Vector3.Distance(rawPoints[rawPoints.Count - 1], currentPos) > pointSpacing)
            {
                AddPoint(currentPos);
                UpdateLine();
            }

            if (rawPoints.Count > 1)
            {
                wallFinder.SetActive(true);
                wallFinder.transform.position = rawPoints[rawPoints.Count - 1];
                if(IsTouchingWall(wallFinder.GetComponent<CircleCollider2D>()) && rawPoints.Count > 2){
                    validLine = false;
                    SetLineColor(Color.red);
                }
                wallFinder.SetActive(false);
            }
        }
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(cam.transform.position.z);
        return cam.ScreenToWorldPoint(mousePos);
    }

    void AddPoint(Vector3 point)
    {
        rawPoints.Add(point);
    }

    float GetPathLength(List<Vector3> points)
    {
        float length = 0f;
        for (int i = 1; i < points.Count; i++)
        {
            length += Vector3.Distance(points[i - 1], points[i]);
        }
        return length;
    }

    void UpdateLine()
    {
        if (rawPoints.Count < 2) return;

        List<Vector3> smoothed = GetSmoothedPath(rawPoints, smoothness);
        lineRenderer.positionCount = smoothed.Count;
        lineRenderer.SetPositions(smoothed.ToArray());
    }

    void SetLineColor(Color color)
    {
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }

    List<Vector3> GetSmoothedPath(List<Vector3> points, int smoothFactor)
    {
        List<Vector3> curved = new List<Vector3>();

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 p0 = i > 0 ? points[i - 1] : points[i];
            Vector3 p1 = points[i];
            Vector3 p2 = points[i + 1];
            Vector3 p3 = i < points.Count - 2 ? points[i + 2] : points[i + 1];

            for (int j = 0; j < smoothFactor; j++)
            {
                float t = j / (float)smoothFactor;
                curved.Add(CatmullRom(p0, p1, p2, p3, t));
            }
        }

        curved.Add(points[points.Count - 1]);

        return curved;
    }

    Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        return 0.5f * (
            2f * p1 +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t
        );
    }
    public static bool IsTouchingWall(Collider2D collider)
    {
        var results = new List<Collider2D>();
        ContactFilter2D filter = new ContactFilter2D().NoFilter();

        collider.Overlap(filter, results);
        foreach (var hit in results)
        {
            if (hit != null && hit.CompareTag("obstacle"))
            {
                return true;
            }
        }

        return false;
    }
}
