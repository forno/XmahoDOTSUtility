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
