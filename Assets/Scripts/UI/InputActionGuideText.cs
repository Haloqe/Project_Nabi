using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputActionGuideText : MonoBehaviour
{
        public TextMeshProUGUI displayText;
        public InputActionReference actionReference;

        private void OnEnable()
        { 
                displayText.text = actionReference.action.bindings[0].ToDisplayString();
        }
        
        //InputBinding.DisplayStringOptions.DontUseShortDisplayNames
}