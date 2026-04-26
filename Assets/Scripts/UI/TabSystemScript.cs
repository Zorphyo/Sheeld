using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TabSystemScript : MonoBehaviour
{
    public List<RectTransform> tabs;
    public List<Button> tabButtons;
    private List<Vector3> originalScales = new List<Vector3>();
    public float selectedMultiplier = 1.1f;
    public int defaultTabIndex = 0;

    void Start()
    {
        foreach (var tab in tabs)
        {
            originalScales.Add(tab.localScale);
        }

        SelectTab(defaultTabIndex);
        tabButtons[defaultTabIndex].onClick.Invoke();
    }

    public void SelectTab(int index)
    {
        for (int i = 0; i < tabs.Count; i++)
        {
            if (i == index)
            {
                tabs[i].localScale = originalScales[i] * selectedMultiplier;
                tabs[i].SetAsLastSibling();   
            }
            else
                tabs[i].localScale = originalScales[i];
        }
    }
}