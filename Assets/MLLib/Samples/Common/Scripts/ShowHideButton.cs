namespace PironGames.MLLib.Samples.Common
{
    using TMPro;
    using UnityEngine;

    public class ShowHideButton : MonoBehaviour
    {
        public GameObject Target;

        public TMP_Text Text;

        public bool IsOpen = false;

        // Use this for initialization
        void Start()
        {
            Configure();
        }

        public void OnClick()
        {
            IsOpen = !IsOpen;

            Configure();
        }

        void Configure()
        {
            Text.text = IsOpen ? "Hide" : "Show";
            Target.SetActive(IsOpen);
        }
    }
}