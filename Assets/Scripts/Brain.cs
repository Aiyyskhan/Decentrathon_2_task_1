using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brain
{
    private int inputSize;
    private int outputSize;
    private int hiddenSize;
    
    private float[][] iWeights;
    private float[][] hWeights;
    private float[][] oWeights;
    private float[] hBiases;
    private float[] potentialArr;
    private float[] outputLayer;

    public Brain(Dictionary<string, float[][]> chromo)
    {
        iWeights = chromo["iWeights"];
        hWeights = chromo["hWeights"];
        oWeights = chromo["oWeights"];
        hBiases = chromo["hBiases"][0];

        inputSize = iWeights.Length;
        hiddenSize = hWeights.Length;
        outputSize = oWeights[0].Length;
        
        potentialArr = new float[hiddenSize];
        outputLayer = new float[outputSize];

        for (int i = 0; i < hiddenSize; i++)
        {
            potentialArr[i] = 0.0f;
        }
        // Debug.Log("Brain created");
    }

    private float Tanh(float x)
    {
        // return (float)((Mathf.Exp(2.0f * x) - 1.0f) / (Mathf.Exp(2.0f * x) + 1.0f));
        return (float)((Mathf.Exp(x) - Mathf.Exp(-x)) / (Mathf.Exp(x) + Mathf.Exp(-x)));
    }

    private float Sigmoid(float x)
    {
        return 1.0f / (1.0f + Mathf.Exp(-x));
    }

    public float[] Forward(float[] inputs)
    {
        

        // Рассчет значений для входного слоя
        for (int j = 0; j < hiddenSize; j++)
        {
            for (int i = 0; i < inputSize; i++)
            {
                potentialArr[j] += inputs[i] * iWeights[i][j];
            }
        }

        // Рассчет значений для скрытого слоя
        for (int j = 0; j < hiddenSize; j++)
        {
            for (int i = 0; i < hiddenSize; i++)
            {
                potentialArr[j] += Sigmoid(potentialArr[i]) * hWeights[i][j];
            }
            potentialArr[j] += hBiases[j];
        }

        // Debug.Log($"Activations: [{string.Join(",", activations)}]");

        // Расчет значений для выходного слоя
        for (int j = 0; j < outputSize; j++)
        {
            outputLayer[j] = 0.0f;
            for (int i = 0; i < hiddenSize; i++)
            {
                outputLayer[j] += Sigmoid(potentialArr[i]) * oWeights[i][j];
            }
            outputLayer[j] = Sigmoid(outputLayer[j]);
        }

        // Debug.Log($"NN output: [{string.Join(",", outputLayer)}]");

        return outputLayer;
    }
}
