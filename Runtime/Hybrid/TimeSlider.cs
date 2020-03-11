using Unity.Entities;
using UnityEngine.UI;
using UnityEngine;
using static Unity.Mathematics.math;

namespace Xmaho
{
    public class TimeSlider : MonoBehaviour
    {
        public Slider slider;
        private LocalTimeSystem localTimeSystem;
        private float initDeltaTime;
        private EntityManager manager;
        private EntityQuery query;

        void Start()
        {
            if (slider == null) {
                slider = GetComponent<Slider>();
                if (slider == null) {
                    Debug.LogError("TimeSlider: Require slider");
                    Destroy(this);
                }
            }
            localTimeSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<LocalTimeSystem>();
            initDeltaTime = Time.fixedDeltaTime;
            manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            query = manager.CreateEntityQuery(ComponentType.ReadOnly<LocalTime>());
        }

        public void UpdateTimeFactor()
        {
            var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            if (slider.value < 0) {
                manager.AddComponent<IsBackToTheFuture>(query);
            } else {
                manager.RemoveComponent<IsBackToTheFuture>(query);
            }
            Time.timeScale = abs(slider.value);
            Time.fixedDeltaTime = initDeltaTime * Time.timeScale;
        }
    }
}
