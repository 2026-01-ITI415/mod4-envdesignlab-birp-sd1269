using UnityEngine;

public class WaterRespawnTrigger : MonoBehaviour
{
    [Header("Respawn Settings")]
    public Transform respawnPoint;

    [Header("Optional")]
    public bool resetVelocity = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        RespawnPlayer(other.gameObject);
    }

    private void RespawnPlayer(GameObject player)
    {
        CharacterController characterController = player.GetComponent<CharacterController>();
        Rigidbody rb = player.GetComponent<Rigidbody>();

        // CharacterController must be disabled before moving the player,
        // otherwise Unity may fight the teleport.
        if (characterController != null)
        {
            characterController.enabled = false;
        }

        if (resetVelocity && rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        player.transform.position = respawnPoint.position;
        player.transform.rotation = respawnPoint.rotation;

        if (characterController != null)
        {
            characterController.enabled = true;
        }
    }
}