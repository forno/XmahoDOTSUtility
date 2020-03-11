/* MIT License

Copyright (c) 2020 FORNO

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using static Unity.Mathematics.math;

namespace Xmaho
{
    public struct SequentialPositionsBlobAsset
    {
        public BlobArray<float3> Positions;
    }

    [Serializable]
    public struct SequentialPositions : IComponentData
    {
        public BlobAssetReference<SequentialPositionsBlobAsset> BlobData;
        public Entity Parent;
    }

    public class SequentialPositionSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var localToWorldFromEntity = GetComponentDataFromEntity<LocalToWorld>(true);
            var deltaTime = UnityEngine.Time.fixedDeltaTime;
            if (deltaTime != 0) {
                Entities
                    .WithReadOnly(localToWorldFromEntity)
                    .ForEach((ref PhysicsVelocity velocity, ref Translation translation, in SequentialPositions positionsRef, in SequenceIndex index, in SequenceFrequency frequency) =>
                    {
                        ref var positions = ref positionsRef.BlobData.Value.Positions;
                        var validIndex = clamp(index.Value, 0, positions.Length - 1);
                        var nextIndex = clamp(index.Value + 1, 0, positions.Length - 1);
                        if (any(isnan(positions[nextIndex]))) {
                            nextIndex = validIndex;
                        }
                        var localPosition = transform(localToWorldFromEntity[positionsRef.Parent].Value, positions[validIndex]);
                        var localNextPosition = transform(localToWorldFromEntity[positionsRef.Parent].Value, positions[nextIndex]);
                        translation.Value = lerp(localPosition, localNextPosition, index.Fraction);
                        velocity.Linear = (localNextPosition - localPosition) * frequency.Value;
                    }).ScheduleParallel();
            } else {
                Entities
                    .WithReadOnly(localToWorldFromEntity)
                    .WithAll<SequenceFrequency>()
                    .ForEach((ref PhysicsVelocity velocity, ref Translation translation, in SequentialPositions positionsRef, in SequenceIndex index) =>
                    {
                        ref var positions = ref positionsRef.BlobData.Value.Positions;
                        var validIndex = clamp(index.Value, 0, positions.Length - 1);
                        var nextIndex = clamp(index.Value + 1, 0, positions.Length - 1);
                        if (any(isnan(positions[nextIndex]))) {
                            nextIndex = validIndex;
                        }
                        var localPosition = transform(localToWorldFromEntity[positionsRef.Parent].Value, positions[validIndex]);
                        var localNextPosition = transform(localToWorldFromEntity[positionsRef.Parent].Value, positions[nextIndex]);
                        translation.Value = lerp(localPosition, localNextPosition, index.Fraction);
                        velocity.Linear = Unity.Mathematics.float3.zero;
                    }).ScheduleParallel();
            }
        }
    }

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class SequentialPositionInitialSystem : SystemBase
    {
        private EndInitializationEntityCommandBufferSystem m_CommandBufferSystem;

        protected override void OnCreate()
        {
            m_CommandBufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var commandBuffer1 = m_CommandBufferSystem.CreateCommandBuffer().ToConcurrent();
            var job1 = Entities
                .ForEach((Entity entity, int entityInQueryIndex, in SequentialPositions positionsRef, in SequenceIndex index) =>
                {
                    ref var positions = ref positionsRef.BlobData.Value.Positions;
                    var validIndex = clamp(index.Value, 0, positions.Length - 1);
                    if (any(isnan(positions[validIndex]))) {
                        commandBuffer1.AddComponent<Disabled>(entityInQueryIndex, entity);
                    }
                }).ScheduleParallel(Dependency);

            var commandBuffer2 = m_CommandBufferSystem.CreateCommandBuffer().ToConcurrent();
            var job2 = Entities
                .WithAll<Disabled>()
                .ForEach((Entity entity, int entityInQueryIndex, in SequentialPositions positionsRef, in SequenceIndex index) =>
                {
                    ref var positions = ref positionsRef.BlobData.Value.Positions;
                    var validIndex = clamp(index.Value, 0, positions.Length - 1);
                    if (!any(isnan(positions[validIndex]))) {
                        commandBuffer2.RemoveComponent<Disabled>(entityInQueryIndex, entity);
                    }
                }).ScheduleParallel(Dependency);

            m_CommandBufferSystem.AddJobHandleForProducer(job1);
            m_CommandBufferSystem.AddJobHandleForProducer(job2);

            Dependency = JobHandle.CombineDependencies(job1, job2);
        }
    }
}
