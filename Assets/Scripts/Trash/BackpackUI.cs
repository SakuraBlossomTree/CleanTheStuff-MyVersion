using UnityEngine;
using TMPro;

public class BackpackUI : MonoBehaviour
{
    public static BackpackUI Instance;

    [System.Serializable]
    public class TrashSlot
    {
        public TrashType type;
        public TextMeshProUGUI countText;
        public GameObject highlight; // Optional: shown when this type is selected (used in Step 3)
    }

    public TrashSlot[] slots;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void Update()
    {
        RefreshCounts();
    }

    void RefreshCounts()
    {
        foreach (var slot in slots)
        {
            string key = slot.type.ToString();
            int count = TrashCleaner.trashCounts.ContainsKey(key) ? TrashCleaner.trashCounts[key] : 0;

            if (slot.countText != null)
                slot.countText.text = count.ToString();
        }
    }

    // Called by the throw system in Step 3 to highlight the selected type
    public void RefreshSelection(TrashType selected)
    {
        foreach (var slot in slots)
        {
            if (slot.highlight != null)
                slot.highlight.SetActive(slot.type == selected);
        }
    }
}