using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Instantiate a rigidbody then set the velocity

public class Particle : MonoBehaviour{

    public Vector2 mPosition = new Vector2(0.0f, 0.0f);
    public Vector2 mVelocity = new Vector2(0.0f, 0.0f);

    public Vector2 mPressureForce = new Vector2(0.0f, 0.0f);
    public Vector2 mAccelerationForce = new Vector2(0.0f, 0.0f);

    public float mMass = Constants.MASS;
    public float mRho = Constants.RHO_0; // density
    public float mPressure = 0.0f; // presssure

    public Cell mCell; // to get the neighbours

    public ArrayList GetNeighbours(){
        return mCell.mParticles;
    }

    public Vector2 GetAcceleration(){
        return new Vector2(0.0f, -Constants.G);
    }

    public void AssignGridCell(){
        float pX = mPosition[0];
        float pY = mPosition[1];

        int cX = (int)pX;
        int cY = (int)pY;

        mCell = Grid.mCells[cY,cX];
    }

    public void UpdatePosition(Vector2 newPos, Vector2 newVel){
        // update internal position
        mPosition = newPos;
        mVelocity = newVel;

        // update position in scene
        transform.position = mPosition;

        // update cell
        AssignGridCell();
    }

}