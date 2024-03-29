using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bone : MonoBehaviour
{
    /// What type of body part this bone belongs to.
    public BoneCategory category;

    public BoneCategory? subCategory;

    public Color? color;

    public float length;

    public float thickness;

    /// The index number of the limb this bone belongs to, zero based. 
    /// A bone with category "Arm" and limbIndex 0 for example belong to the skeletons first arm.
    /// Bones with identical limb indices belong to the same limb.
    public int limbIndex;

    /// The index number of the bone within its limb.
    /// Taking an arm as an example, the bone connected to the torso has boneIndex 0, the bone below that has boneIndex 1, and so on.
    public int boneIndex;

    public bool isRoot;

    public float? width;
    /// <summary>
    ///  True if this bone is a mirrored version of another bone
    /// </summary>
    public bool mirrored;
    public Vector3 WorldProximalPoint() {
        return gameObject.transform.position;
    }

    public Vector3 WorldDistalPoint() {
        return WorldProximalPoint() + WorldDistalAxis() * length;
    }

    public Vector3 WorldMidpoint() {
        return WorldProximalPoint() + WorldDistalPoint() / 2.0f;
    }

    public Vector3 WorldDistalAxis() {
        return gameObject.transform.forward;
    }

    public Vector3 WorldVentralAxis() {
        return gameObject.transform.up;
    }

    public Vector3 WorldLateralAxis() {
        return gameObject.transform.right;
    }

    public Vector3 LocalProximalPoint() {
        return Vector3.zero;
        //return gameObject.transform.localPosition;
    }

    public Vector3 LocalDistalPoint() {
        return LocalProximalPoint() + LocalDistalAxis() * length;
    }

    public Vector3 LocalMidpoint() {
        return LocalProximalPoint() + LocalDistalPoint() / 2.0f;
    }

    public Vector3 LocalDistalAxis() {
        return Vector3.forward;
    }

    public Vector3 LocalLateralAxis() {
        return Vector3.right;
    }

    public Vector3 LocalVentralAxis() {
        return Vector3.up;
    }
}