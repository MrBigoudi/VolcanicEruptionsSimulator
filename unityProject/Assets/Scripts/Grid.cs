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