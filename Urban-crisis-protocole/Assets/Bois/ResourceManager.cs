using System.Collections.Generic;
using UnityEngine;

public static class ResourceManager
{
    private static readonly Dictionary<ResourceType, List<Transform>> resources = new();

    public static void RegisterResource(ResourceType type, Transform transform)
    {
        if (!resources.ContainsKey(type))
        {
            resources[type] = new List<Transform>();
        }
        resources[type].Add(transform);
    }
   

    // Return array for the resources of parameter type
    public static Transform[] GetResourceTransforms(ResourceType type) => 
    resources.ContainsKey(type) ? resources[type].ToArray() : new Transform[0];





// Monobehaviour

    //private Dictionary<ResourceType, Transform[]> resources;
   
     //[SerializeField] private Transform[] treeTransforms;

    /*void Awake()
    {
        resources = new Dictionary<ResourceType, Transform[]>
        {
            {ResourceType.Wood, treeTransforms}
        };


        public Transform[] GetResourceTransforms(ResourceType type) => resources[type];
    } */

}
