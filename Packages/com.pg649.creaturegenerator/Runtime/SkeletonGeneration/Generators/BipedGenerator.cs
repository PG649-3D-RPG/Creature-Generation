using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class BipedGenerator {
    private BipedInstance instance;

    private BoneDefinition neckAttachmentBone;

    private BoneDefinition shoulderAttachmentBone;

    private static readonly Dictionary<(BoneCategory, BoneCategory), JointLimits> HumanoidJointLimits = new Dictionary<(BoneCategory, BoneCategory), JointLimits>() {
        {(BoneCategory.Arm, BoneCategory.Arm), new JointLimits { XAxisMin = -10, XAxisMax = 160, Axis = Vector3.up, SecondaryAxis = Vector3.forward }},
        {(BoneCategory.Shoulder, BoneCategory.Arm), new JointLimits { XAxisMin = -90, XAxisMax = 160, ZAxisSymmetric = 90, Axis = Vector3.up, SecondaryAxis = Vector3.forward }},
        {(BoneCategory.Torso, BoneCategory.Torso), new JointLimits { XAxisMin = -10, XAxisMax = 10, YAxisSymmetric = 10 }},
        {(BoneCategory.Torso, BoneCategory.Hip), new JointLimits { XAxisMin = -10, XAxisMax = 10, YAxisSymmetric = 10 }},
        {(BoneCategory.Torso, BoneCategory.Head), new JointLimits() { XAxisMin = -90, XAxisMax = 90, YAxisSymmetric = 45 }},
        {(BoneCategory.Hip, BoneCategory.Leg), new JointLimits() { XAxisMin = -45, XAxisMax = 120, YAxisSymmetric = 45 }},
        {(BoneCategory.Leg, BoneCategory.Leg), new JointLimits() { XAxisMin = -90, XAxisMax = 10 }},
        {(BoneCategory.Arm, BoneCategory.Hand), new JointLimits() { XAxisMin = -180, XAxisMax = 180, YAxisSymmetric = 45, ZAxisSymmetric = 90, Axis = Vector3.forward }},
        {(BoneCategory.Leg, BoneCategory.Foot), new JointLimits() { XAxisMin = -10, XAxisMax = 10, YAxisSymmetric = 45 }}
    };

    public SkeletonDefinition BuildCreature(BipedSettings settings, int? seed, JointLimitOverrides limitOverrides) {
        instance = new BipedInstance(settings, seed);
        List<BoneDefinition> legs = buildLegs();
        BoneDefinition shoulder = buildArms();

        BoneDefinition root = buildTorso();
        var hip = buildHip(root, legs[0], legs[1], Vector3.down, Vector3.forward, RelativePositions.ProximalPoint);

        shoulderAttachmentBone.LinkChild(shoulder);
        neckAttachmentBone = shoulder;

        BoneDefinition neck = buildNeck(neckAttachmentBone);
        buildHead(neck);

        root.AttachmentHint.Offset = new(0, instance.LegLengths.Sum() + hip.Length + SkeletonAssembler.FootHeight, 0);

        LimitTable jointLimits = new(HumanoidJointLimits);
        if (limitOverrides != null)
            jointLimits.Add(limitOverrides.ToLimitTable());
        return new SkeletonDefinition(root, jointLimits, instance);
    }

    private BoneDefinition buildArms() {
        List<BoneDefinition> arms = new();
        var leftRoot = buildArm(instance.ArmLengths, instance.ArmThicknesses, false);
        leftRoot.AttachmentHint.Rotation = Quaternion.Euler(0f, -90f, 0f);
        leftRoot.AttachmentHint.Position = new RelativePosition(1f, 0.0f, 0.5f);
        
        var rightRoot = buildArm(instance.ArmLengths, instance.ArmThicknesses, true);
        rightRoot.AttachmentHint.Rotation = Quaternion.Euler(0f, 90f, 0f);
        rightRoot.AttachmentHint.Position = new RelativePosition(-1f, 0.0f, 0.5f);

        var shoulder = buildShoulder();
        shoulder.LinkChild(leftRoot);
        shoulder.LinkChild(rightRoot);

        return shoulder;
    }

    private BoneDefinition buildShoulder()
    {
        float width = instance.TorsoWidths[instance.TorsoWidths.Count - 1] + instance.ArmThicknesses[0] * 2f;
        return new BoneDefinition
        {
            Length = 2f * instance.ArmThicknesses[0],
            DistalAxis = Vector3.up,
            VentralAxis = Vector3.forward,
            Category = BoneCategory.Shoulder,
            Width = width,
            Thickness = width * instance.TorsoRatio,
            Mirrored = false
        };
    }

    private BoneDefinition buildArm(List<float> lengths, List<float> thicknesses, bool mirrored = false)
    {
        var (root, end) = GeneratorUtils.BuildLimb(lengths, thicknesses, (length, thickness, index) =>
            new BoneDefinition()
            {
                Length = length,
                Thickness = thickness,
                DistalAxis = Vector3.down,
                VentralAxis = Vector3.forward,
                Category = BoneCategory.Arm,
                Mirrored = mirrored,
            });
        
        BoneDefinition hand = new()
        {
            DistalAxis = Vector3.down,
            VentralAxis = Vector3.forward,
            Length = instance.HandRadius,
            Thickness = instance.HandRadius,
            Category = BoneCategory.Hand,
            AttachmentHint = new AttachmentHint()
        };

        end.LinkChild(hand);
        return root;
    }

    private List<BoneDefinition> buildLegs() {
        List<BoneDefinition> legs = new()
        {
            buildLeg(instance.LegLengths, instance.LegThicknesses),
            buildLeg(instance.LegLengths, instance.LegThicknesses)
        };
        return legs;
    }

    private BoneDefinition buildLeg(List<float> lengths, List<float> thicknesses) {
        var (root, end) = GeneratorUtils.BuildLimb(lengths, thicknesses, (length, thickness, _) =>
            // Construct Legs pointing down, with front side facing forward.
            new BoneDefinition()
            {
                Length = length,
                DistalAxis = Vector3.down,
                VentralAxis = Vector3.forward,
                Category = BoneCategory.Leg,
                AttachmentHint = new AttachmentHint(),
                Thickness = thickness,
            });

        BoneDefinition foot = new()
        {
            DistalAxis = Vector3.forward,
            VentralAxis = Vector3.up,
            Length = instance.FeetLength,
            Thickness = SkeletonAssembler.FootHeight,
            Category = BoneCategory.Foot,
            Width = instance.FeetWidth,
            AttachmentHint = new AttachmentHint
            {
                Position = new RelativePosition(0.0f, 0.0f, 1.05f),
            }
        };

        end.LinkChild(foot);
        return root;
    }

    private BoneDefinition buildTorso() {
        var (bottom, top) = GeneratorUtils.BuildLimb(instance.TorsoLengths, instance.TorsoWidths,
            (length, width, _) => new BoneDefinition()
            {
                // Construct Torso pointing up, with front side facing forward.
                Length = length,
                DistalAxis = Vector3.up,
                VentralAxis = Vector3.forward,
                Category = BoneCategory.Torso,
                AttachmentHint = new AttachmentHint(),
                Width = width,
                Thickness = width * instance.TorsoRatio,
            });
        shoulderAttachmentBone = top;

        return bottom;
    }

    private BoneDefinition buildNeck(BoneDefinition attachTo) {
        var prev = attachTo;
        for (var i = 0; i < instance.NeckBones; i++)
        {
            var neckPart = buildNeckPart(instance.NeckBoneLength, instance.NeckThickness);
            prev.LinkChild(neckPart);
            prev = neckPart;
        }
        return prev;
    }

    private BoneDefinition buildNeckPart(float length, float thickness) {
        // Construct Neck pointing up, with front side facing forward.
        BoneDefinition part = new()
        {
            Length = length,
            DistalAxis = Vector3.up,
            VentralAxis = Vector3.forward,
            Category = BoneCategory.Torso,
            AttachmentHint = new AttachmentHint(),
            Thickness = thickness
        };

        return part;
    }

    private BoneDefinition buildHead(BoneDefinition attachTo) {
        BoneDefinition head = new()
        {
            Length = instance.HeadSize,
            DistalAxis = Vector3.up,
            VentralAxis = Vector3.forward,
            Category = BoneCategory.Head,
            AttachmentHint = new AttachmentHint(),
            Thickness = instance.HeadSize
        };
        attachTo.LinkChild(head);

        return head;
    }

    private BoneDefinition buildHip(BoneDefinition attachTo, BoneDefinition leg1, BoneDefinition leg2, Vector3 distalAxis, Vector3 ventralAxis, RelativePosition pos)
    {
        float width = Mathf.Max(instance.TorsoWidths.Average(), leg1.Thickness*5f);
        BoneDefinition hip = new()
        {
            Length = width * instance.TorsoRatio,
            Category = BoneCategory.Hip,
            DistalAxis = distalAxis,
            VentralAxis = ventralAxis,
            Width = width,
            Thickness = width * instance.TorsoRatio,
            AttachmentHint =
            {
                Position = pos
            }
        };

        attachTo.LinkChild(hip);
        hip.LinkChild(leg1);
        leg1.AttachmentHint.Position = new RelativePosition(1f - (leg1.Thickness/(width/2f)), 0.0f, 1f);
        hip.LinkChild(leg2);
        leg2.AttachmentHint.Position = new RelativePosition(-1f + (leg1.Thickness / (width / 2f)), 0.0f, 1f);

        return hip;
    }
}