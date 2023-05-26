using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * A class representing a particle
*/
public class Particle : MonoBehaviour{

    // public Vector3 mPosition = new Vector3();
    /**
     * The maximum density
    */
    public static float sMaxRho = Constants.RHO_0;

    /**
     * The maximum viscosity
    */
    public static float sMaxVisc = Constants.VISC;

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
     * The height's gradient
    */
    public Vector3 mHeightGradient = new Vector3();

    /**
     * The height's laplacian
    */
    public float mHeightLaplacian = 0.0f;

    /**
     * The particle's radius
    */
    public float mRadius = sRadius; 

    /**
     * The particles radii
    */
    public static float sRadius = 0.35f; 

    /**
     * The particle's volume
    */
    public static float mVolume = (4.0f/3.0f)*Constants.PI*sRadius*sRadius*sRadius;

    /**
     * The particle's mass
    */
    public float mMass = mVolume * Constants.RHO_0; // volume * density

    /**
     * The particle's density
    */
    public float mRho = Constants.RHO_0;

    /**
     * The particle's viscosity
    */
    public float mVisc = Constants.VISC;

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
     * The height of the particle
    */
    public float mHeight = sRadius;

    // public Vector3 mU = new Vector3();
    // public Vector3 mULap = new Vector3();

    /**
     * Get the acceleration force applied on the particle
     * @return The force as a vec3
    */
    public Vector3 GetAcceleration(){
        return new Vector3(0.0f, -mMass*Constants.G, 0.0f);
    }

    /**
     * Assign a grid cell to the particle
     * @return False if no cell could have been assigned
    */
    public bool AssignGridCell(){
        float pX = GetPosition()[0];
        float pZ = GetPosition()[2];

        int cX = (int)(pX/Grid.mCellWidth);
        int cZ = (int)(pZ/Grid.mCellDepth);

        // Debug.Log("x: " + cX + ", z: " + cZ + ", px: " + pX + ", pz: " + pZ);

        // update grid
        if(cX <= 0 || cX >= Grid.mNbCols || cZ <= 0 || cZ >= Grid.mNbLines){ // outside of the grid
            // Debug.Log("outside grid");
            if(mCell != null) mCell.mParticles.Remove(this);
            return false; 
        }
        Cell newCell = Grid.mCells[cX,cZ];

        if(mCell == null){ // first assign
            mCell = newCell;
            mCell.mParticles.Add(this);
            // gameObject.GetComponent<Renderer>().material.color = mCell.mColor;
            return true;
        }

        if(newCell != mCell) { // if same cell do nothing
            mCell.mParticles.Remove(this);
            newCell.mParticles.Add(this);
            // gameObject.GetComponent<Renderer>().material.color = newCell.mColor;
            mCell = newCell;
        }

        return true;
    }

    /**
     * Get the particle's position
     * @return The postion as a vec3
    */
    public Vector3 GetPosition(){
        return transform.position;
        // return GetComponent<Rigidbody>().position;
    }

    /**
     * Update the rigid body of the particle
     * @param newPos The Particle's new position
     * @param newVel The Particle's new velocity
     * @return True if the particlue should be delete
    */
    public bool UpdateRigidBody(Vector3 newPos, Vector3 newVel){
        // update internal position
        // mPosition = newPos;
        mVelocity = newVel;

        // GetComponent<Rigidbody>().AddForce(mVelocity);
        // GetComponent<Rigidbody>().MovePosition(newPos);
        // mPosition = GetComponent<Rigidbody>().position;
        transform.position = newPos;

        // update cell
        return AssignGridCell();
    }

    /**
     * Update the height
    */
    public void UpdateHeight(){
        mHeight = mRho / Constants.RHO_0;
        // float oldRadius = mRadius;
        mRadius = mHeight / 2.0f;

        // make sphere bigger
        // float scaleFactor = 0.0f;
        // if(oldRadius != 0.0f) scaleFactor = mRadius / oldRadius;

        // transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
    }

    /**
     * Update the particle's mass
    */
    public void UpdateMass(){
        mMass = mVolume * mRho;
    }

    /**
     * Update the particle's color
    */
    public void UpdateColor(){
        //Color color = newCell.mColor; // color depending on the grid cell
        Color color = new Color(mRho/Particle.sMaxRho, 0.0f, 0.0f, 1.0f); // color depending on the density
        // Color color = new Color(mVisc/Particle.sMaxVisc, 0.0f, 0.0f, 1.0f); // color depending on the density

        gameObject.GetComponent<Renderer>().material.color = color;
    }

}