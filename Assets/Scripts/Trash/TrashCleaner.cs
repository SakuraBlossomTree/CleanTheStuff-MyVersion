using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TrashCleaner : MonoBehaviour
{
    public Slider cleanUpSlider;
    private Trash currentTrash;
    private Coroutine cleaningCoroutine;

    public static Dictionary<string, int> trashCounts = new Dictionary<string, int>();
    public static int totalPoints;
    public static int trashCollected = 0;

    void Start()
    {
        if (cleanUpSlider != null)
            cleanUpSlider.gameObject.SetActive(false);
    }

    void Update()
    {
        // Auto-pickup: no mouse input needed
    }

    IEnumerator CleanUpCoroutine()
    {
        if (currentTrash == null) yield break;

        cleanUpSlider.gameObject.SetActive(true);
        cleanUpSlider.maxValue = currentTrash.cleanUpTime;
        cleanUpSlider.value = 0f;

        while (cleanUpSlider.value < cleanUpSlider.maxValue)
        {
            if (BackpackManager.Instance.IsBackpackFull())
            {
                BackpackManager.Instance.ShowNotification("Backpack Full!");
                cleanUpSlider.gameObject.SetActive(false);
                yield break;
            }

            cleanUpSlider.value += Time.deltaTime;
            yield return null;
        }

        if (cleanUpSlider.value >= cleanUpSlider.maxValue)
        {
            string trashType = currentTrash.name.Replace("(Clone)", "");

            if (trashCounts.ContainsKey(trashType))
                trashCounts[trashType]++;
            else
                trashCounts[trashType] = 1;

            totalPoints += currentTrash.points;
            trashCollected++;

            // ==========================================
            // DEBUG LOG: Shows the counter going up
            // ==========================================
            Debug.Log($"🗑️ Trash Collected! trashCollected = {trashCollected}");

            BackpackManager.Instance.ShowNotification($"Collected {trashType} +{currentTrash.points} points");

            Destroy(currentTrash.gameObject);
            currentTrash = null;
            cleanUpSlider.gameObject.SetActive(false);
        }

        cleaningCoroutine = null;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Trash"))
        {
            currentTrash = other.GetComponent<Trash>();

            if (cleaningCoroutine == null)
            {
                cleaningCoroutine = StartCoroutine(CleanUpCoroutine());
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Trash") && currentTrash != null && other.gameObject == currentTrash.gameObject)
        {
            currentTrash = null;
            cleanUpSlider.gameObject.SetActive(false);

            if (cleaningCoroutine != null)
            {
                StopCoroutine(cleaningCoroutine);
                cleaningCoroutine = null;
            }
        }
    }

    public static void ResetProgress()
    {
        trashCounts.Clear();
        totalPoints = 0;
        trashCollected = 0;
        Debug.Log("Progress Reset! trashCollected = 0");
    }
}