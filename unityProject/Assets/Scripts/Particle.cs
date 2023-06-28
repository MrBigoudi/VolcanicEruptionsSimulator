using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * A class representing a particle
*/
public class Particle : MonoBehaviour{
    /**
     * The particles radii
    */
    public static float sInitRadius = 0.2f;

    private static int _IdGenerator = 0;

    public float mRadius;
    public int _Id;

    /**
     * The particle's volume
    */
    public float mVolume;
    
    /**
     * The height of the particle
    */
    public float mHeight;

    /**
     * The height's gradient
    */
    public Vector3 mHeightGradient;

    /**
     * The height's laplacian
    */
    public float mHeightLaplacian;

    /**
     * The particle's velocity
    */
    public Vector3 mVelocity;

    /**
     * The cell in which the particle's in
    */
    public Cell mCell;

    /**
     * The particle's neighbours
    */
    public List<Particle> mNeighbours;

    /**
     * The number of neighbours
    */
    public int mNbNeighbours;

    public void Init(bool ghost){
        mRadius = ghost ? 0.0f : sInitRadius;
        mVolume = 200.0f/Constants.RHO_0; // mass / density;
        mHeight = ghost ? 0.0f : 2.0f*sInitRadius;
        mHeightGradient = new Vector3();
        mHeightLaplacian = 0.0f;
        mVelocity = new Vector3(1,1,1);
        mCell = null;
        _Id = _IdGenerator++;
        mNeighbours = new List<Particle>();
        mNbNeighbours = 1;
        this.transform.localScale = ghost ? new Vector3() : new Vector3(mRadius, mRadius, mRadius);
        AssignGridCell();
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
    }

    public void UpdateRadius(){
        mRadius = mHeight/2.0f;
        this.transform.localScale = new Vector3(mRadius, mRadius, mRadius);
    }

    /**
     * Update the position of the particle
     * @param newPos The Particle's new position
     * @param newVelocity The Particle's new velocity
     * @return True if the particlue should be delete
    */
    public bool UpdatePosition(Vector3 newPos, Vector3 newVelocity){
        transform.position = newPos;
        mVelocity = newVelocity;
        UpdateRadius();
        // update cell
        return AssignGridCell();
    }

    /**
     * Update the particle's color
    */
    public void UpdateColor(){
        Color color = mCell.mColor; // color depending on the grid cell
        // Color color = new Color(mHeight/sMaxHeight, 0.0f, 0.0f, 1.0f); // color depending on the height
        gameObject.GetComponent<Renderer>().material.color = color;
    }

    void OnDrawGizmosSelected(){
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, Constants.H);
    }

}