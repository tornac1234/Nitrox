using System;
using System.Collections.Generic;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

[RequireComponent(typeof(LineRenderer))]
public class NitroxCinematicCamera : MonoBehaviour
{
    public static NitroxCinematicCamera? Instance { get; private set; }
    public float PlaybackSpeed = 1f;
    public float ColdStartDuration = 5f;

    public readonly List<KeyPoint> KeyPoints = [];

    public bool IsRecording { get; private set; }
    public bool IsPlaying { get; private set; }

    private float recordingStartTime;
    private float playingStartTime;
    private int currentPointIndex;
    private LineRenderer lineRenderer;
    private readonly Lazy<Transform> playerTransform = new(() => Player.mainObject.transform);
    private readonly Lazy<Transform> cameraTransform = new(() => MainCamera.camera.transform);

    private void Awake()
    {
        Instance = this;
        enabled = false;
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
    }

    public void Update()
    {
        if (IsPlaying)
        {
            PlayCinematic();
            return;
        }

        if (Input.GetKeyUp(KeyCode.P))
        {
            AddKeyPoint();
        }
        if (Input.GetKeyUp(KeyCode.M))
        {
            if (IsRecording)
            {
                StopRecording();
            }
            else
            {
                StartRecording();
            }
        }
        if (Input.GetKeyUp(KeyCode.O))
        {
            if (IsPlaying)
            {
                StopPlayback();
            }
            else
            {
                StartPlayback();
            }
        }
    }

    public void StartRecording()
    {
        KeyPoints.Clear();
        IsRecording = true;
        recordingStartTime = Time.time;
        Log.InGame("Recording started");
    }

    public void AddKeyPoint()
    {
        if (!IsRecording)
        {
            return;
        }

        float t = Time.time - recordingStartTime;
        KeyPoints.Add(new KeyPoint(t, playerTransform.Value.position, cameraTransform.Value.rotation));
        Log.InGame($"Key point added at {t:F2}s");
        Log.Debug($"Key point added at {t:F2}s, pos={playerTransform.Value.position}");
    }

    public void StopRecording()
    {
        if (!IsRecording)
        {
            return;
        }
        IsRecording = false;

        // Fix hemisphere alignment for all quaternions
        for (int i = 1; i < KeyPoints.Count; i++)
        {
            KeyPoints[i].Rotation = EnsureSameHemisphere(KeyPoints[i - 1].Rotation, KeyPoints[i].Rotation);
        }

        // Detect 180° flips and insert intermediate keyframes
        for (int i = 0; i < KeyPoints.Count - 1; i++)
        {
            Quaternion a = KeyPoints[i].Rotation;
            Quaternion b = KeyPoints[i + 1].Rotation;

            float dot = Mathf.Abs(Quaternion.Dot(a, b));
            if (dot < 0.05f) // ~177° apart
            {
                // Insert intermediate Rotation halfway in Time
                float midTime = (KeyPoints[i].Time + KeyPoints[i + 1].Time) * 0.5f;
                Vector3 midPos = (KeyPoints[i].Position + KeyPoints[i + 1].Position) * 0.5f;
                Quaternion midRot = Quaternion.Slerp(a, b, 0.5f);

                KeyPoint midPoint = new(midTime, midPos, midRot, true);
                KeyPoints.Insert(i + 1, midPoint);

                i++; // Skip past the inserted keyframe
            }
        }

        // Add a coldstart long enough to load the zone around first point

        KeyPoint firstPoint = KeyPoints[0];
        float startTime = firstPoint.Time;

        foreach (KeyPoint keyPoint in KeyPoints)
        {
            keyPoint.Time += ColdStartDuration;
        }

        KeyPoint newStartKeypoint = new(startTime, firstPoint.Position, firstPoint.Rotation, true);
        KeyPoints.Insert(0, newStartKeypoint);

        Log.Debug($"Recording stopped, total points: {KeyPoints.Count}");
        Log.InGame($"Recording stopped, total points: {KeyPoints.Count}");
        RefreshDebugLines();
    }

    public bool ShowDebugLine
    {
        get => lineRenderer.enabled;
        set => lineRenderer.enabled = value;
    }

    public void RefreshDebugLines()
    {
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.green;
        lineRenderer.startWidth = 0.2f;
        lineRenderer.endWidth = 0.2f;
        lineRenderer.positionCount = KeyPoints.Count * 3;
        lineRenderer.useWorldSpace = true;

        for (int index = 0; index < KeyPoints.Count; index++)
        {
            KeyPoint keyPoint = KeyPoints[index];

            int lineNumber = index * 3;
            lineRenderer.SetPosition(lineNumber, keyPoint.Position);
            lineRenderer.SetPosition(lineNumber + 1, keyPoint.Position + keyPoint.Rotation * Vector3.forward);
            lineRenderer.SetPosition(lineNumber + 2, keyPoint.Position);
        }
    }

    public void StartPlayback()
    {
        if (KeyPoints.Count < 2)
        {
            Log.Warn("Not enough key points to play cinematic.");
            return;
        }

        Player.main.cinematicModeActive = true;
        IsPlaying = true;
        playingStartTime = Time.time;
        currentPointIndex = 0;
        Log.Debug("Playback started");
    }

    public void StopPlayback()
    {
        IsPlaying = false;
        Player.main.cinematicModeActive = false;
        cameraTransform.Value.localRotation = Quaternion.identity;
        Log.Debug("Playback stopped");
    }

