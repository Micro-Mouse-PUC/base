using UnityEngine;

public class Cheese : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Handle what happens when the player collects the cheese
            Debug.Log("Cheese collected! You win!");
            // For example, you can load a win screen or display a victory message
        }
    }
}
