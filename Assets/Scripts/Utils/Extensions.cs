using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public static class TransformExtensions
    {
        public static List<GameObject> FindObjectsWithTag(this Transform parent, string tag)
        {
            List<GameObject> taggedGameObjects = new List<GameObject>();

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.CompareTag(tag))
                {
                    taggedGameObjects.Add(child.gameObject);
                }

                if (child.childCount > 0)
                {
                    taggedGameObjects.AddRange(FindObjectsWithTag(child, tag));
                }
            }

            return taggedGameObjects;
        }

        public static GameObject FindObjectWithTag(this Transform parent, string tag)
        {
            GameObject found = null;

            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                
                if (child.CompareTag(tag))
                {
                    found = child.gameObject;
                    break;
                }

                if (child.childCount > 0)
                {
                    found = FindObjectWithTag(child, tag);

                    if (found != null) { break; }
                }
            }
            
            return found;
        }
    }
}