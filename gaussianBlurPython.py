from PIL import Image
import numpy as np
import sys

def getValue(mat, kernel, factor):
    sum = 0
    for i in range(kernel.shape[0]):
        for j in range(kernel.shape[1]):
            sum += mat[i,j]*kernel[i,j]
    return factor*sum

def convolve(arr, kernel, factor):
    # get dimensions
    kernelY, kernelX = kernel.shape
    arrY, arrX, arrZ = arr.shape
    newArr = arr.copy()
    delta = kernelY//2
    # make array bigger
    tmpArr = np.pad(arr, pad_width=delta)
    # do convolution
    for j in range(delta, arrY+delta):
        for i in range(delta, arrX+delta):
            for k in range(arrZ):
                matTmp = tmpArr[j-delta:j-delta+kernelY, i-delta:i-delta+kernelX, k]
                newArr[j-delta][i-delta][k] = getValue(matTmp, kernel, factor)
    return newArr


def main():
    # get file path
    filePath = sys.argv[1]
    nbPass = 1
    if(len(sys.argv) > 2 ):
        nbPass = sys.argv[2]
    # open image
    image = Image.open(filePath)
    # convert the image to np array
    arr = np.asarray(image)
    # the gaussian matrix
    gaussMatrix = np.matrix([
                [1,  4,  7,  4,  1],
                [4, 16, 26, 16,  4],
                [7, 26, 41, 26,  7],
                [4, 16, 26, 16,  4],
                [1,  4,  7,  4,  1]])
    gaussFactor = 1/273
    # create the new image by convolution
    for i in range(nbPass):
        arr = convolve(arr, gaussMatrix, gaussFactor)
    # back to the image
    image = Image.fromarray(arr)
    if image.mode != 'RGB':
        image = image.convert('RGB')
    # save the image
    image.save("new_"+filePath)

if __name__ == "__main__":
    main()