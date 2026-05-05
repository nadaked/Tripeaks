using UnityEngine;
using _Project.Scripts.Application.Presenters;

namespace _Project.Scripts.Presentation.Views
{
    public sealed class SlotClickView : MonoBehaviour
    {
        [SerializeField] private int slotIndex;
        [SerializeField] private Renderer renderer;
        public int SlotIndex => slotIndex;

        public void Init(int index)
        {
            slotIndex = index;
            
            if (renderer == null)
                renderer = GetComponent<Renderer>();
        }
        
        public void SetVisual(bool selectable, bool removed)
        {
            if (renderer == null)
                return;

            if (removed)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            if (selectable)
                renderer.material.color = Color.green;
            else
                renderer.material.color = Color.gray;
        }
    }
}