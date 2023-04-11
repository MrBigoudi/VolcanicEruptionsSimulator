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
     * The y coordinate of the cell
    */
    public int mY;

    /**
     * The heigth of the cell
    */
    public int mHeight;

    /**
     * The width of the cell
    */
    public int mWidth;

    /**
     * A basic constructor
    */
    public Cell(int x, int y, int height, int width){
        mX = x;
        mY = y;
        mHeight = height;
        mWidth = width;
    }
}