using UnityEngine;

public class Annotation
{
    public Rect bounds;
    public string action;
    public uint agentId;
    public float detectionConfidenceScore;
    public Vector3 worldPosition;
    public bool isCrowd;

    public Annotation(string action, Rect bounds, uint agentId, float detectionConfidenceScore, Vector3 worldPosition, bool isCrowd)
    {
        this.bounds = bounds;
        this.action = action;
        this.agentId = agentId;
        this.detectionConfidenceScore = detectionConfidenceScore;
        this.worldPosition = worldPosition;
        this.isCrowd = isCrowd;
    }
}
