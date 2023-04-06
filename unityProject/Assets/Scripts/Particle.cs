using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Instantiate a rigidbody then set the velocity

public class Particle : MonoBehaviour{

    public Vector3 mPosition = new Vector3();
    public Vector3 mVelocity = new Vector3();

    public Vector3 mPressureForce = new Vector3();
    public Vector3 mAccelerationForce = new Vector3();

    public static float mRadius = 0.2f; 
    public static float mVolume = (4.0f/3.0f)*Constants.PI*mRadius*mRadius*mRadius;
    public float mMass = mVolume * Constants.RHO_0; // volume * density

    public float mRho = Constants.RHO_0; // density
    public float mPressure = 0.0f; // presssure

    public Cell mCell = null; // to get the neighbours
    public ArrayList mNeighbours = new ArrayList(); // to get the neighbours

    public Vector3 GetAcceleration(){
        return new Vector3(0.0f, -Constants.G, 0.0f);
    }

    public void AssignGridCell(){
        // redo this part
        float pX = mPosition[0];
        float pY = mPosition[1];

        int cX = (int)((pX+(Grid.mWidth/2.0f))/Grid.mCellWidth);
        int cY = (int)((pY+(Grid.mHeight/2.0f))/Grid.mCellHeight);

        // Debug.Log("x: " + cX + ", y: " + cY + ", px: " + pX + ", py: " + pY);

        // update old grid
        if(mCell != null) mCell.mParticles.Remove(this);

        // update new grid
        mCell = Grid.mCells[cY,cX];
        Grid.mCells[cY,cX].mParticles.Add(this);
    }

    public void UpdateRigidBody(Vector3 newPos, Vector3 newVel){
        // update internal position
        mPosition = newPos;
        mVelocity = newVel;

        GetComponent<Rigidbody>().AddForce(mVelocity);

        // update cell
        AssignGridCell();
    }

}