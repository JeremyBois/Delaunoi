using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Delaunoi.DataStructures.Extensions
{
    public static class Vec3Extensions
    {
        /// <summary>
        /// Create a new Vec3 from a UnityEngine.Vector3
        /// </summary>
        public static Vec3 FromVector3(this Vector3 vecStruct)
        {
            return new Vec3(vecStruct.x, vecStruct.y, vecStruct.z);
        }

        /// <summary>
        /// Convert to a Unity Vector3 structure.
        /// </summary>
        public static Vector3 AsVector3(this Vec3 vec)
        {
            return new Vector3((float)vec.x, (float)vec.y, (float)vec.z);
        }
    }
}

