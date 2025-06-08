using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    [System.Serializable]
    public class SnowSettings
    {
        public ParticleSystem snowSystem;
        public Light directionalLight;
        [Range(0f, 1f)]
        public float intensity = 0.5f;
        public Color lightColor = Color.white;
        [Range(0.1f, 1f)]
        public float lightIntensity = 0.7f;
    }

    [SerializeField] private SnowSettings lightSnow;
    [SerializeField] private SnowSettings heavySnow;

    [SerializeField] private bool useHeavySnow = false;

    void Start()
    {
        // 应用设置
        ApplySettings(useHeavySnow ? heavySnow : lightSnow);
    }

    void ApplySettings(SnowSettings settings)
    {
        if (settings.snowSystem != null)
        {
            var emission = settings.snowSystem.emission;
            var mainModule = settings.snowSystem.main;

            // 设置强度
            emission.rateOverTimeMultiplier = settings.intensity * 100f;

            // 打开粒子系统
            if (!settings.snowSystem.isPlaying)
            {
                settings.snowSystem.Play();
            }
        }

        if (settings.directionalLight != null)
        {
            // 设置灯光
            settings.directionalLight.color = settings.lightColor;
            settings.directionalLight.intensity = settings.lightIntensity;
        }
    }

    // 可以在游戏中动态切换天气
    public void SetHeavySnow(bool heavy)
    {
        useHeavySnow = heavy;
        ApplySettings(useHeavySnow ? heavySnow : lightSnow);
    }
}