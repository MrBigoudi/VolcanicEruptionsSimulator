from PIL import Image
import numpy as np
import sys

def convolve(arr, kernel):
    # make array bigger
    tmpArr = np.pad(arr, pad_width=1)
    # get dimensions
    kernelY, kernelX = kernel.shape
    arrY, arrX = arr.shape
    tmpArrY, tmpArrX = tmpArr.shape
    newArr = np.zeros((tmpArrY, tmpArrX))

    delta = kernelY//2

    # do convolution
    for j in range(delta, arrY+delta+1):
        for i in range(delta, arrX+delta+1):
            newArr[j][i] = np.sum(tmpArr[j-delta:j-delta+kernelY, i-delta:i-delta+kernelX]*kernel)

    return newArr

def main():
    # get file path
    filePath = sys.argv[1]
    # open image
    image = Image.open(filePath)
    # convert the image to np array
    arr = np.asarray(image)[:,:,0]

    # the gaussian matrix
    gauss = (1/273) * np.matrix([
                [1,  4,  7,  4,  1],
                [4, 16, 26, 16,  4],
                [7, 26, 41, 26,  7],
                [4, 16, 26, 16,  4],
                [1,  4,  7,  4,  1]])
    # create the new image by convolution
    arr = convolve(arr, gauss)
    # back to the image
    image = Image.fromarray(arr)
    if image.mode != 'RGB':
        image = image.convert('RGB')
    # save the image
    image.save("new_"+filePath)

if __name__ == "__main__":
    main()