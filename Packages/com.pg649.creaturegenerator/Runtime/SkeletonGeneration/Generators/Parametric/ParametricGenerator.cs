using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using System;

using Random = UnityEngine.Random;

enum Mode {
    Biped,
    Quadruped
}

public class ParametricGenerator {
    private CreatureParameters parameters;

    private Mode mode;

    private BoneDefinition neckAttachmentBone;

    private BoneDefinition armAttachmentBone;

    private static Dictionary<(BoneCategory, BoneCategory), JointLimits> humanoidJointLimits = new Dictionary<(BoneCategory, BoneCategory), JointLimits>() {
        {(BoneCategory.Arm, BoneCategory.Arm), new JointLimits { XAxisMin = 20, XAxisMax = 160, YAxisSymmetric = 0, ZAxisSymmetric = 0}}
    };

    private static Dictionary<(BoneCategory, BoneCategory), JointLimits> quadrupedJointLimits = new Dictionary<(BoneCategory, BoneCategory), JointLimits>() {
        {(BoneCategory.Hip, BoneCategory.Leg), new JointLimits { XAxisMin = 50, XAxisMax = 0, YAxisSymmetric = 0, ZAxisSymmetric = 0}},
        {(BoneCategory.Leg, BoneCategory.LowerLeg1), new JointLimits { XAxisMin = -50, XAxisMax = 0, YAxisSymmetric = 0, ZAxisSymmetric = 0} },
        {(BoneCategory.LowerLeg1, BoneCategory.LowerLeg2), new JointLimits { XAxisMin = 50, XAxisMax = 0, YAxisSymmetric = 0, ZAxisSymmetric = 0}},
        {(BoneCategory.LowerLeg2, BoneCategory.Leg), new JointLimits { XAxisMin = -50, XAxisMax = 0, YAxisSymmetric = 0, ZAxisSymmetric = 0}}
    };


    public ParametricGenerator(CreatureParameters parameters) {
        this.parameters = parameters;
    }

    public SkeletonDefinition BuildCreature(int seed = 0) {
        if (seed != 0) {
            Random.InitState(seed);
        }
        int pairs = Random.Range(parameters.minLegPairs, parameters.maxLegPairs + 1);
        if (pairs == 1) {
            mode = Mode.Biped;
        } else {
            mode = Mode.Quadruped;
        }

        List<BoneDefinition> legs = buildLegs();
        List<BoneDefinition> arms = buildArms();

        BoneDefinition root = buildTorso();
        BoneDefinition neck = buildNeck(neckAttachmentBone);
        buildHead(neck);

        attachLegs(legs, root);

        foreach (var arm in arms)
            armAttachmentBone.LinkChild(arm);

        if (mode == Mode.Biped)
            return new SkeletonDefinition(root, new LimitTable(humanoidJointLimits));
        else
            return new SkeletonDefinition(root, new LimitTable(quadrupedJointLimits));
    }

