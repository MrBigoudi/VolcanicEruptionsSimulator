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
    public static float sInitRadius = 0.35f;

    /**
     * The particles max height
    */
    public static float sMaxHeight = 2*sInitRadius;

    /**
     * The particle's mass
    */
    public float mMass = (4.0f/3.0f)*Constants.PI*sInitRadius*sInitRadius*sInitRadius * Constants.RHO_0; // volume * density
    
    /**
     * The height of the particle
    */
    public float mHeight = 2*sInitRadius;

    /**
     * The height's gradient
    */
    public Vector3 mHeightGradient = new Vector3();

    /**
     * The cell in which the particle's in
    */
    public Cell mCell = null;

    /**
     * The particle's neighbours
    */
    public ArrayList mNeighbours = new ArrayList();

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

    /**
     * Update the position of the particle
     * @param newPos The Particle's new position
     * @return True if the particlue should be delete
    */
    public bool UpdatePosition(Vector3 newPos){
        transform.position = newPos;
        // update cell
        return AssignGridCell();
    }

    /**
     * Update the particle's color
    */
    public void UpdateColor(){
        //Color color = newCell.mColor; // color depending on the grid cell
        Color color = new Color(mHeight/sMaxHeight, 0.0f, 0.0f, 1.0f); // color depending on the height
        gameObject.GetComponent<Renderer>().material.color = color;
    }

}