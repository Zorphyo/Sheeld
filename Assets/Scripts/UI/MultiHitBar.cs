using UnityEngine;
using UnityEngine.UI;

public class MultiHitBar : MonoBehaviour
{
    public Slider slider;
    private Image fill;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fill = slider.fillRect.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (ScoreManager.Instance == null) return;

        float value = ScoreManager.Instance.GetMultiHitTimerNormalized();
        slider.value = Mathf.Pow(value, 0.5f);;

        fill.color = Color.Lerp(Color.blue, Color.purple, value);

        slider.gameObject.SetActive(value > 0f);
    }
}
