import matplotlib.pyplot as plt 
import numpy as np 
import math
    
xdata = [i/100.0 for i in range(0, 201, 1)] 
ydataPos = [4.72124, 4.02846, 3.62353, 3.33654, 
            3.11423, 2.93288, 2.77982, 2.64749, 
            2.53102, 2.42707, 2.33327, 2.24786, 
            2.1695, 2.09717, 2.03003, 1.96742, 
            1.9088, 1.85371, 1.80179, 1.7527, 
            1.70619, 1.662, 1.61994, 1.57983, 
            1.54151, 1.50484, 1.46971, 1.436, 
            1.40361, 1.37246, 1.34247, 1.31356, 
            1.28567, 1.25873, 1.23271, 1.20754, 
            1.18317, 1.15958, 1.13671, 1.11453, 
            1.09301, 1.07212, 1.05182, 1.03209, 
            1.01291, 0.994256, 0.976098, 0.958418, 
            0.941198, 0.924419, 0.908064, 0.892116, 
            0.87656, 0.861382, 0.846568, 0.832105, 
            0.81798, 0.804182, 0.7907, 0.777522, 
            0.76464, 0.752042, 0.739721, 0.727668, 
            0.715873, 0.70433, 0.69303, 0.681966, 
            0.671132, 0.66052, 0.650124, 0.639938, 
            0.629956, 0.620173, 0.610582, 0.60118, 
            0.591959, 0.582917, 0.574048, 0.565347, 
            0.556811, 0.548434, 0.540214, 0.532146, 
            0.524226, 0.516451, 0.508818, 0.501322, 
            0.49396, 0.48673, 0.479629, 0.472652, 
            0.465798, 0.459064, 0.452447, 0.445944, 
            0.439552, 0.43327, 0.427095, 0.421024, 
            0.415056, 0.409188, 0.403418, 0.397743, 
            0.392163, 0.386674, 0.381276, 0.375966, 
            0.370742, 0.365602, 0.360546, 0.355571, 
            0.350675, 0.345858, 0.341117, 0.336451, 
            0.331858, 0.327338, 0.322888, 0.318508, 
            0.314196, 0.309951, 0.305771, 0.301655, 
            0.297603, 0.293613, 0.289683, 0.285813, 
            0.282002, 0.278248, 0.27455, 0.270908, 
            0.267321, 0.263787, 0.260305, 0.256876, 
            0.253497, 0.250167, 0.246887, 0.243655, 
            0.24047, 0.237332, 0.234239, 0.231191, 
            0.228188, 0.225227, 0.22231, 0.219434, 
            0.216599, 0.213806, 0.211052, 0.208337, 
            0.20566, 0.203022, 0.200421, 0.197857, 
            0.195329, 0.192836, 0.190378, 0.187955, 
            0.185565, 0.183209, 0.180886, 0.178594, 
            0.176335, 0.174107, 0.171909, 0.169742, 
            0.167604, 0.165496, 0.163417, 0.161366, 
            0.159343, 0.157348, 0.15538, 0.153438, 
            0.151523, 0.149634, 0.14777, 0.145931, 
            0.144117, 0.142328, 0.140562, 0.13882, 
            0.137102, 0.135406, 0.133733, 0.132082, 
            0.130453, 0.128846, 0.12726, 0.125695, 
            0.124151, 0.122627, 0.121123, 0.119638, 
            0.118174, 0.116728, 0.115302, 0.113894]

# ydataNeg = [ydataPos[len(ydataPos)-i-1] for i in range(len(ydataPos))]
ydata = [1000] + ydataPos

# print(len(xdata))
# print(len(ydata))

# wpoly6
def wPoly6(r, h):
    if (0<=r and r<=h):
        return (4/(math.pi*h**8))*((h*h-r*r)**3)
    return 0
ypoly6 = [wPoly6(i/100.0, 1) for i in range(0, 201, 1)]
ypoly6_2 = [wPoly6(i/100.0, 2) for i in range(0, 201, 1)]
ypoly6_3 = [wPoly6(i/100.0, 0.5) for i in range(0, 201, 1)]

# wspiky
def wSpiky(r, h):
    if (0<=r and r<=h):
        return (10/(math.pi*h**5))*((h-r)**3)
    return 0
yspiky = [wSpiky(i/100.0, 1) for i in range(0, 201, 1)]
yspiky_2 = [wSpiky(i/100.0, 2) for i in range(0, 201, 1)]
yspiky_3 = [wSpiky(i/100.0, 0.5) for i in range(0, 201, 1)]

# wvisc
def wVisc(r, h):
    if (0<r and r<=h):
        tmp = 4*r**3 + 9*r**2*h - 5*h**3 + 6*h**3*(math.log(h) - math.log(r)) 
        return (10/(9*math.pi*h**5))*tmp
    return 0
yvisc = [wVisc(i/100.0, 1) for i in range(0, 201, 1)]
yvisc_2 = [wVisc(i/100.0, 2) for i in range(0, 201, 1)]
yvisc_3 = [wVisc(i/100.0, 0.5) for i in range(0, 201, 1)]

def besselSerie(r, h):
    if(r<=0):
        return 0
    gamma = 0.577216
    return (2/(math.pi*h**2))*((-math.log(r)-gamma+math.log(2)) + (1/4.0)*r**2*(-math.log(r)-gamma+1+math.log(2)))# + (1/128.0)*r**4*(-2*math.log(r)-2*gamma+3+math.log(4)))
yserie = [besselSerie(i/100.0, 1) for i in range(0, 201, 1)]
yserie_2 = [besselSerie(i/100.0, 2) for i in range(0, 201, 1)]
yserie_3 = [besselSerie(i/100.0, 0.5) for i in range(0, 201, 1)]

# print(yspiky)
    
# plot the data 
# plt.plot(xdata, ydata, color ='tab:blue')
# plt.plot(xdata, yserie_3, color ='tab:green')
# plt.plot(xdata, yserie, color ='tab:purple')
# plt.plot(xdata, yserie_2, color ='tab:orange')
# plt.plot(xdata, ypoly6_3, color ='tab:green')
# plt.plot(xdata, ypoly6, color ='tab:purple')
# plt.plot(xdata, ypoly6_2, color ='tab:orange')
# plt.plot(xdata, yspiky_3, color ='tab:green')
# plt.plot(xdata, yspiky, color ='tab:purple')
# plt.plot(xdata, yspiky_2, color ='tab:orange')
plt.plot(xdata, yvisc_3, color ='tab:green')
plt.plot(xdata, yvisc, color ='tab:purple')
plt.plot(xdata, yvisc_2, color ='tab:orange')

plt.legend(["l=0.5", "l=1", "l=2"], loc ="lower right")
    
    
# set the limits 
plt.xlim([0.0, 2.0]) 
plt.ylim([0.0, 5.0]) 
  
# plt.title('matplotlib.pyplot.plot() example 2') 
    
# display the plot 
plt.show()