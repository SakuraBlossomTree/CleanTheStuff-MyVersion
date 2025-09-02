using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

public class TrashCleaner : MonoBehaviour
{
    public Slider cleanUpSlider; 
    public KeyCode cleanUpKey = KeyCode.E;
    private Trash currentTrash;
    private Coroutine cleaningCoroutine;

    public static System.Collections.Generic.Dictionary<string, int> trashCounts = new System.Collections.Generic.Dictionary<string, int>();
    public static int totalPoints; 

    void Start()
    {
        if (cleanUpSlider != null)
            cleanUpSlider.gameObject.SetActive(false);
    }

    void Update()
    {
        if (currentTrash != null && cleanUpSlider != null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (cleaningCoroutine == null)
                    cleaningCoroutine = StartCoroutine(CleanUpCoroutine());
            }
            else
            {
                if (cleaningCoroutine != null)
                {
                    StopCoroutine(cleaningCoroutine);
                    cleaningCoroutine = null;
                }
            }
        }
    }

    IEnumerator CleanUpCoroutine()
    {
        if (currentTrash == null) yield break;

        cleanUpSlider.gameObject.SetActive(true);
        cleanUpSlider.maxValue = currentTrash.cleanUpTime;
        cleanUpSlider.value = 0f;

        while (cleanUpSlider.value < cleanUpSlider.maxValue && Input.GetMouseButtonDown(0))
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

            // if (BackpackManager.Instance.IsBackpackFull())
            // {
            //     BackpackManager.Instance.ShowNotification("Backpack Full!");
            //     cleanUpSlider.gameObject.SetActive(false);
            //     yield break;
            // }

            if (trashCounts.ContainsKey(trashType))
                trashCounts[trashType]++;
            else
                trashCounts[trashType] = 1;

            totalPoints += currentTrash.points;

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
}
