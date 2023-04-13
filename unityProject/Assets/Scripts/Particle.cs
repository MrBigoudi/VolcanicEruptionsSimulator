using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * A class representing a particle
*/
public class Particle : MonoBehaviour{

    // public Vector3 mPosition = new Vector3();

    /**
     * The particle's velocity
    */
    public Vector3 mVelocity = new Vector3();

    /**
     * The pressure force applied on the particle
    */
    public Vector3 mPressureForce = new Vector3();

    /**
     * The acceleration force applied on the particle
    */
    public Vector3 mAccelerationForce = new Vector3();

    /**
     * The viscosity force applied on the particle
    */
    public Vector3 mViscosityForce = new Vector3();

    /**
     * The particle's radius
    */
    public static float mRadius = 0.35f; 

    /**
     * The particle's volume
    */
    public static float mVolume = (4.0f/3.0f)*Constants.PI*mRadius*mRadius*mRadius;

    /**
     * The particle's mass
    */
    public float mMass = mVolume * Constants.RHO_0; // volume * density

    /**
     * The particle's density
    */
    public float mRho = Constants.RHO_0;

    /**
     * The particle's pressure
    */
    public float mPressure = 0.0f;

    /**
     * The cell in which the particle's in
    */
    public Cell mCell = null;

    /**
     * The particle's neighbours
    */
    public ArrayList mNeighbours = new ArrayList();

    /**
     * Get the acceleration force applied on the particle
     * @return The force as a vec3
    */
    public Vector3 GetAcceleration(){
        return new Vector3(0.0f, -Constants.G, 0.0f);
    }

    /**
     * Assign a grid cell to the particle
    */
    public void AssignGridCell(){
        // TODO: redo this part
        float pX = GetPosition()[0];
        float pY = GetPosition()[1];

        int cX = (int)((pX+(Grid.mWidth/2.0f))/Grid.mCellWidth);
        int cY = (int)((pY+(Grid.mHeight/2.0f))/Grid.mCellHeight);

        // Debug.Log("x: " + cX + ", y: " + cY + ", px: " + pX + ", py: " + pY);

        // update old grid
        if(mCell != null) mCell.mParticles.Remove(this);

        // update new grid
        mCell = Grid.mCells[cY,cX];
        Grid.mCells[cY,cX].mParticles.Add(this);
    }

    /**
     * Get the particle's position
     * @return The postion as a vec3
    */
    public Vector3 GetPosition(){
        return GetComponent<Rigidbody>().position;
    }

    /**
     * Update the rigid body of the particle
     * @param newPos The Particle's new position
     * @param newVel The Particle's new velocity
    */
    public void UpdateRigidBody(Vector3 newPos, Vector3 newVel){
        // update internal position
        // mPosition = newPos;
        mVelocity = newVel;

        // GetComponent<Rigidbody>().AddForce(mVelocity);
        GetComponent<Rigidbody>().MovePosition(newPos);
        // mPosition = GetComponent<Rigidbody>().position;
        // transform.position = mPosition;

        // update cell
        AssignGridCell();
    }

}