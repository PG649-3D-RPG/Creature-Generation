using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRigSolver
{

    public enum RigSolverType {
        ClosestBone, BoneHeat, VisibleBones, Metaball
    }

    public static IRigSolver Get(RigSolverType type) {
        if (type == RigSolverType.ClosestBone)
            return new ClosestBoneRigSolver();
        else if (type == RigSolverType.BoneHeat)
            return new BoneHeatRigSolver();
        else if (type == RigSolverType.VisibleBones)
            return new VisibleBonesRigSolver();
        else if (type == RigSolverType.Metaball)
            return new MetaballRigSolver();
        else
            return null;
    }


    void CalcBoneWeights(Mesh mesh, IVisibilityTester tester, Bone[] bones, Transform meshTransform, Metaball metaball);

}