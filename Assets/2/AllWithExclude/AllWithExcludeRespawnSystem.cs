using System;
using Latios;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;


public partial struct PrefabElement : IBufferElementData
{
    public Entity entity;
}

[Serializable]
public partial struct AllWithExcludeRespawnData : IComponentData
{
    public float radius;
    public float intensity;
}

public partial struct DestroyMe : IComponentData
{
    public double at;
}

[BurstCompile]
public partial struct ALlWithExcludeRespawnSystem : ISystem
{
    private float _delay;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EndInitializationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<AllWithExcludeRespawnData>();
        state.InitSystemRng(3289048239);
        _delay = 0f;
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        _delay += SystemAPI.Time.DeltaTime;
        var settings = SystemAPI.GetSingleton<AllWithExcludeRespawnData>();
        var ecbSingleton = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        var rng = state.GetMainThreadRng();
        var et = SystemAPI.Time.ElapsedTime;
        
        
        foreach (var (d, entity) in SystemAPI.Query<RefRO<DestroyMe>>().WithEntityAccess())
        {
            if (d.ValueRO.at <= et) ecb.DestroyEntity(entity);
        }
        
        
        if (_delay <= settings.intensity) return;
        _delay = 0f;
        var prefabs = SystemAPI.GetSingletonBuffer<PrefabElement>(true);
        for (int i = 0; i < settings.intensity; i++)
        {
            var prefab = prefabs[rng.NextInt(0, prefabs.Length)];
            var spawned = ecb.Instantiate(prefab.entity);
            ecb.SetComponent(
                spawned,
                new LocalTransform()
                {
                    Position = rng.NextFloat3Direction() * settings.radius,
                    Rotation = quaternion.identity,
                    Scale = 1f,
                });
            ecb.AddComponent(spawned, new DestroyMe() { at = et + 2f });
        }

    }
}