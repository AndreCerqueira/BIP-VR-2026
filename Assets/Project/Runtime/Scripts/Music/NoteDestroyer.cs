using UnityEngine;

namespace Project.Runtime.Scripts.Music
{
    [RequireComponent(typeof(Collider))]
    public class NoteDestroyer : MonoBehaviour
    {
        private void Awake()
        {
            var col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            var fallingNote = other.GetComponent<FallingNoteView>();
            if (fallingNote == null) return;
            
            Destroy(fallingNote.gameObject);
        }
    }
}