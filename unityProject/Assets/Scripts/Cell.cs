using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell {

    public ArrayList mParticles = new ArrayList();
    public int mX;
    public int mY;
    public int mHeight;
    public int mWidth;

    public Cell(int x, int y, int height, int width){
        mX = x;
        mY = y;
        mHeight = height;
        mWidth = width;
    }
}