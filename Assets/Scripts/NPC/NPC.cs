using System;

namespace NPC
{
    public class NPC : IInteract
    {
        public void Interact(Player player)
        {
            
        }

        public bool CanInteract(Player player)
        {
            Interact(player);
            return true;
        }
    }
}