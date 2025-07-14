using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class InventoryManagerTests
{
    private InventoryManager inventoryManager;
    private ItemData stackableItem;
    private ItemData nonStackableItem;

    [SetUp]
    public void Setup()
    {
        // InventoryManager는 MonoBehaviour이므로 GameObject에 추가해야 합니다.
        GameObject inventoryGameObject = new GameObject();
        inventoryManager = inventoryGameObject.AddComponent<InventoryManager>();

        // 테스트를 위해 ItemSlot 리스트를 초기화합니다.
        // 실제 게임에서는 인벤토리 UI 등에서 슬롯이 생성될 수 있습니다.
        inventoryManager.itemSlots = new List<ItemSlot>();
        for (int i = 0; i < inventoryManager.capacity; i++)
        {
            inventoryManager.itemSlots.Add(new ItemSlot(null, 0));
        }

        // 테스트용 ItemData 생성
        stackableItem = ScriptableObject.CreateInstance<ItemData>();
        stackableItem.itemName = "Stackable Potion";
        stackableItem.isStackable = true;
        stackableItem.maxStack = 10;
        stackableItem.itemType = ItemType.Crop;

        nonStackableItem = ScriptableObject.CreateInstance<ItemData>();
        nonStackableItem.itemName = "Non-Stackable Sword";
        nonStackableItem.isStackable = false;
        nonStackableItem.maxStack = 1; // Non-stackable items have max stack size of 1
        nonStackableItem.itemType = ItemType.Essence;
    }

    [TearDown]
    public void Teardown()
    {
        // 테스트 후 생성된 GameObject를 파괴하여 씬을 정리합니다.
        if (inventoryManager != null && inventoryManager.gameObject != null)
        {
            Object.DestroyImmediate(inventoryManager.gameObject);
        }
    }

    // AddItem Tests
    [Test]
    public void AddNonStackableItem_ToEmptyInventory_AddsSuccessfully()
    {
        // Arrange
        int initialSlotCount = inventoryManager.itemSlots.Count;

        // Act
        bool result = inventoryManager.AddItem(nonStackableItem, 1);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(1, inventoryManager.itemSlots.Count(slot => slot.itemData == nonStackableItem));
        Assert.AreEqual(1, inventoryManager.itemSlots.First(slot => slot.itemData == nonStackableItem).quantity);
    }

    [Test]
    public void AddNonStackableItem_ToFullInventory_Fails()
    {
        // Arrange
        // 인벤토리를 nonStackableItem으로 가득 채웁니다.
        for (int i = 0; i < inventoryManager.capacity; i++)
        {
            inventoryManager.itemSlots[i].itemData = nonStackableItem;
            inventoryManager.itemSlots[i].quantity = 1;
        }

        // Act
        bool result = inventoryManager.AddItem(nonStackableItem, 1);

        // Assert
        Assert.IsFalse(result); // 추가 실패해야 합니다.
        Assert.AreEqual(inventoryManager.capacity, inventoryManager.itemSlots.Count(slot => slot.itemData == nonStackableItem)); // 아이템 수가 변하지 않아야 합니다.
    }

    [Test]
    public void AddStackableItem_ToEmptyInventory_AddsSuccessfully()
    {
        // Arrange
        // Act
        bool result = inventoryManager.AddItem(stackableItem, 5);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(1, inventoryManager.itemSlots.Count(slot => slot.itemData == stackableItem));
        Assert.AreEqual(5, inventoryManager.itemSlots.First(slot => slot.itemData == stackableItem).quantity);
    }

    [Test]
    public void AddStackableItem_ToExistingStack_StacksSuccessfully()
    {
        // Arrange
        inventoryManager.AddItem(stackableItem, 5); // 5개 추가

        // Act
        bool result = inventoryManager.AddItem(stackableItem, 3); // 3개 더 추가

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(1, inventoryManager.itemSlots.Count(slot => slot.itemData == stackableItem)); // 여전히 1개의 슬롯만 차지해야 합니다.
        Assert.AreEqual(8, inventoryManager.itemSlots.First(slot => slot.itemData == stackableItem).quantity); // 총 8개가 되어야 합니다.
    }

    [Test]
    public void AddStackableItem_ExceedingMaxStackSize_CreatesNewStack()
    {
        // Arrange
        inventoryManager.AddItem(stackableItem, 8); // 8개 추가 (maxStackSize 10)

        // Act
        bool result = inventoryManager.AddItem(stackableItem, 5); // 5개 더 추가 (2개는 기존 스택, 3개는 새 스택)

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(2, inventoryManager.itemSlots.Count(slot => slot.itemData == stackableItem)); // 2개의 슬롯을 차지해야 합니다.
        Assert.AreEqual(10, inventoryManager.itemSlots.Where(slot => slot.itemData == stackableItem).Sum(slot => slot.quantity)); // 총 13개가 되어야 합니다.
    }

    [Test]
    public void AddStackableItem_ToFullInventory_Fails()
    {
        // Arrange
        // 인벤토리를 stackableItem으로 가득 채웁니다.
        for (int i = 0; i < inventoryManager.capacity; i++)
        {
            inventoryManager.itemSlots[i].itemData = stackableItem;
            inventoryManager.itemSlots[i].quantity = stackableItem.maxStack;
        }

        // Act
        bool result = inventoryManager.AddItem(stackableItem, 1);

        // Assert
        Assert.IsFalse(result); // 추가 실패해야 합니다.
        Assert.AreEqual(inventoryManager.capacity * stackableItem.maxStack, inventoryManager.itemSlots.Where(slot => slot.itemData == stackableItem).Sum(slot => slot.quantity)); // 아이템 수가 변하지 않아야 합니다.
    }

    // RemoveItem Tests
    [Test]
    public void RemoveExistingItem_PartialQuantity_RemovesSuccessfully()
    {
        // Arrange
        inventoryManager.AddItem(stackableItem, 10); // 10개 추가

        // Act
        bool result = inventoryManager.RemoveItem(stackableItem, 3); // 3개 제거

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(7, inventoryManager.itemSlots.First(slot => slot.itemData == stackableItem).quantity);
    }

    [Test]
    public void RemoveExistingItem_ExactQuantity_RemovesSuccessfullyAndClearsSlot()
    {
        // Arrange
        inventoryManager.AddItem(stackableItem, 5); // 5개 추가

        // Act
        bool result = inventoryManager.RemoveItem(stackableItem, 5); // 5개 제거

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(0, inventoryManager.itemSlots.Count(slot => slot.itemData == stackableItem)); // 슬롯이 비워져야 합니다.
    }

    [Test]
    public void RemoveNonExistentItem_Fails()
    {
        // Arrange
        ItemData anotherItem = ScriptableObject.CreateInstance<ItemData>();
        anotherItem.itemName = "Another Item";

        // Act
        bool result = inventoryManager.RemoveItem(anotherItem, 1);

        // Assert
        Assert.IsFalse(result); // 제거 실패해야 합니다.
    }

    [Test]
    public void RemoveMoreThanAvailable_Fails()
    {
        // Arrange
        inventoryManager.AddItem(stackableItem, 5); // 5개 추가

        // Act
        bool result = inventoryManager.RemoveItem(stackableItem, 10); // 10개 제거 시도

        // Assert
        Assert.IsFalse(result); // 제거 실패해야 합니다.
        Assert.AreEqual(5, inventoryManager.itemSlots.First(slot => slot.itemData == stackableItem).quantity); // 아이템 수가 변하지 않아야 합니다.
    }

    [Test]
    public void InventoryChangedEvent_IsInvokedOnAddItem()
    {
        // Arrange
        bool eventInvoked = false;
        inventoryManager.onInventoryChangedCallback += () => eventInvoked = true;

        // Act
        inventoryManager.AddItem(stackableItem, 1);

        // Assert
        Assert.IsTrue(eventInvoked);
    }

    [Test]
    public void InventoryChangedEvent_IsInvokedOnRemoveItem()
    {
        // Arrange
        inventoryManager.AddItem(stackableItem, 5);
        bool eventInvoked = false;
        inventoryManager.onInventoryChangedCallback += () => eventInvoked = true;

        // Act
        inventoryManager.RemoveItem(stackableItem, 1);

        // Assert
        Assert.IsTrue(eventInvoked);
    }
}
