using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Base class for reading symmetric joint pairs</summary>
public abstract class JointPairsReader : MonoBehaviour
{
    public abstract Tuple<Transform, Transform>[] ReadJointPairs(); // if can't read joint pairs return null

}
