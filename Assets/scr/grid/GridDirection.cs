using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridDirection
{
    public static Vector2 FORWARD = new Vector2(0, 1);
    public static Vector2 LEFT = new Vector2(-1, 0);
    public static Vector2 RIGHT = new Vector2(1, 0);
    public static Vector2 BACKWARD = new Vector2(0, -1);
    public static Vector2 FORWARD_LEFT_DIAGONAL = new Vector2(-1, 1);
    public static Vector2 FORWARD_RIGHT_DIAGONAL = new Vector2(1, 1);
    public static Vector2 BACKWARD_LEFT_DIAGONAL = new Vector2(1, -1);
    public static Vector2 BACKWARD_RIGHT_DIAGONAL = new Vector2(-1, -1);
}
