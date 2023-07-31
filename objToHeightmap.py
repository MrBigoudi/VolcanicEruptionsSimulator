from PIL import Image
import numpy as np
import sys
import math

def getValue(mat, kernel, factor):
    sum = 0
    for i in range(kernel.shape[0]):
        for j in range(kernel.shape[1]):
            sum += mat[i,j]*kernel[i,j]
    return factor*sum

def convolve(arr, kernel, factor):
    # get dimensions
    kernelY, kernelX = kernel.shape
    arrY, arrX = arr.shape
    newArr = arr.copy()
    delta = kernelY//2
    # make array bigger
    tmpArr = np.pad(arr, pad_width=delta)
    # do convolution
    for j in range(delta, arrY+delta):
        for i in range(delta, arrX+delta):
            matTmp = tmpArr[j-delta:j-delta+kernelY, i-delta:i-delta+kernelX]
            if(newArr[j-delta][i-delta] == 0):
                newArr[j-delta][i-delta] = getValue(matTmp, kernel, factor)
    return newArr


def gaussianBlur(arr, nbPasses):
    # the gaussian matrix
    gaussMatrix = np.matrix([
                [1,  4,  7,  4,  1],
                [4, 16, 26, 16,  4],
                [7, 26, 41, 26,  7],
                [4, 16, 26, 16,  4],
                [1,  4,  7,  4,  1]])
    gaussFactor = 1/273
    # create the new image by convolution
    for i in range(nbPasses):
        arr = convolve(arr, gaussMatrix, gaussFactor)
    return arr

def getNbVertices(file):
    max = 0
    for line in file:
        if(line[0] != 'v' or line[1] != ' '):
            continue
        arr = line.split()
        val1 = math.floor(float(arr[1]))
        val2 = math.floor(float(arr[len(arr)-1]))
        if(max < val1):
            max = val1
        if(max < val2):
            max = val2
    return (max+1) << 1
    
def getValues(file, dim):
    sumVal = np.zeros((dim, dim))
    nbVal = np.zeros((dim, dim))
    max = 0

    # get all the values
    halfDim = dim >> 1
    for line in file:
        if(line[0] != 'v' or line[1] != ' '):
            continue
        arr = line.split()
        # get coords
        x = halfDim + math.floor(float(arr[1]))
        z = halfDim + math.floor(float(arr[len(arr)-1]))
        
        if(x >= dim):
            x = dim-1
        if(x < 0):
            x = 0
        if(z >= dim):
            z = dim-1
        if(z < 0):
            z = 0

        # get y value
        y = float(arr[2])
        # print(x, y, z)
        sumVal[x,z] += y
        nbVal[x,z] += 1
        
        
    # get the medium values
    for i in range(dim):
        for j in range(dim):
            # print(nbVal[i,j])
            realVal = sumVal[i,j]
            if(nbVal[i,j] != 0):
                realVal = sumVal[i,j] / nbVal[i,j]
            if(max < realVal):
                max = realVal
            sumVal[i,j] = realVal

    # normalize values
    for i in range(dim):
        for j in range(dim):
            sumVal[i,j] *= (255.0/max)

    return sumVal
        

def objToHeightmap(file):
    dim = abs(getNbVertices(file))
    # print(dim)
    file.seek(0)
    return gaussianBlur(getValues(file, dim), 5)

def main():
    # get file path
    filePath = sys.argv[1]
    # open obj file
    file = open(filePath, "r")
    # convert the obj to a np array
    arr = objToHeightmap(file)
    # back to the image
    image = Image.fromarray(arr)
    if image.mode != 'RGB':
        image = image.convert('RGB')
    # save the image
    image.save("new_image.png")
    file.close()

if __name__ == "__main__":
    main()