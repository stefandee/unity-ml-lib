using System;
using UnityEngine;

namespace PironGames.MLLib.NN
{
    public enum ActivationFunction
    {
        Identity,
        BinaryStep,
        Sigmoid,
        Tanh,
        ReLU
    }

    public static class NNActivationFunctions
    {
        public static Func<float, float> Identity = (x) => x;
        public static Func<float, float> BinaryStep = (x) => x < 0 ? 0 : 1;
        public static Func<float, float> Sigmoid = (x) => 1 / (1 + Mathf.Exp(-x));
        public static Func<float, float> Tanh = (x) => (Mathf.Exp(x) - Mathf.Exp(-x)) / (Mathf.Exp(x) + Mathf.Exp(-x));
        public static Func<float, float> ReLU = (x) => Mathf.Max(0, x);

        public static Func<float, float> FunctionFromEnum(ActivationFunction func)
        {
            switch(func)
            {
                case ActivationFunction.Identity:
                    return Identity;

                case ActivationFunction.BinaryStep:
                    return BinaryStep;

                case ActivationFunction.Sigmoid:
                    return Sigmoid;

                case ActivationFunction.Tanh:
                    return Tanh;

                case ActivationFunction.ReLU:
                    return ReLU;
            }

            return null;
        }
    }
}