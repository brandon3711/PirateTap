using UnityEngine;

public class LevelTester : MonoBehaviour
{
    public BackgroundManagerUI backgroundManager;
    public KeyCode key = KeyCode.Space; // touche de test

    int currentLevel = 0;

    void Update()
    {
        if (Input.GetKeyDown(key))
        {
            currentLevel++;
            Debug.Log("Niveau " + currentLevel);
            if (backgroundManager != null)
                backgroundManager.ApplyForLevel(currentLevel);
        }
    }
}
