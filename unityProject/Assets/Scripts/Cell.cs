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
    public List<Particle> mParticles = new List<Particle>();

    /**
     * The x coordinate of the cell
    */
    public float mX;

    /**
     * The z coordinate of the cell
    */
    public float mZ;

    /**
     * The depth of the cell
    */
    public float mDepth;

    /**
     * The width of the cell
    */
    public float mWidth;

    /**
     * The cell's color (for debugging purposes)
    */
    public Color mColor;

    /**
     * The nieghbouring cells
    */
    public List<Cell> mNeighbours = new List<Cell>();

    /**
     * A basic constructor
     * @param x The x position of the cell
     * @param z The z position of the cell
     * @param width The cell's width
     * @param depth The cell's depth
    */
    public Cell(float x, float y, float width, float depth){
        mX = x;
        mZ = y;
        mDepth = depth;
        mWidth = width;
    }

    /**
     * Get the particles arround the cell
     * @return The list of particles
    */
    public List<Particle> GetAllParticles(){
        List<Particle> res = new List<Particle>(mParticles);
        for(int i=0; i<mNeighbours.Count; i++){
            List<Particle> curList = ((Cell)mNeighbours[i]).mParticles;
            res.AddRange(curList);
            // for(int j=0; j<curList.Count; j++){
            //     res.Add(curList[j]);
            // }
        }
        return res;
    }
}