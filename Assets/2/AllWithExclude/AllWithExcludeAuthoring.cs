using System;
using Unity.Entities;
using UnityEngine;

namespace _2.AllWithExclude
{
    public class AllWithExcludeAuthoring : MonoBehaviour
    {
        [SerializeField] private AllWithExcludeRespawnData settings_s;
        [SerializeField] private GameObject[] prefabs = Array.Empty<GameObject>();

        private class AllWithExcludeBaker : Baker<AllWithExcludeAuthoring>
        {
            public override void Bake(AllWithExcludeAuthoring authoring)
            {
                var e = GetEntity(TransformUsageFlags.None);
                AddComponent(e, authoring.settings_s);
                AddBuffer<PrefabElement>(e);
                foreach (var prefab in authoring.prefabs)
                {
                    AppendToBuffer(
                        e,
                        new PrefabElement()
                        {
                            entity = GetEntity(prefab, TransformUsageFlags.Dynamic)
                        });
                }
            }
        }
    }
}