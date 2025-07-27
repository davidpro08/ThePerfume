using NUnit.Framework.Interfaces;
using UnityEngine;

public class Well : MonoBehaviour, IInteract
{
    public bool CanInteract(Player player)
    {
        ItemData itemData = InventoryManager.Instance.EquippedItem();
        if (itemData == null)
        {
            Debug.Log("[Well] equipped item is null");
            return false;
        }
        ToolData toolData = Caster.CastTo<ToolData>(itemData);

        if (toolData.toolType == ToolType.WateringCan)
        {
            if (toolData.nowDurability != toolData.maxDurability)
            {
                return true;
            }
            else
            {
                Debug.Log("물뿌리개는 이미 찼음");
                return false; // 물뿌리개 이미 최대참
            }
        }
        else
        {
            Debug.Log("물뿌리개가 아님!");
            return false; // 물뿌리개 아님
        }

    }

    public void Interact(Player player)
    {
        if (!CanInteract(player)) return;
        Debug.Log("Start Interaction");
        ItemData itemData = InventoryManager.Instance.EquippedItem();
        ToolData toolData = Caster.CastTo<ToolData>(itemData);

        switch (toolData.toolType)
        {
            case ToolType.WateringCan:
                // 물 채우는 애니메이션 넣을거면 여기
                toolData.nowDurability = toolData.maxDurability; // 내구도 리셋
                break;
        }
    }

}