    private List<BoneDefinition> buildArms() {
        List<BoneDefinition> arms = new();
        if (mode == Mode.Biped) {
            float armHeight = Random.Range(parameters.minArmSize, parameters.maxArmSize);
            float armSplit = Random.Range(0.25f * armHeight, 0.75f * armHeight);
            //legHeights = new List<float>() { legHeight };

            List<float> heights = new() {armSplit, armHeight - armSplit};

            List<float> thicknesses = new();
            thicknesses.Add(Random.Range(parameters.minArmThickness, parameters.maxArmThickness));
            thicknesses.Add(Random.Range(parameters.minArmThickness, parameters.maxArmThickness));
            //thicknesses.Sort();
            //thicknesses.Reverse();

            BoneDefinition leftRoot = buildArm(heights, thicknesses);
            BoneDefinition rightRoot = buildArm(heights, thicknesses);

            leftRoot.AttachmentHint.AttachmentPoint = AttachmentPoints.MidPoint;
            leftRoot.AttachmentHint.VentralDirection = Vector3.right;
            leftRoot.AttachmentHint.Offset = new Vector3(-0.75f, 0.0f, 0.0f);
            leftRoot.AttachmentHint.Rotation = Quaternion.Euler(-90.0f, 0.0f, 0.0f);

            rightRoot.AttachmentHint.AttachmentPoint = AttachmentPoints.MidPoint;
            rightRoot.AttachmentHint.VentralDirection = Vector3.left;
            rightRoot.AttachmentHint.Offset = new Vector3(0.75f, 0.0f, 0.0f);
            rightRoot.AttachmentHint.Rotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);

            arms.Add(leftRoot);
            arms.Add(rightRoot);
        }
        return arms;
    }

    private BoneDefinition buildArm(List<float> lengths, List<float> thicknesses) {
        Assert.AreEqual(lengths.Count, thicknesses.Count);
        Assert.IsTrue(lengths.Count > 0);

        BoneDefinition root = buildArmPart(lengths[0], thicknesses[0]);
        BoneDefinition prev = root;

        foreach (var (length, thickness) in lengths.Zip(thicknesses, (a, b) => (a, b)).Skip(1)) {
            BoneDefinition part = buildArmPart(length, thickness);
            prev.LinkChild(part);
            prev = part;
        }
        return root;
    }

    private BoneDefinition buildArmPart(float length, float thickness) {
        BoneDefinition part = new();

        part.Length = length;
        part.ProximalAxis = Vector3.up;
        part.VentralAxis = Vector3.forward;
        part.Category = BoneCategory.Arm;
        part.Thickness = thickness;

        return part;
    }

    private List<BoneDefinition> buildLegs() {
        List<BoneDefinition> legs = new();
        if (mode == Mode.Biped) {
            float legHeight = Random.Range(parameters.minLegSize, parameters.maxLegSize);
            float legSplit = Random.Range(0.25f * legHeight, 0.75f * legHeight);
            //legHeights = new List<float>() { legHeight };

            List<float> heights = new() {legSplit, legHeight - legSplit};

            List<float> thicknesses = new();
            thicknesses.Add(Random.Range(parameters.minLegThickness, parameters.maxLegThickness));
            thicknesses.Add(Random.Range(parameters.minLegThickness, parameters.maxLegThickness));
            thicknesses.Sort();
            thicknesses.Reverse();

            BoneDefinition leftLeg = buildLeg(heights, thicknesses);
            BoneDefinition rightLeg = buildLeg(heights, thicknesses);

            legs.Add(leftLeg);
            legs.Add(rightLeg);
        }
        // TODO: Port quadruped code
        else if (mode == Mode.Quadruped)
        {
            // quadruped
            float frontLegHeight = Random.Range(parameters.minLegSize, parameters.maxLegSize);
            float hindLegHeight = Random.Range(parameters.minLegSize, parameters.maxLegSize);
            //legHeights = new List<float>() { hindLegHeight, frontLegHeight };

            for (int i = 0; i < 2; i++)
            {
                float height = hindLegHeight;
                if (i == 1)
                    height = frontLegHeight;
                float legSplit = Random.Range(0.1f * height, 0.9f * height);
                float lowerLegSplit = Random.Range(legSplit + 0.25f * (height - legSplit), legSplit + 0.75f * (height - legSplit));
                float upperLegSplit = Random.Range(0.25f * legSplit, 0.75f * legSplit);

                List<float> heights = new() {upperLegSplit, legSplit-upperLegSplit, lowerLegSplit-legSplit, height-lowerLegSplit};

                List<float> thicknesses = new();

                thicknesses.Add(Random.Range(parameters.minLegThickness, parameters.maxLegThickness));
                thicknesses.Add(Random.Range(parameters.minLegThickness, parameters.maxLegThickness));
                thicknesses.Add(Random.Range(parameters.minLegThickness, parameters.maxLegThickness));
                thicknesses.Add(Random.Range(parameters.minLegThickness, parameters.maxLegThickness));
                thicknesses.Sort();
                thicknesses.Reverse();

                for (int j = -1; j < 2; j+=2)
                {
                    BoneDefinition leg = buildLeg(heights, thicknesses);
                    legs.Add(leg);
                }
            }
        }
        return legs;
    }

    private BoneDefinition buildLeg(List<float> lengths, List<float> thicknesses) {
        Assert.AreEqual(lengths.Count, thicknesses.Count);
        Assert.IsTrue(lengths.Count > 0);

        BoneDefinition root = buildLegPart(lengths[0], thicknesses[0]);
        BoneDefinition prev = root;
        JointLimits jointLimits = quadrupedJointLimits[(BoneCategory.Hip, root.Category)];
        //if(mode == Mode.Quadruped)
            //root.AttachmentHint.Rotation = Quaternion.Euler(Random.Range(jointLimits.XAxisMin, jointLimits.XAxisMax), 0f, 0f);

        for (int i = 1; i < lengths.Count; i++)
        {
            BoneDefinition part = buildLegPart(lengths[i], thicknesses[i], i);
            prev.LinkChild(part);
            /*if (mode == Mode.Quadruped && quadrupedJointLimits.ContainsKey((prev.Category, part.Category)))
            {
                jointLimits = quadrupedJointLimits[(prev.Category, part.Category)];
                part.AttachmentHint.Rotation = Quaternion.Euler(Random.Range(jointLimits.XAxisMin, jointLimits.XAxisMax), 0f, 0f);
            }*/
            prev = part;
        }
        return root;
    }

    private BoneDefinition buildLegPart(float length, float thickness, int index=0) {
        BoneDefinition part = new();

        part.Length = length;
        // Construct Legs pointing down, with front side facing forward.
        part.ProximalAxis = Vector3.up;
        part.VentralAxis = Vector3.forward;
        part.Category = BoneCategory.Leg;
        if (index == 1)
            part.Category = BoneCategory.LowerLeg1;
        else if (index == 2)
            part.Category = BoneCategory.LowerLeg2;
        /*else if (index == 3)
            part.Category = BoneCategory.Foot;*/
        part.Thickness = thickness;

        return part;
    }

    private BoneDefinition buildTorso() {
        float torsoSize = Random.Range(parameters.minTorsoSize, parameters.maxTorsoSize);

        List<float> torsoSplits = new List<float>()
        {
            Random.Range(0.1f * torsoSize, 0.9f * torsoSize)
        };
        do
        {
            float split = Random.Range(0.1f * torsoSize, 0.9f * torsoSize);
            if (Mathf.Abs(split - torsoSplits[0]) >= 0.1f * torsoSize)
                torsoSplits.Add(split);
        } while (torsoSplits.Count < 2);
        torsoSplits.Sort();

        if(mode == Mode.Biped || mode == Mode.Quadruped) {
            float thicknessBottom = Random.Range(parameters.minTorsoThickness, parameters.maxTorsoThickness);
            float thicknessMiddle = Random.Range(parameters.minTorsoThickness, parameters.maxTorsoThickness);
            float thicknessTop = Random.Range(parameters.minTorsoThickness, parameters.maxTorsoThickness);
            BoneDefinition bottom = buildTorsoPart(torsoSplits[0], thicknessBottom);
            BoneDefinition middle = buildTorsoPart(torsoSplits[1] - torsoSplits[0], thicknessMiddle);
            BoneDefinition top = buildTorsoPart(torsoSize - torsoSplits[1], thicknessTop);

            bottom.LinkChild(middle);
            middle.LinkChild(top);

            neckAttachmentBone = top;
            armAttachmentBone = top;

            return bottom;
        }
        return null;
    }

    private BoneDefinition buildTorsoPart(float length, float thickness) {
        BoneDefinition part = new();

        part.Length = length;
        // Construct Torso pointing down, with front side facing forward.
        if (mode == Mode.Biped)
        {
            part.ProximalAxis = Vector3.down;
            part.VentralAxis = Vector3.forward;
        }
        // Construct Torso pointing forward, with front side facing down.
        else if (mode == Mode.Quadruped)
        {
            part.ProximalAxis = Vector3.back;
            part.VentralAxis = Vector3.down;
        }
        part.Category = BoneCategory.Torso;
        part.AttachmentHint = new();
        part.Thickness = thickness;

        return part;
    }

    private BoneDefinition buildNeck(BoneDefinition attachTo) {
        int neckSegments = Random.Range(parameters.minNeckSegments, parameters.maxNeckSegments + 1);
        float neckSize = Random.Range(parameters.minNeckSize, parameters.maxNeckSize);
        float segmentLength = neckSize / neckSegments;
        float neckThickness = 0.2f;

        //tilt neck at random angle between being aligned with torso and 90 degrees upwards (quadruped)
        float angle = Random.Range(0f, 90f);

        BoneDefinition prev = attachTo;
        for (int i=0; i<neckSegments; i++)
        {
            BoneDefinition neckPart = buildNeckPart(segmentLength, neckThickness);
            if (mode == Mode.Quadruped)
            {
                neckPart.AttachmentHint.Rotation = Quaternion.Euler(angle, 0f, 0f);
                angle = Random.Range(-30f, 30f);
            }
            prev.LinkChild(neckPart);
            prev = neckPart;
        }
        return prev;
    }

    private BoneDefinition buildNeckPart(float length, float thickness) {
        BoneDefinition part = new();

        part.Length = length;
        // Construct Neck pointing down, with front side facing forward.
        part.ProximalAxis = Vector3.down;
        part.VentralAxis = Vector3.forward;
        part.Category = BoneCategory.Torso;
        part.AttachmentHint = new();
        part.Thickness = thickness;

        return part;
    }

    private BoneDefinition buildHead(BoneDefinition attachTo) {
        float headSize = Random.Range(parameters.minHeadSize, parameters.maxHeadSize);

        BoneDefinition head = new();
        attachTo.LinkChild(head);

        head.Length = headSize;
        head.ProximalAxis = Vector3.down;
        head.VentralAxis = Vector3.forward;
        head.Category = BoneCategory.Head;
        head.AttachmentHint = new();
        head.Thickness = headSize;

        return head;
    }

    private void attachLegs(List<BoneDefinition> legs, BoneDefinition torso)
    {
        float upDownAngle = 0f;
        float lrAngle = 0f;
        //set random hip rotation within limits
        if (mode == Mode.Biped)
        {
            upDownAngle = Random.Range(-40f, 40f);
        }
        else if (mode == Mode.Quadruped)
        {
            upDownAngle = Random.Range(-40f, 40f);
            lrAngle = Random.Range(-30, 30);
        }
        //build one hip segment for each leg with mirrored rotation
        buildHip(torso, legs[0], Vector3.left, rotation: Quaternion.Euler(upDownAngle, lrAngle, 0f));
        buildHip(torso, legs[1], Vector3.right, rotation: Quaternion.Euler(upDownAngle, -lrAngle, 0f));

        if (mode == Mode.Quadruped)
        {
            //make separate hip rotation for front legs
            upDownAngle = Random.Range(-40f, 40f);
            lrAngle = Random.Range(-30, 30);
            buildHip(neckAttachmentBone, legs[2], Vector3.left, AttachmentPoints.DistalPoint, Quaternion.Euler(lrAngle, upDownAngle, 0f));
            buildHip(neckAttachmentBone, legs[3], Vector3.right, AttachmentPoints.DistalPoint, Quaternion.Euler(lrAngle, -upDownAngle, 0f));
        }
    }
    
    private BoneDefinition buildHip(BoneDefinition attachTo, BoneDefinition leg, Vector3 proximalAxis, float attachmentPoint = AttachmentPoints.ProximalPoint, Quaternion? rotation = null)
    {
        BoneDefinition hip = new();
        hip.Length = attachTo.Thickness;
        hip.ProximalAxis = proximalAxis;
        hip.VentralAxis = Vector3.down;
        hip.Category = BoneCategory.Hip;
        hip.Thickness = 0f; // For now to avoid metaball generation around hip
        hip.AttachmentHint.AttachmentPoint = attachmentPoint;

        //make legs counter hip rotation so that they still go straight down
        hip.AttachmentHint.Rotation = rotation;
        float flipRotation = Vector3.Dot(proximalAxis, Vector3.left) > 0 ? 1f : -1f;
        if (leg.AttachmentHint.Rotation.HasValue)
            leg.AttachmentHint.Rotation *= Quaternion.Euler(0f, flipRotation * rotation.Value.eulerAngles.x, rotation.Value.eulerAngles.y);
        else
            leg.AttachmentHint.Rotation = Quaternion.Euler(0f, flipRotation * rotation.Value.eulerAngles.x, rotation.Value.eulerAngles.y);
        
        attachTo.LinkChild(hip);
        hip.LinkChild(leg);
        return hip;
    }
}