using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * A static class representing the grid for the sph's neighbour search
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
     * The cells height
    */
    public static int mCellHeight;

    /**
     * The grid width
    */
    public static int mWidth;

    /**
     * The grid height
    */
    public static int mHeight;

    /**
     * The cells as a 2D array
    */
    public static Cell [,] mCells;

    /**
     * Initiate the grid
    */
    public static void InitGrid(){
        mCellWidth = (int)Constants.H * 4;
        mCellHeight = (int)Constants.H * 4;        
        mWidth = Screen.width;
        mHeight = Screen.height;
        mNbCols = mWidth / mCellWidth;
        mNbLines = mHeight / mCellHeight;

        mCells = new Cell[mNbLines, mNbCols];

        // init the cells
        for(int i=0; i<mNbLines; i++){
            for(int j=0; j<mNbCols; j++){
                int curX = j*mCellWidth;
                int curY = i*mCellHeight;
                mCells[i,j] = new Cell(curX, curY, mCellHeight, mCellWidth);
            }
        }
    }
}