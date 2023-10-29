//MakeContract
using UnityEngine;
using UnityEngine.InputSystem;

public class MetalContractItem : MonoBehaviour
{
    private InputAction _interactAction;

    private void Awake()
    {
        _interactAction = FindObjectOfType<PlayerInput>().actions["Player/Interact"];
    }

    private void Update()
    {
        transform.Rotate(0, 50.0f * Time.deltaTime, 0);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _interactAction.performed += OnInteract;
        }
    }

    private void OnInteract(InputAction.CallbackContext obj)
    {
        Debug.Log("MetalContractItem::OnInteract");
        UIManager.Instance.LoadMetalContractUI(this);
    }

    //You can exit the Skill Selection UI if you move far away from the Game Object Contractor or press F again.
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _interactAction.RemoveAllBindingOverrides();
        }
    }

}