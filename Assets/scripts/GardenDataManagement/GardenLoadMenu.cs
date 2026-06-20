using UnityEngine;
using UnityEngine.Events;

namespace GardenDataManagement
{
    public class GardenLoadMenu : MonoBehaviour
    {
        [SerializeField] private Transform gardenContainer;
        [SerializeField] private GardenListItem gardenItemPrefab;
        
        public UnityEvent<string> onGardenSelected;

        private void OnEnable()
        {
            PopulateGardenList();
        }

        private void PopulateGardenList()
        {
            // Clear old entries
            foreach (Transform child in gardenContainer)
            {
                Destroy(child.gameObject);
            }

            string[] gardenNames = Datainterface.GetAllGardenNames();

            foreach (string gardenName in gardenNames)
            {
                GardenListItem item = Instantiate(gardenItemPrefab, gardenContainer);
                item.Setup(gardenName, HandleGardenClicked);
            }
        }

        private void HandleGardenClicked(string gardenName)
        {
            onGardenSelected?.Invoke(gardenName);
            gameObject.SetActive(false);
        }
    }
}