using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Metaball
{
    private List<Ball> balls = new List<Ball>();

    public Metaball() { }

    public void AddBall(Ball ball)
    {
        balls.Add(ball);
    }

    public void AddBall(float radius, Vector3 position, MetaballFunction function = MetaballFunction.Polynomial2)
    {
        Ball newBall = new Ball(radius, position, function);
        balls.Add(newBall);
    }

    public void AddCapsule(Segment segment, MetaballFunction function = MetaballFunction.Polynomial2)
    {
        Capsule newCapsule = new Capsule(segment, function);
        balls.Add(newCapsule);
    }

    public void AddCone(Segment segment, float tipThickness, MetaballFunction function = MetaballFunction.Polynomial2)
    {
        Cone newCone = new Cone(segment, tipThickness, function);
        balls.Add(newCone);
    }

    public float Value(float x, float y, float z)
    {
        float result = 0f;
        foreach(Ball ball in balls)
        {
            result += ball.Value(x, y, z);
        }
        return result;
    }

    /// <summary>
    /// Builds a metaball from the provided segments
    /// </summary>
    /// <param name="segments">Array of segments</param>
    /// <param name="function">The MetaballFunction to use</param>
    /// <returns>A Metaball made up of balls distributed along the provided segments</returns>
    public static Metaball BuildFromSegments(Segment[] segments, MetaballFunction function = MetaballFunction.Polynomial2, float variation=0.75f, bool useCapsules= true)
    {
        Metaball metaball = new Metaball();

        if (useCapsules)
        {
            foreach (Segment segment in segments)
            {
                metaball.AddCapsule(segment);
            }
        }
        else
        {
            foreach (Segment segment in segments)
            {
                int numBalls = Ball.GetMinimumNumBalls(function, segment.GetLength(), segment.thickness);

                Vector3 fwd = segment.GetEndPoint() - segment.GetStartPoint();
                Vector3 toMidPoint = fwd / (2 * numBalls);

                for (float i = 0; i <= numBalls; i++)
                {
                    Vector3 position = segment.GetStartPoint() + (i / numBalls) * fwd;
                    Vector3 randomDirection = new Vector3(RandomGaussian(-toMidPoint.magnitude, toMidPoint.magnitude),
                        RandomGaussian(-toMidPoint.magnitude, toMidPoint.magnitude),
                        RandomGaussian(-toMidPoint.magnitude, toMidPoint.magnitude)) * variation;
                    metaball.AddBall(Mathf.Abs(RandomGaussian(0.5f, 1.5f) * variation) * segment.thickness, position + randomDirection, function);
                    metaball.AddBall(segment.thickness, position, function);
                }
            }
        }
        return metaball;
    }

    public static Metaball BuildFromSkeleton(SkeletonDefinition skeletonDefinition, MetaballFunction function = MetaballFunction.ExponentialThin)
    {
        Metaball metaball = new Metaball();

        Stack<BoneDefinition> boneStk = new();
        boneStk.Push(skeletonDefinition.RootBone);

        Stack<Vector3> proximalPositions = new();
        proximalPositions.Push(Vector3.zero);

        Stack<Quaternion> rotations = new();
        rotations.Push(Quaternion.identity);

        while (boneStk.Count > 0)
        {
            BoneDefinition bone = boneStk.Pop();
            Vector3 proximalPos = proximalPositions.Pop();
            Vector3 proximalAxis = bone.ProximalAxis;
            if (bone.AttachmentHint.Offset != null)
                proximalPos += bone.AttachmentHint.Offset.Value;
            Quaternion rotation = rotations.Pop();
            if (bone.AttachmentHint.Rotation != null)
                rotation *= Quaternion.LookRotation(proximalAxis, bone.VentralAxis) * bone.AttachmentHint.Rotation.Value * Quaternion.Inverse(Quaternion.LookRotation(proximalAxis, bone.VentralAxis));
            proximalAxis = rotation * proximalAxis;
            Vector3 distalPos = proximalPos - bone.Length * proximalAxis;
            foreach(var child in bone.ChildBones)
            {
                boneStk.Push(child);
                proximalPositions.Push(Vector3.Lerp(proximalPos, distalPos, child.AttachmentHint.AttachmentPoint));
                rotations.Push(rotation);
            }

            metaball.AddCapsule(new(proximalPos, distalPos, bone.Thickness), function);
        }
        return metaball;
    }

    public static float RandomGaussian(float minValue = 0.0f, float maxValue = 1.0f)
    {
        float u, v, S;

        do
        {
            u = 2.0f * UnityEngine.Random.value - 1.0f;
            v = 2.0f * UnityEngine.Random.value - 1.0f;
            S = u * u + v * v;
        }
        while (S >= 1.0f);

        // Standard Normal Distribution
        float std = u * Mathf.Sqrt(-2.0f * Mathf.Log(S) / S);

        // Normal Distribution centered between the min and max value
        // and clamped following the "three-sigma rule"
        float mean = (minValue + maxValue) / 2.0f;
        float sigma = (maxValue - mean) / 3.0f;
        return Mathf.Clamp(std * sigma + mean, minValue, maxValue);
    }
}
