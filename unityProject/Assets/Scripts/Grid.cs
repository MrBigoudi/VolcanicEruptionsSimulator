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
        mCellWidth = (int)Constants.H;
        mCellDepth = (int)Constants.H;
        Vector3 terrainSize = Terrain.activeTerrain.terrainData.size;
        mWidth = (int)(terrainSize.x+1);
        mDepth = (int)(terrainSize.z+1);

        mNbCols = mWidth / mCellWidth;
        mNbLines = mDepth / mCellDepth;
        int iMax = mNbLines-1;
        int jMax = mNbCols-1;

        mCells = new Cell[mNbLines, mNbCols];

        // init the cells
        for(int i=iMax; i>=0; i--){
            for(int j=0; j<=jMax; j++){
                int curX = i*mCellWidth;
                int curZ = j*mCellDepth;
                mCells[i,j] = new Cell(curX, curZ, mCellWidth, mCellDepth);

                Color cellColor = Random.ColorHSV();
                cellColor.a = 1.0f;
                mCells[i,j].mColor = cellColor;

                Debug.DrawLine(new Vector3(curX, 0, curZ), new Vector3(curX+mCellWidth, 0, curZ), cellColor, 200.0f, false);
                Debug.DrawLine(new Vector3(curX, 0, curZ), new Vector3(curX, 0, curZ+mCellDepth), cellColor, 200.0f, false);
            }
        }

        // init neighbouring cells
        for(int i=iMax; i>=0; i--){
            for(int j=0; j<=jMax; j++){
                if(i>0 && j>0) mCells[i,j].mNeighbours.Add(mCells[i-1,j-1]);
                if(i>0) mCells[i,j].mNeighbours.Add(mCells[i-1,j]);
                if(i>0 && j<jMax) mCells[i,j].mNeighbours.Add(mCells[i-1,j+1]);

                if(j>0) mCells[i,j].mNeighbours.Add(mCells[i,j-1]);
                if(j<jMax) mCells[i,j].mNeighbours.Add(mCells[i,j+1]);
                
                if(i<iMax && j>0) mCells[i,j].mNeighbours.Add(mCells[i+1,j-1]);
                if(i<iMax) mCells[i,j].mNeighbours.Add(mCells[i+1,j]);
                if(i<iMax && j<jMax) mCells[i,j].mNeighbours.Add(mCells[i+1,j+1]);
            }
        }
    }
}