    private void PlayCinematic()
    {
        if (currentPointIndex >= KeyPoints.Count - 1)
        {
            StopPlayback();
            return;
        }

        float elapsed = (Time.time - playingStartTime) * PlaybackSpeed;

        KeyPoint start = KeyPoints[currentPointIndex];
        KeyPoint end = KeyPoints[currentPointIndex + 1];

        float segmentDuration = end.Time - start.Time;
        if (segmentDuration <= 0.001f)
        {
            segmentDuration = 0.001f;
        }

        float segmentElapsed = elapsed - start.Time;
        float t = Mathf.Clamp01(segmentElapsed / segmentDuration);

        // Smooth curve interpolation
        Vector3 pos = GetCatmullRomPosition(currentPointIndex, t);
        playerTransform.Value.position = pos;

        // Smooth quaternion interpolation
        Quaternion rot = GetSquadRotation(currentPointIndex, t);
        playerTransform.Value.rotation = rot;

        if (segmentElapsed >= segmentDuration)
        {
            currentPointIndex++;
        }
    }

    private Vector3 GetCatmullRomPosition(int index, float t)
    {
        Vector3 p0 = KeyPoints[Mathf.Max(index - 1, 0)].Position;
        Vector3 p1 = KeyPoints[index].Position;
        Vector3 p2 = KeyPoints[Mathf.Min(index + 1, KeyPoints.Count - 1)].Position;
        Vector3 p3 = KeyPoints[Mathf.Min(index + 2, KeyPoints.Count - 1)].Position;

        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * (t * t) +
            (-p0 + 3f * p1 - 3f * p2 + p3) * (t * t * t)
        );
    }

    private Quaternion GetSquadRotation(int index, float t)
    {
        Quaternion q0 = KeyPoints[Mathf.Max(index - 1, 0)].Rotation;
        Quaternion q1 = KeyPoints[index].Rotation;
        Quaternion q2 = KeyPoints[Mathf.Min(index + 1, KeyPoints.Count - 1)].Rotation;
        Quaternion q3 = KeyPoints[Mathf.Min(index + 2, KeyPoints.Count - 1)].Rotation;

        // Ensure hemisphere consistency
        q0 = EnsureSameHemisphere(q1, q0);
        q2 = EnsureSameHemisphere(q1, q2);
        q3 = EnsureSameHemisphere(q2, q3);

        // Intermediate control quaternions for smooth squad interpolation
        Quaternion s1 = GetIntermediate(q0, q1, q2);
        Quaternion s2 = GetIntermediate(q1, q2, q3);

        s1 = EnsureSameHemisphere(q1, s1);
        s2 = EnsureSameHemisphere(q2, s2);

        float dot = Mathf.Abs(Quaternion.Dot(q1, q2));
        if (dot < 0.05f)
        {
            return Quaternion.Slerp(q1, q2, t); // safer fallback
        }

        return Squad(q1, q2, s1, s2, t);
    }

    private Quaternion Squad(Quaternion q1, Quaternion q2, Quaternion s1, Quaternion s2, float t)
    {
        Quaternion slerp1 = Quaternion.Slerp(q1, q2, t);
        Quaternion slerp2 = Quaternion.Slerp(s1, s2, t);
        return Quaternion.Slerp(slerp1, slerp2, 2 * t * (1 - t));
    }

    private Quaternion GetIntermediate(Quaternion q0, Quaternion q1, Quaternion q2)
    {
        Quaternion q1Inv = Quaternion.Inverse(q1);

        Vector3 p0 = Logarithm(q1Inv * q0);
        Vector3 p2 = Logarithm(q1Inv * q2);

        Vector3 sum = (p0 + p2) * -0.25f;

        Quaternion exp = Exponential(sum);
        return q1 * exp;
    }

    private Vector3 Logarithm(Quaternion q)
    {
        q = q.normalized;

        float a = Mathf.Acos(Mathf.Clamp(q.w, -1f, 1f));
        float sina = Mathf.Sin(a);

        if (Mathf.Abs(sina) < 0.0001f)
        {
            return new Vector3(q.x, q.y, q.z);
        }

        float coeff = a / sina;
        return new Vector3(q.x * coeff, q.y * coeff, q.z * coeff);
    }

    private Quaternion Exponential(Vector3 v)
    {
        float a = v.magnitude;
        float sina = Mathf.Sin(a);

        Quaternion result = new() { w = Mathf.Cos(a) };

        if (Mathf.Abs(a) > 0.0001f)
        {
            float coeff = sina / a;
            result.x = v.x * coeff;
            result.y = v.y * coeff;
            result.z = v.z * coeff;
        }
        else
        {
            result.x = v.x;
            result.y = v.y;
            result.z = v.z;
        }
        return result.normalized;
    }

    private Quaternion EnsureSameHemisphere(Quaternion q1, Quaternion q2)
    {
        if (Quaternion.Dot(q1, q2) < 0f)
        {
            q2 = new Quaternion(-q2.x, -q2.y, -q2.z, -q2.w);
        }
        return q2;
    }

    [Serializable]
    public class KeyPoint
    {
        public float Time;
        public Vector3 Position;
        public Quaternion Rotation;
        public bool IsGenerated;

        public KeyPoint(float time, Vector3 position, Quaternion rotation, bool isGenerated = false)
        {
            Time = time;
            Position = position;
            Rotation = rotation;
            IsGenerated = isGenerated;
        }
    }
}
