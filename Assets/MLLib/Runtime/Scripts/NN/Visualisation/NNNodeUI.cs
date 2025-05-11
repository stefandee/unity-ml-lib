namespace PironGames.MLLib.NN.Visualisation
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class NNNodeUI : MonoBehaviour
    {
        public TMP_Text ValueText;

        public TMP_Text BiasText;

        public Image Background;

        public int Index { get; set; }

        public void SetValues(int index, float input, float bias)
        {
            Index = index;
            ValueText.text = input.ToString("n2");
            BiasText.text = bias.ToString("n2");
        }
    }
}