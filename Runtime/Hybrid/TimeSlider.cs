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
using UnityEngine.UI;
using UnityEngine;
using static Unity.Mathematics.math;

namespace Xmaho
{
    public class TimeSlider : MonoBehaviour
    {
        public Slider slider;
        private float initDeltaTime;
        private EntityManager manager;
        private EntityQuery query;
        private LocalTimeFactor cache;

        void Start()
        {
            if (slider == null) {
                slider = GetComponent<Slider>();
                if (slider == null) {
                    Debug.LogError("TimeSlider: Require slider");
                    Destroy(this);
                }
            }
            initDeltaTime = Time.fixedDeltaTime;
            manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            query = manager.CreateEntityQuery(typeof(LocalTimeFactor));
            cache = new LocalTimeFactor();
        }

        public void UpdateTimeFactor()
        {
            cache.Value = slider.value < 0 ? -1 : 1;
            using (var entities = query.ToEntityArray(Unity.Collections.Allocator.TempJob)) {
                foreach (var entitiy in  entities) {
                    manager.SetComponentData(entitiy, cache);
                }
            }
            Time.timeScale = abs(slider.value);
            Time.fixedDeltaTime = initDeltaTime * Time.timeScale;
        }
    }
}
