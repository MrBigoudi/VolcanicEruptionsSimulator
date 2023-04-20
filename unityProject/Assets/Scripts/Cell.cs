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
}