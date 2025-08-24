using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Custom/FarmRuleTile")]
public class FarmRuleTile: RuleTile<FarmRuleTile.Neighbor>
{
    public class Neighbor: RuleTile.TilingRule.Neighbor
    {
        public const int FarmGroup = 3;
    }

    public FarmState defaultState = FarmState.Dry;

    public enum FarmState { Dry, Watered, Planted }

    private static readonly Dictionary<Vector3Int, FarmState> farmStates = new Dictionary<Vector3Int , FarmState>();

    public Sprite[] drySprites;
    public Sprite[] wateredSprites;
    public Sprite[] plantedSprites;

    public override bool RuleMatch(int neighbor, TileBase tile)
    {
        if (neighbor == Neighbor.FarmGroup)
        {
            return tile is FarmRuleTile || tile is RuleOverrideTile;
        }
        return base.RuleMatch(neighbor, tile);
    }


    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        base.GetTileData(position, tilemap, ref tileData);

        FarmState state;
        if (!farmStates.TryGetValue(position, out state))
        {
            state = defaultState;
        }

        Sprite baseSprite = tileData.sprite;
        int spriteIndex = System.Array.IndexOf(drySprites, baseSprite);

        if(spriteIndex >= 0)
        {
            switch (state)
            {
                case FarmState.Dry:
                    tileData.sprite = drySprites[spriteIndex];
                    break;
                case FarmState.Watered:
                    tileData.sprite = wateredSprites[spriteIndex];
                    break;
                case FarmState.Planted:
                    tileData.sprite = plantedSprites[spriteIndex];
                    break;
            }
        }
        
    }

    public static void SetFarmState(Vector3Int position, FarmState newState, Tilemap tilemap)
    {
        farmStates[position] = newState;
        tilemap.RefreshTile(position);
    }


}
