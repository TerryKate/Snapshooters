using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpshipUnit : GridUnit
{
    protected Vector3Int mMovement = Vector3Int.one;

    public override bool[,] FindPossibleMoves()
    {
        moves = new bool[8, 8];

        ProjectPath(GridDirection.FORWARD, false, true);
        ProjectPath(GridDirection.BACKWARD);
        ProjectPath(GridDirection.LEFT);
        ProjectPath(GridDirection.RIGHT);
        //ProjectPath(GridDirection.FORWARD_LEFT_DIAGONAL);
        //ProjectPath(GridDirection.FORWARD_RIGHT_DIAGONAL);
        //ProjectPath(GridDirection.BACKWARD_LEFT_DIAGONAL);
        //ProjectPath(GridDirection.BACKWARD_RIGHT_DIAGONAL);

        return moves;
    }

    public override bool[,] FindPossibleActions()
    {
        actions = new bool[8, 8];
        actionsArea = new bool[8, 8];

        ProjectPath(GridDirection.FORWARD, true);
        ProjectPath(GridDirection.BACKWARD, true);
        ProjectPath(GridDirection.LEFT, true);
        ProjectPath(GridDirection.RIGHT, true);
        //ProjectPath(GridDirection.FORWARD_LEFT_DIAGONAL, true);
        //ProjectPath(GridDirection.FORWARD_RIGHT_DIAGONAL, true);
        //ProjectPath(GridDirection.BACKWARD_LEFT_DIAGONAL, true);
        //ProjectPath(GridDirection.BACKWARD_RIGHT_DIAGONAL, true);

        return actions;

    }
}
