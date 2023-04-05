using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Grid {
    public static int mNbCols;
    public static int mNbLines;
    public static int mCellWidth;
    public static int mCellHeight;
    public static int mWidth;
    public static int mHeight;

    public static Cell [,] mCells;

    public static void InitGrid(int nbCols, int nbLines){
        mNbCols = nbCols;
        mNbLines = nbLines;

        mCells = new Cell[nbLines,nbCols];
        
        mWidth = Screen.width;
        mHeight = Screen.height;
        mCellWidth = mWidth / nbCols;
        mCellHeight = mHeight / nbLines;

        for(int i=0; i<nbLines; i++){
            for(int j=0; j<nbLines; j++){
                int curX = j*mCellWidth;
                int curY = i*mCellHeight;
                mCells[i,j] = new Cell(curX, curY, mCellHeight, mCellWidth);
            }
        } 
    }
}