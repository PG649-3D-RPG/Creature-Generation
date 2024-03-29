using System.Collections.Generic;
using UnityEngine;

public class BipedInstance : ISettingsInstance
{ 

    [ObservationOrder(19)] public readonly int NumArmBones;
    [ObservationOrder(2)] public readonly List<float> ArmThicknesses;
    [ObservationOrder(3)] public readonly List<float> ArmLengths;

    [ObservationOrder(4)] public readonly float HandRadius;

    [ObservationOrder(18)] public readonly int NumLegBones;
    [ObservationOrder(5)] public readonly List<float> LegLengths;
    [ObservationOrder(6)] public readonly List<float> LegThicknesses;

    [ObservationOrder(7)] public readonly float FeetWidth;
    [ObservationOrder(8)] public readonly float FeetLength;

    [ObservationOrder(17)] public readonly int NumTorsoBones;
    [ObservationOrder(9)] public readonly List<float> TorsoWidths;
    [ObservationOrder(10)] public readonly List<float> TorsoLengths;
    [ObservationOrder(20)] public readonly float TorsoRatio;

    [ObservationOrder(11)] public readonly int NeckBones;
    [ObservationOrder(12)] public readonly float NeckBoneLength;
    [ObservationOrder(13)] public readonly float NeckThickness;

    [ObservationOrder(14)] public readonly float HeadSize;

    public BipedInstance(BipedSettings settings, int? seed)
    {
        if (seed.HasValue) {
            Random.InitState(seed.Value);
        }
        
        var numArmBones = settings.ArmBones.Sample();
        var armLengths = settings.ArmLength.Samples(numArmBones);
        var armThicknesses = settings.ArmThickness.Samples(numArmBones);
        armThicknesses.Sort((a, b) => b.CompareTo(a));

        var numLegBones = settings.LegBones.Sample();
        var legLengths = settings.LegLength.Samples(numLegBones);
        var legThicknesses = settings.LegThickness.Samples(numLegBones);
        legThicknesses.Sort((a, b) => b.CompareTo(a));

        var numTorsoBones = settings.TorsoBones.Sample();
        var torsoLengths = settings.TorsoLength.Samples(numTorsoBones);
        var torsoWidths = settings.TorsoWidth.Samples(numTorsoBones);

        NumArmBones = numArmBones;
        ArmLengths = armLengths;
        ArmThicknesses = armThicknesses;
        HandRadius = settings.HandRadius.Sample();
        NumLegBones = numLegBones;
        LegLengths = legLengths;
        LegThicknesses = legThicknesses;
        NumTorsoBones = numTorsoBones;
        FeetWidth = settings.FeetWidth.Sample();
        FeetLength = settings.FeetLength.Sample();
        TorsoWidths = torsoWidths;
        TorsoLengths = torsoLengths;
        NeckBones = settings.NeckBones.Sample();
        NeckBoneLength = settings.NeckLength.Sample();
        NeckThickness = settings.NeckThickness.Sample();
        HeadSize = settings.HeadSize.Sample();
        TorsoRatio = 0.4f;
    }
}
