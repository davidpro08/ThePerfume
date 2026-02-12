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
                Debug.Log("���Ѹ����� �̹� á��");
                return false; // ���Ѹ��� �̹� �ִ���
            }
        }
        else
        {
            Debug.Log("���Ѹ����� �ƴ�!");
            return false; // ���Ѹ��� �ƴ�
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
                // �� ä��� �ִϸ��̼� �����Ÿ� ����
                SoundManager.Instance.PlaySFX(SFXType.WateringCan);
                toolData.nowDurability = toolData.maxDurability; // ������ ����
                InventoryManager.Instance.InventoryChanged();
                break;
        }
    }

}
