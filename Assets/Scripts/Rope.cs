﻿using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    //[SerializeField] Transform StartPoint;
    [SerializeField] Transform EndPoint;
    Vector2 forceGravity = new Vector2(0f, 0f);

    private LineRenderer lineRenderer;
    private List<RopeSegment> ropeSegments = new List<RopeSegment>();
    [SerializeField] float ropeSegLen = 0.25f;
    [SerializeField] int segmentLength = 35;
    [SerializeField] float lineWidth = 0.1f;

    bool isEndPoint;

    // Use this for initialization
    void Start()
    {
        this.lineRenderer = this.GetComponent<LineRenderer>();
        Vector3 ropeStartPoint = transform.position;

        for (int i = 0; i < segmentLength; i++)
        {
            this.ropeSegments.Add(new RopeSegment(ropeStartPoint));
            ropeStartPoint.y -= ropeSegLen;
        }

        GetComponent<Revolve>().OnLaunch += RemoveEndPoint;
        GetComponent<Throw>().OnCollision += SetGravityToOne;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        this.DrawRope();        
    }

    private void Update()
    {        
        this.Simulate();
    }

    private void Simulate()
    {
        // SIMULATION      
        for (int i = 1; i < this.segmentLength; i++)
        {
            RopeSegment firstSegment = this.ropeSegments[i];
            Vector2 velocity = firstSegment.posNow - firstSegment.posOld;
            firstSegment.posOld = firstSegment.posNow;
            firstSegment.posNow += velocity;
            firstSegment.posNow += forceGravity * Time.fixedDeltaTime;
            this.ropeSegments[i] = firstSegment;
        }

        //CONSTRAINTS
        for (int i = 0; i < 50; i++)
        {
            this.ApplyConstraint();
        }
    }

    private void ApplyConstraint()
    {
        //Constrant to First Point 
        RopeSegment firstSegment = this.ropeSegments[0];
        firstSegment.posNow = this.transform.position;
        this.ropeSegments[0] = firstSegment;


        //Constrant to Second Point 
        EndPointSelection();

        for (int i = 0; i < this.segmentLength - 1; i++)
        {
            RopeSegment firstSeg = this.ropeSegments[i];
            RopeSegment secondSeg = this.ropeSegments[i + 1];

            float dist = (firstSeg.posNow - secondSeg.posNow).magnitude;
            float error = Mathf.Abs(dist - this.ropeSegLen);
            Vector2 changeDir = Vector2.zero;

            if (dist > ropeSegLen)
            {
                changeDir = (firstSeg.posNow - secondSeg.posNow).normalized;
            }
            else if (dist < ropeSegLen)
            {
                changeDir = (secondSeg.posNow - firstSeg.posNow).normalized;
            }

            Vector2 changeAmount = changeDir * error;
            if (i != 0)
            {
                firstSeg.posNow -= changeAmount * 0.5f;
                this.ropeSegments[i] = firstSeg;
                secondSeg.posNow += changeAmount * 0.5f;
                this.ropeSegments[i + 1] = secondSeg;
            }
            else
            {
                secondSeg.posNow += changeAmount;
                this.ropeSegments[i + 1] = secondSeg;
            }
        }
    }

    private void EndPointSelection()
    {
        if(!isEndPoint)
        {
            RopeSegment endSegment = this.ropeSegments[this.ropeSegments.Count - 1];
            endSegment.posNow = this.EndPoint.position;
            this.ropeSegments[this.ropeSegments.Count - 1] = endSegment;
        }
        
    }

    private void DrawRope()
    {
        float lineWidth = this.lineWidth;

        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        Vector3[] ropePositions = new Vector3[this.segmentLength];
        for (int i = 0; i < this.segmentLength; i++)
        {
            ropePositions[i] = this.ropeSegments[i].posNow;
        }

        lineRenderer.positionCount = ropePositions.Length;
        lineRenderer.SetPositions(ropePositions);
    }

    public struct RopeSegment
    {
        public Vector2 posNow;
        public Vector2 posOld;

        public RopeSegment(Vector2 pos)
        {
            this.posNow = pos;
            this.posOld = pos;
        }
    }

    public void RemoveEndPoint()
    {
        isEndPoint = true;
    }

    public void SetGravityToOne()
    {
        forceGravity = new Vector2(0, -0.7f);
    }          
        
}