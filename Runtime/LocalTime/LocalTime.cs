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

using Unity.Entities;
using Unity.Jobs;

namespace Xmaho
{
    [GenerateAuthoringComponent]
    public struct LocalTime : IComponentData
    {
        public float Value;
    }

    public struct IsBackToTheFuture : IComponentData { }

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(UpdateWorldTimeSystem))]
    public class LocalTimeSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var deltaTime = Time.DeltaTime;
            var job0 = Entities
                .WithEntityQueryOptions(EntityQueryOptions.IncludeDisabled)
                .WithNone<IsBackToTheFuture>()
                .ForEach((ref LocalTime time) =>
                {
                    time.Value += deltaTime;
                }).ScheduleParallel(Dependency);

            var job1 = Entities
                .WithEntityQueryOptions(EntityQueryOptions.IncludeDisabled)
                .WithAll<IsBackToTheFuture>()
                .ForEach((ref LocalTime time) =>
                {
                    time.Value -= deltaTime;
                }).ScheduleParallel(Dependency);

            Dependency = JobHandle.CombineDependencies(job0, job1);
        }
    }
}
