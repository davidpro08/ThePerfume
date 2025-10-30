using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Custom/FarmRuleTile")]
public class FarmRuleTile: RuleTile<FarmRuleTile.Neighbor>
{
    public class Neighbor
    {
        public const int FarmGroup = 3;
    }

    public override bool RuleMatch(int neighbor, TileBase tile)
    {
        if (neighbor == Neighbor.FarmGroup)
        {
            return tile is FarmRuleTile || tile is RuleOverrideTile;
        }
        return base.RuleMatch(neighbor, tile);
    }

}
