using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * A class to manipulate cells for neighbour search
*/
public class Cell {

    /**
     * The list of particles inside the cell
    */
    public ArrayList mParticles = new ArrayList();

    /**
     * The x coordinate of the cell
    */
    public int mX;

    /**
     * The z coordinate of the cell
    */
    public int mZ;

    /**
     * The depth of the cell
    */
    public int mDepth;

    /**
     * The width of the cell
    */
    public int mWidth;

    /**
     * The cell's color (for debugging purposes)
    */
    public Color mColor;

    /**
     * The nieghbouring cells
    */
    public ArrayList mNeighbours = new ArrayList();

    /**
     * A basic constructor
     * @param x The x position of the cell
     * @param z The z position of the cell
     * @param width The cell's width
     * @param depth The cell's depth
    */
    public Cell(int x, int y, int width, int depth){
        mX = x;
        mZ = y;
        mDepth = depth;
        mWidth = width;
    }

    /**
     * Get the particles arround the cell
     * @return The list of particles
    */
    public ArrayList GetAllParticles(){
        ArrayList res = new ArrayList(mParticles);
        for(int i=0; i<mNeighbours.Count; i++){
            ArrayList curList = ((Cell)mNeighbours[i]).mParticles;
            res.AddRange(curList);
            // for(int j=0; j<curList.Count; j++){
            //     res.Add(curList[j]);
            // }
        }
        return res;
    }
}