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
        // Ӧ������
        ApplySettings(useHeavySnow ? heavySnow : lightSnow);
    }

    void ApplySettings(SnowSettings settings)
    {
        if (settings.snowSystem != null)
        {
            var emission = settings.snowSystem.emission;
            var mainModule = settings.snowSystem.main;

            // ����ǿ��
            emission.rateOverTimeMultiplier = settings.intensity * 100f;

            // ������ϵͳ
            if (!settings.snowSystem.isPlaying)
            {
                settings.snowSystem.Play();
            }
        }

        if (settings.directionalLight != null)
        {
            // ���õƹ�
            settings.directionalLight.color = settings.lightColor;
            settings.directionalLight.intensity = settings.lightIntensity;
        }
    }

    // ��������Ϸ�ж�̬�л�����
    public void SetHeavySnow(bool heavy)
    {
        useHeavySnow = heavy;
        ApplySettings(useHeavySnow ? heavySnow : lightSnow);
    }
}