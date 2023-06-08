import numpy as np

def RBF_kernel(nRow, mRow, kernelHyperparameter = 1):
    """
    Inputs:
        xn: row n of x
        xm: row m of x
        l:  kernel hyperparameter, set to 1 by default
    Outputs:
        K:  kernel matrix element: K[n, m] = k(xn, xm)
    """
    kernelMatrixElement = np.exp(-np.linalg.norm(nRow - mRow)**2 / (2 * kernelHyperparameter**2))
    return kernelMatrixElement

def make_RBF_kernel(inputRows, kernelHyperparameter = 1, sigma = 0):
    """
    Inputs:
        X: set of φ rows of inputs
        l: kernel hyperparameter, set to 1 by default
        sigma: Gaussian noise std dev, set to 0 by default
    Outputs:
        K:  Covariance matrix 
    """
    covarianceMatrix = np.zeros([len(inputRows), len(inputRows)])
    for i in range(len(inputRows)):
        for j in range(len(inputRows)):
            covarianceMatrix[i, j] = RBF_kernel(inputRows[i], inputRows[j], kernelHyperparameter)
    return covarianceMatrix + sigma * np.eye(len(covarianceMatrix))

def gaussian_process_predict_mean(inputRows, observations, newInputRows):
    """
    Inputs:
        X: set of φ rows of inputs
        y: set of φ observations 
        X_new: new input 
    Outputs:
        y_new: predicted target corresponding to X_new
    """
    rbf_kernel = make_RBF_kernel(np.vstack([inputRows, newInputRows]))
    K = rbf_kernel[:len(inputRows), :len(inputRows)]
    k = rbf_kernel[:len(inputRows), -1]
    return  np.dot(np.dot(k, np.linalg.inv(K)), observations)

def gaussian_process_predict_std(inputRows, newInputRows):
    """
    Inputs:
        X: set of φ rows of inputs
        X_new: new input
    Outputs:
        y_std: std dev. corresponding to X_new
    """
    rbf_kernel = make_RBF_kernel(np.vstack([inputRows, newInputRows]))
    K = rbf_kernel[:len(inputRows), :len(inputRows)]
    k = rbf_kernel[:len(inputRows), -1]
    return rbf_kernel[-1,-1] - np.dot(np.dot(k,np.linalg.inv(K)),k)

# def f(x):
#     return (x-5) ** 2
# Training data x and y:
# X = np.array([1.0, 3.0, 5.0, 7.0, 9.0])
# y = f(X)
# X = X.reshape(-1, 1)

import pandas as pd

data = pd.read_csv('casedistribution.csv')
data = data[data['countriesAndTerritories'] == 'Poland']
data = data[['dateRep','cases']]
data['dateRep'] = pd.to_datetime(data['dateRep'], dayfirst=True)
data = data.sort_values(by='dateRep')


import matplotlib.pyplot as plt

days = np.linspace(1, 365, 729)
trainingDays = 50

X = np.arange(0, trainingDays)
X = X.reshape(-1, 1)
y_pred = []
#y_std = []
for i in range(len(days)):
    X_new = np.array([days[i]])
    y_pred.append(gaussian_process_predict_mean(X, data['cases'][:trainingDays], X_new))
    #y_std.append(np.sqrt(gaussian_process_predict_std(X, X_new)))
y_pred = np.array(y_pred)
#y_std = np.array(y_std)

plt.figure(figsize = (15, 5))
#plt.plot(x, f(x), "r")
plt.plot(X, data['cases'][:trainingDays], "ro")
plt.plot(np.arange(trainingDays, data.shape[0]), data['cases'][trainingDays:], "yo")
plt.plot(days, y_pred, "b-")
# plt.fill(np.hstack([x, x[::-1]]),
#          np.hstack([y_pred - 1.9600 * y_std, 
#                    (y_pred + 1.9600 * y_std)[::-1]]),
#          alpha = 0.5, fc = "b")
plt.xlabel("$day$", fontsize = 14)
plt.ylabel("$cases$", fontsize = 14)
plt.legend(["observations in training set", "real observations", "predictions"], fontsize = 10)
plt.grid(True)
plt.xticks(fontsize = 10)
plt.yticks(fontsize = 10)
plt.show()