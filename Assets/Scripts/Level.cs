using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "ColorSort/Level")]
public class Level : ScriptableObject
{
    public Color BackGroundColor;
    public Sprite ImageSprite;

    public int Row;
    public int Col;

    public List<Vector2Int> LockedCells;
}
