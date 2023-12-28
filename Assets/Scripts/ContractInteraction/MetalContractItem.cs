//MakeContract
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class MetalContractItem : Interactor
{
    private void Update()
    {
        transform.Rotate(0, 50.0f * Time.deltaTime, 0);
    }
    
    protected override void OnInteract(InputAction.CallbackContext obj)
    {
        // TEMP TEST
        //var relics = RelicManager.Instance.RandomChooseRelic_Store(3);
        //var relics = RelicManager.Instance.RandomChooseRelic_Field();

        //Debug.Log(string.Format("Here's the list: ({0}).", string.Join(", ", relics.Select(relic => relic.Name_KO).ToList())));
        //Debug.Log(relics.Name_KO + " " + relics.Rarity.ToString());

        //Debug.Log("MetalContractItem::OnInteract");
        //UIManager.Instance.LoadMetalContractUI(this);
    }
}