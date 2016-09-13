using UnityEngine;

public class Annotation
{
    public Rect trackingBounds;
    public Rect actionRecognitionBounds;
    public string action;
    public uint agentId;
    public float detectionConfidenceScore;
    public Vector3 worldPosition;
    public bool isCrowd;
    public bool actionRecognitionBoundsIsValid;

    public Annotation(string action, Rect trackingBounds, Rect actionrecognitionBounds, uint agentId, float detectionConfidenceScore, Vector3 worldPosition, bool isCrowd, bool actionRecognitionBoundsInvalid)
    {
        this.trackingBounds = trackingBounds;
        this.actionRecognitionBounds = actionrecognitionBounds;
        this.action = action;
        this.agentId = agentId;
        this.detectionConfidenceScore = detectionConfidenceScore;
        this.worldPosition = worldPosition;
        this.isCrowd = isCrowd;
        this.actionRecognitionBoundsIsValid = actionRecognitionBoundsInvalid;
    }
}
