using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * A static class representing the grid for the sph's neighbour search
 * The grid is a 2D one at y = 0
*/
public static class Grid {
    /**
     * The number of columns
    */
    public static int mNbCols;

    /**
     * The number of lines
    */
    public static int mNbLines;

    /**
     * The cells width
    */
    public static int mCellWidth;

    /**
     * The cells depth
    */
    public static int mCellDepth;

    /**
     * The grid width
    */
    public static int mWidth;

    /**
     * The grid depth
    */
    public static int mDepth;

    /**
     * The cells as a 2D array
    */
    public static Cell [,] mCells;

    /**
     * Initiate the grid
    */
    public static void InitGrid(){
        mCellWidth = (int)Constants.H * 4;
        mCellDepth = (int)Constants.H * 4;        
        mWidth = 2*Screen.width;
        mDepth = 2*Screen.height;
        mNbCols = mWidth / mCellWidth;
        mNbLines = mDepth / mCellDepth;

        mCells = new Cell[mNbLines, mNbCols];

        // init the cells
        for(int i=mNbLines-1; i>=0; i--){
            for(int j=0; j<mNbCols; j++){
                int curX = i*mCellWidth;
                int curZ = j*mCellDepth;
                mCells[i,j] = new Cell(curX, curZ, mCellWidth, mCellDepth);
            }
        }
    }
}